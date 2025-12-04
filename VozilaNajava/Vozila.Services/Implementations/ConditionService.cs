using AutoMapper;
using Vozila.DataAccess.Interfaces;
using Vozila.Domain.Enums;
using Vozila.Domain.Models;
using Vozila.Services.Interfaces;
using Vozila.ViewModels.Models;

namespace Vozila.Services.Implementations
{
    public class ConditionService : IConditionService
    {
        private readonly IConditionRepository _conditionRepository;
        private readonly IContractRepository _contractRepository;
        private readonly ITransporterRepository _transporterRepository;
        private readonly IDestinationRepository _destinationRepository;
        private readonly IMapper _mapper; // For VM mapping (AutoMapper)

        public ConditionService(
            IConditionRepository conditionRepository,
            IContractRepository contractRepository,
            ITransporterRepository transporterRepository,
            IDestinationRepository destinationRepository,
            IMapper mapper)
        {
            _conditionRepository = conditionRepository;
            _contractRepository = contractRepository;
            _transporterRepository = transporterRepository;
            _destinationRepository = destinationRepository;
            _mapper = mapper;
        }

        // Create condition for one contract (one transporter) with initial destinations
        public async Task<ConditionVM?> CreateConditionAsync(CreateConditionVM createVM)
        {
            // Validate: One contract per transporter, no existing condition
            var existingCondition = await _conditionRepository.GetConditionForContractAsync(createVM.ContractId);
            if (existingCondition != null)
                throw new InvalidOperationException("Contract already has a condition.");

            var contract = await _contractRepository.GetByIdAsync(createVM.ContractId);
            if (contract == null)
                throw new ArgumentException("Contract not found.");

            var condition = new Condition
            {
                ContractId = createVM.ContractId,
                ContractOilPrice = createVM.ContractOilPrice,
                Destinations = createVM.DestinationIds.Select(id => new Destination
                {
                    City = (City)id  // City enum cast from int ID
                }).ToList()
            };

            var savedCondition = await _conditionRepository.AddAsync(condition);
            return _mapper.Map<ConditionVM>(savedCondition);
        }

        // Update condition oil price or add destinations
        public async Task<ConditionVM> UpdateConditionAsync(UpdateConditionVM updateVM)
        {
            var condition = await _conditionRepository.GetFullConditionDetailsAsync(updateVM.Id);
            if (condition == null)
                throw new ArgumentException("Condition not found.");

            // Update oil price if provided
            if (updateVM.ContractOilPrice.HasValue)
                condition.ContractOilPrice = updateVM.ContractOilPrice.Value;

            // Add new destinations if provided
            if (updateVM.NewDestinationIds?.Any() == true)
            {
                var validAdditions = await _conditionRepository.CanAddDestinationToConditionAsync(condition.Id, updateVM.NewDestinationIds.First());
                if (!validAdditions)
                    throw new InvalidOperationException("Cannot add destinations to this condition.");

                foreach (var cityId in updateVM.NewDestinationIds)
                {
                    condition.Destinations.Add(new Destination { City = (City)cityId });
                }
            }

            await _conditionRepository.UpdateAsync(condition);
            return _mapper.Map<ConditionVM>(condition);
        }

        // Get condition summary for contract/transporter
        public async Task<ConditionVM?> GetConditionSummaryAsync(int contractId)
        {
            var condition = await _conditionRepository.GetConditionForContractAsync(contractId);
            return condition == null ? null : _mapper.Map<ConditionVM>(condition);
        }

        // Get best condition for city (lowest oil price among valid contracts)
        public async Task<ConditionVM?> GetBestConditionForCityAsync(int cityId)
        {
            var condition = await _conditionRepository.GetBestConditionForCityAsync(cityId);
            return _mapper.Map<ConditionVM>(condition);
        }
        // Delete condition by id
        public async Task<bool> DeleteConditionAsync(int id)
        {
                // Check if condition exists and can be deleted
                var condition = await _conditionRepository.GetByIdAsync(id);
                if (condition == null) return false;

                // Business rule: Check if contract is still active
                var contract = await _contractRepository.GetByIdAsync(condition.ContractId);
                if (contract?.ValidUntil < DateTime.Now)
                    throw new InvalidOperationException("Cannot delete condition for active contract");

                await _conditionRepository.DeleteAsync(id);
                return true;
        }
        // -------------------------------------------------------------
        // ADD DESTINATION TO CONDITION  (Validation included)
        // -------------------------------------------------------------
        public async Task<bool> AddDestinationToConditionAsync(int conditionId, int cityId)
        {
            // Check if allowed to add
            var canAdd = await _conditionRepository.CanAddDestinationToConditionAsync(conditionId, cityId);
            if (!canAdd) return false;

            var condition = await _conditionRepository.GetConditionWithDestinationsAsync(conditionId);
            if (condition == null) return false;

            // Create destination
            var destination = new Destination
            {
                ConditionId = conditionId,
                City = (City)cityId
            };

            condition.Destinations.Add(destination);

            await _conditionRepository.UpdateAsync(condition);
            return true;
        }
        // -------------------------------------------------------------
        // BUSINESS LOGIC: AVERAGE OIL PRICE FOR TRANSPORTER
        // -------------------------------------------------------------
        public async Task<decimal> GetAverageOilPriceForTransporterAsync(int transporterId)
        {
            return await _conditionRepository.CalculateAverageContractOilPriceAsync(transporterId);
        }

        // -------------------------------------------------------------
        // GET CONDITIONS BY TRANSPORTER
        // -------------------------------------------------------------
        public async Task<IEnumerable<ConditionVM>> GetConditionsByTransporterAsync(int transporterId)
        {
            var list = await _conditionRepository.GetConditionsByTransporterAsync(transporterId);

            return list.Select(c => new ConditionVM
            {
                Id = c.Id,
                ContractId = c.ContractId,
                ContractNumber = c.Contract.ContractNumber,
                ContractOilPrice = c.ContractOilPrice,
                DestinationCount = c.Destinations.Count
            });
        }
         // Reporting: Get all active conditions
        public async Task<IEnumerable<ConditionVM>> GetActiveConditionsAsync()
        {
                var conditions = await _conditionRepository.GetActiveConditionsAsync();
                var conditionVms = new List<ConditionVM>();

                foreach (var condition in conditions)
                {
                    var vm = await MapToConditionVM(condition);
                    conditionVms.Add(vm);
                }
                return conditionVms;
        }
        // Helper method to map Condition to ConditionVM with additional details
        public async Task<IEnumerable<ConditionVM>> GetConditionsExpiringWithinAsync(int days)
        {
                var timeSpan = TimeSpan.FromDays(days);
                var conditions = await _conditionRepository.GetConditionsExpiringWithinAsync(timeSpan);
                var conditionVms = new List<ConditionVM>();

                foreach (var condition in conditions)
                {
                    var vm = await MapToConditionVM(condition);
                    conditionVms.Add(vm);
                }
                return conditionVms;
        }

        // Helper method to map Condition to ConditionVM
        private async Task<ConditionVM> MapToConditionVM(Condition condition)
        {
            var contract = await _contractRepository.GetByIdAsync(condition.ContractId);
            var transporter = contract != null ? await _transporterRepository.GetByIdAsync(contract.TransporterId) : null;
            int destinationCount;

            if (condition.Destinations != null)
            {
                destinationCount = condition.Destinations.Count;
            }
            else
            {
                var destinations = await _destinationRepository.GetDestinationsByContractIdAsync(condition.ContractId);
                destinationCount = destinations?.Count ?? 0;
            }

            return new ConditionVM
            {
                Id = condition.Id,
                ContractId = condition.ContractId,
                ContractNumber = contract?.ContractNumber ?? string.Empty,
                ContractOilPrice = condition.ContractOilPrice,
                DestinationCount = destinationCount,
                
            };
        }

    }
}
