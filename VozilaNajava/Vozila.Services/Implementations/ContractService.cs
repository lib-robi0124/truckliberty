using Vozila.DataAccess.Interfaces;
using Vozila.Domain.Enums;
using Vozila.Domain.Models;
using Vozila.Services.Interfaces;
using Vozila.ViewModels.Models;

namespace Vozila.Services.Implementations
{
    public class ContractService : IContractService
    {
        private readonly IContractRepository _contractRepository;
        private readonly ITransporterRepository _transporterRepository;
        private readonly IDestinationRepository _destinationRepository;

        public ContractService(
            IContractRepository contractRepository,
            ITransporterRepository transporterRepository,
            IDestinationRepository destinationRepository)
        {
            _contractRepository = contractRepository;
            _transporterRepository = transporterRepository;
            _destinationRepository = destinationRepository;
        }

        // --------------------------------------------------------
        // Helpers
        // --------------------------------------------------------
        private ContractVM MapToVM(Contract c)
        {
            return new ContractVM
            {
                Id = c.Id,
                ContractNumber = c.ContractNumber,
                TransporterId = c.TransporterId,
                TransporterName = c.Transporter?.CompanyName ?? "",
                CreatedDate = c.CreatedDate,
                ValidUntil = c.ValidUntil,
                DaysUntilExpiry = (c.ValidUntil - DateTime.Now).Days,
                IsActive = c.ValidUntil > DateTime.Now
            };
        }

        private ContractListVM MapToListVM(Contract c)
        {
            return new ContractListVM
            {
                Id = c.Id,
                ContractNumber = c.ContractNumber,
                TransporterName = c.Transporter?.CompanyName ?? "",
                CreatedDate = c.CreatedDate,
                ValidUntil = c.ValidUntil,
                IsActive = c.ValidUntil > DateTime.Now,
                DestrinationCount = c.Destinations != null ? c.Destinations.Count : 0
            };
        }

        private ContractDetailsVM MapToDetailsVM(Contract contract)
        {
            return new ContractDetailsVM
            {
                Id = contract.Id,
                ContractNumber = contract.ContractNumber,
                TransporterName = contract.Transporter?.CompanyName ?? "",
                TransporterEmail = contract.Transporter?.ContactPerson ?? "",
                CreatedDate = contract.CreatedDate,
                ValidUntil = contract.ValidUntil,
                IsActive = contract.ValidUntil > DateTime.Now,
                Destinations = contract.Destinations?
                    .Select(c => new DestinationVM
                    {
                        Id = c.Id,
                        ContractId = contract.Id,
                        ContractOilPrice = c.ContractOilPrice,
                        DestinationCount = c.Contract.Destinations?.Count ?? 0
                    })
                    .ToList()
            };
        }

        // --------------------------------------------------------
        // CRUD
        // --------------------------------------------------------
        public async Task<ContractVM?> GetByIdAsync(int id)
        {
            var contract = await _contractRepository.GetContractWithTransporterAsync(id);
            if (contract == null) return null;
            return MapToVM(contract);
        }

        public async Task<IEnumerable<ContractListVM>> GetAllAsync()
        {
            var list = await _contractRepository.GetAllAsync();
            return list.Select(MapToListVM);
        }

        public async Task<ContractDetailsVM?> GetDetailsAsync(int id)
        {
            var contract = await _contractRepository.GetContractWithDestinationsAsync(id);
            if (contract == null) return null;
            return MapToDetailsVM(contract);
        }

        public async Task<ContractVM> CreateAsync(ContractVM model)
        {
            // Business rule: One contract per transporter
            bool alreadyHasContract = await TransporterHasContractAsync(model.TransporterId);
            if (alreadyHasContract)
                throw new InvalidOperationException("Transporter already has a contract.");

            var contract = new Contract
            {
                ContractNumber = model.ContractNumber,
                TransporterId = model.TransporterId,
                CreatedDate = DateTime.Now,
                ValidUntil = DateTime.Now.AddYears(1)
            };

            var result = await _contractRepository.AddAsync(contract);
            return MapToVM(result);
        }

        public async Task UpdateAsync(ContractVM model)
        {
            var contract = await _contractRepository.GetByIdAsync(model.Id)
                ?? throw new Exception("Contract not found");

            contract.ContractNumber = model.ContractNumber;
            contract.ValidUntil = model.ValidUntil;

            await _contractRepository.UpdateAsync(contract);
        }

        public async Task DeleteAsync(int id)
        {
            await _contractRepository.DeleteAsync(id);
        }

        // --------------------------------------------------------
        // Business operations
        // --------------------------------------------------------
        public async Task<IEnumerable<ContractListVM>> GetContractsByTransporterAsync(int transporterId)
        {
            var list = await _contractRepository.GetContractsByTransporterAsync(transporterId);
            return list.Select(MapToListVM);
        }

        public async Task<IEnumerable<ContractListVM>> GetExpiringContractsAsync(int daysThreshold)
        {
            DateTime threshold = DateTime.Now.AddDays(daysThreshold);
            var list = await _contractRepository.GetExpiringContractsAsync(threshold);

            return list.Select(MapToListVM);
        }

        public async Task<bool> TransporterHasContractAsync(int transporterId)
        {
            var list = await _contractRepository.GetContractsByTransporterAsync(transporterId);
            return list.Any();
        }

        // --------------------------------------------------------
        // CONDITION LOGIC  (One Condition per Contract)
        // --------------------------------------------------------
        public async Task<Destination?> GetDestinationForContractAsync(int contractId)
        {
            var contract = await _contractRepository.GetContractWithDestinationsAsync(contractId);
            return contract?.Destinations?.FirstOrDefault();
        }

        public async Task AssignDestinationToContractAsync(int contractId, Destination destination)
        {
            var contract = await _contractRepository.GetContractWithDestinationsAsync(contractId)
                ?? throw new Exception("Contract not found");

            // Business rule: Only one Condition allowed
            if (contract.Destinations.Any())
                throw new InvalidOperationException("Contract already has a Destination.");

            destination.ContractId = contractId;  // FK assignment
            await _destinationRepository.AddAsync(destination);
        }
        // Contract Price Retrieval based on Destination
        public async Task<decimal?> GetPriceForDestinationAsync(int contractId, int cityId, string destinationType)
        {
            // Convert int to City enum
            if (!Enum.IsDefined(typeof(City), cityId))
                return null;

            var city = (City)cityId;

            var contract = await _contractRepository.GetContractWithDestinationsAsync(contractId);
            if (contract == null || contract.ValidUntil < DateTime.Now)
                return null;

            // Since there's only one condition per contract, get the first one
            var destination = contract.Destinations.FirstOrDefault(d => d.City == city);
            if (destination == null)
                return null;

            return destination?.DestinationContractPrice;
        }
    }

}
