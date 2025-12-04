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
        private readonly IConditionRepository _conditionRepository;

        public ContractService(
            IContractRepository contractRepository,
            ITransporterRepository transporterRepository,
            IConditionRepository conditionRepository)
        {
            _contractRepository = contractRepository;
            _transporterRepository = transporterRepository;
            _conditionRepository = conditionRepository;
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
                ValueEUR = c.ValueEUR,
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
                ValueEUR = c.ValueEUR,
                CreatedDate = c.CreatedDate,
                ValidUntil = c.ValidUntil,
                IsActive = c.ValidUntil > DateTime.Now,
                ConditionCount = c.Conditions?.Count ?? 0
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
                ValueEUR = contract.ValueEUR,
                CreatedDate = contract.CreatedDate,
                ValidUntil = contract.ValidUntil,
                IsActive = contract.ValidUntil > DateTime.Now,
                Conditions = contract.Conditions
                    .Select(c => new ConditionVM
                    {
                        Id = c.Id,
                        ContractId = contract.Id,
                        ContractOilPrice = c.ContractOilPrice,
                        DestinationCount = c.Destinations?.Count ?? 0
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
            var contract = await _contractRepository.GetContractWithConditionsAsync(id);
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
                ValueEUR = model.ValueEUR,
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
            contract.ValueEUR = model.ValueEUR;
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
        public async Task<Condition?> GetConditionForContractAsync(int contractId)
        {
            var contract = await _contractRepository.GetContractWithConditionsAsync(contractId);
            return contract?.Conditions?.FirstOrDefault();
        }

        public async Task AssignConditionToContractAsync(int contractId, Condition condition)
        {
            var contract = await _contractRepository.GetContractWithConditionsAsync(contractId)
                ?? throw new Exception("Contract not found");

            // Business rule: Only one Condition allowed
            if (contract.Conditions.Any())
                throw new InvalidOperationException("Contract already has a Condition.");

            condition.ContractId = contractId;  // FK assignment
            await _conditionRepository.AddAsync(condition);
        }
        // Contract Price Retrieval based on Destination
        public async Task<decimal?> GetPriceForDestinationAsync(int contractId, int cityId, string conditionType)
        {
            // Convert int to City enum
            if (!Enum.IsDefined(typeof(City), cityId))
                return null;

            var city = (City)cityId;

            var contract = await _contractRepository.GetContractWithConditionsAsync(contractId);
            if (contract == null || contract.ValidUntil < DateTime.Now)
                return null;

            // Since there's only one condition per contract, get the first one
            var condition = contract.Conditions.FirstOrDefault();
            if (condition == null)
                return null;

            var destination = condition.Destinations
                .FirstOrDefault(d => d.City == city);

            return destination?.DestinationContractPrice;
        }
    }

}
