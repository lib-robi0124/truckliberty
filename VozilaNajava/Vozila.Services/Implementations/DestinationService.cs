using System.Collections.Generic;
using Vozila.DataAccess.Implementations;
using Vozila.DataAccess.Interfaces;
using Vozila.Domain.Models;
using Vozila.Services.Interfaces;
using Vozila.ViewModels.Models;

namespace Vozila.Services.Implementations
{
    public class DestinationService : IDestinationService
    {
        private readonly IDestinationRepository _destinationRepo;
        private readonly IRepository<Condition> _conditionRepo;
        private readonly IRepository<Contract> _contractRepo;
        private readonly IRepository<Transporter> _transporterRepo;

        public DestinationService(
            IDestinationRepository destinationRepo,
            IRepository<Condition> conditionRepo,
            IRepository<Contract> contractRepo,
            IRepository<Transporter> transporterRepo)
        {
            _destinationRepo = destinationRepo;
            _conditionRepo = conditionRepo;
            _contractRepo = contractRepo;
            _transporterRepo = transporterRepo;
        }

        // ------------------------------------------------------------
        // LIST ALL
        // ------------------------------------------------------------
        public async Task<IEnumerable<DestinationListVM>> GetAllActiveDestinationsAsync()
        {
            var list = await _destinationRepo.GetAllActiveAsync();

            return list.Select(d => new DestinationListVM
            {
                Id = d.Id,
                CityName = d.City.ToString(),
                CountryName = d.Country.ToString(),
                DestinationContractPrice = d.DestinationContractPrice,
                CalculatedPrice = d.DestinationPriceFromFormula,
                OrderCount = d.Orders?.Count ?? 0
            });
        }
        
        // ------------------------------------------------------------
        // BY CONDITION
        // ------------------------------------------------------------
        public async Task<IEnumerable<DestinationListVM>> GetByConditionAsync(int conditionId)
        {
            var list = await _destinationRepo.GetActiveDestinationsByContractAsync(conditionId);

            return list.Select(d => new DestinationListVM
            {
                Id = d.Id,
                CityName = d.City.ToString(),
                CountryName = d.Country.ToString(),
                DestinationContractPrice = d.DestinationContractPrice,
                CalculatedPrice = d.DestinationPriceFromFormula,
                OrderCount = d.Orders?.Count ?? 0
            });
        }

        // ------------------------------------------------------------
        // BY CONTRACT
        // ------------------------------------------------------------
        public async Task<IEnumerable<DestinationListVM>> GetByContractAsync(int contractId)
        {
            var list = await _destinationRepo.GetDestinationsByContractIdAsync(contractId);

            return list.Select(d => new DestinationListVM
            {
                Id = d.Id,
                CityName = d.City.ToString(),
                CountryName = d.Country.ToString(),
                DestinationContractPrice = d.DestinationContractPrice,
                CalculatedPrice = d.DestinationPriceFromFormula,
                OrderCount = d.Orders?.Count ?? 0
            });
        }

        // ------------------------------------------------------------
        // BY TRANSPORTER
        // ------------------------------------------------------------
        public async Task<IEnumerable<DestinationListVM>> GetByTransporterAsync(int transporterId)
        {
            var list = await _destinationRepo.GetByTransporterIdAsync(transporterId);

            return list.Select(d => new DestinationListVM
            {
                Id = d.Id,
                CityName = d.City.ToString(),
                CountryName = d.Country.ToString(),
                DestinationContractPrice = d.DestinationContractPrice,
                CalculatedPrice = d.DestinationPriceFromFormula,
                OrderCount = d.Orders?.Count ?? 0
            });
        }

        // ------------------------------------------------------------
        // DETAILS (Using FullDetails repo method)
        // ------------------------------------------------------------
        public async Task<DestinationDetailsVM?> GetDestinationDetailsAsync(int id)
        {
            var dest = await _destinationRepo.GetDestinationWithFullDetailsAsync(id);
            if (dest == null) return null;

            var contract = dest.Condition?.Contract;
            var transporter = contract?.Transporter;

            return new DestinationDetailsVM
            {
                Id = dest.Id,
                CityName = dest.City.ToString(),
                CountryName = dest.Country.ToString(),
                DestinationContractPrice = dest.DestinationContractPrice,
                DailyPricePerLiter = dest.DailyPricePerLiter,
                ContractOilPrice = dest.ContractOilPrice,
                CalculatedPrice = dest.DestinationPriceFromFormula,
                ContractNumber = contract?.ContractNumber ?? "N/A",
                TransporterName = transporter?.CompanyName ?? "N/A",
                IsContractActive = contract?.ValidUntil > DateTime.Now
            };
        }

        // ------------------------------------------------------------
        // GET SIMPLE VM
        // ------------------------------------------------------------
        public async Task<DestinationVM?> GetByIdAsync(int id)
        {
            var dest = await _destinationRepo.GetByIdAsync(id);
            if (dest == null) return null;

            return new DestinationVM
            {
                Id = dest.Id,
                City = dest.City,
                Country = dest.Country,
                CityName = dest.City.ToString(),
                CountryName = dest.Country.ToString(),
                DestinationContractPrice = dest.DestinationContractPrice,
                DailyPricePerLiter = dest.DailyPricePerLiter,
                ConditionId = dest.ConditionId,
                ContractOilPrice = dest.ContractOilPrice,
                CalculatedPrice = dest.DestinationPriceFromFormula
            };
        }

        // ------------------------------------------------------------
        // CREATE
        // ------------------------------------------------------------
        public async Task<DestinationVM> CreateAsync(DestinationVM model)
        {
            var entity = new Destination
            {
                City = model.City,
                Country = model.Country,
                DestinationContractPrice = model.DestinationContractPrice,
                DailyPricePerLiter = model.DailyPricePerLiter,
                ConditionId = model.ConditionId
            };

            await _destinationRepo.AddAsync(entity);

            model.Id = entity.Id;
            model.CalculatedPrice = entity.DestinationPriceFromFormula;

            return model;
        }

        // ------------------------------------------------------------
        // UPDATE
        // ------------------------------------------------------------
        public async Task UpdateAsync(DestinationVM model)
        {
            var entity = await _destinationRepo.GetByIdAsync(model.Id);
            if (entity == null) return;

            entity.City = model.City;
            entity.Country = model.Country;
            entity.DestinationContractPrice = model.DestinationContractPrice;
            entity.DailyPricePerLiter = model.DailyPricePerLiter;
            entity.ConditionId = model.ConditionId;

            await _destinationRepo.UpdateAsync(entity);
        }

        // ------------------------------------------------------------
        // DELETE
        // ------------------------------------------------------------
        public async Task DeleteAsync(int id)
        {
            await _destinationRepo.DeleteAsync(id);
        }

        // ------------------------------------------------------------
        // PRICE LOGIC FROM REPOSITORY
        // ------------------------------------------------------------
        public Task<decimal> GetCurrentPriceAsync(int destinationId)
            => _destinationRepo.GetCurrentDestinationPriceAsync(destinationId);
        public Task<Dictionary<int, decimal>> GetAllPricesForContractAsync(int contractId)
            => _destinationRepo.CalculateAllPricesForContractAsync(contractId);
        public Task UpdateOilPriceForAllAsync(decimal newDailyPrice)
            => _destinationRepo.UpdateAllDestinationOilPricesAsync(newDailyPrice);
        public Task UpdateOilPriceForContractAsync(int contractId, decimal newDailyPrice)
            => _destinationRepo.UpdateContractDestinationsOilPricesAsync(contractId, newDailyPrice);
    }
   
}




