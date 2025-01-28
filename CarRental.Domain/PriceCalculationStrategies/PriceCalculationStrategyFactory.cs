using CarRental.Domain.Enums;
using CarRental.Domain.Interfaces;

namespace CarRental.Domain.PriceCalculationStrategies
{
    public class PriceCalculationStrategyFactory : IPriceCalculationStrategyFactory
    {
        public IPriceCalculationStrategy GetStrategy(CarCategory carCategory)
        {
            return carCategory switch
            {
                CarCategory.Small => new SmallCarPriceCalculation(),
                CarCategory.Medium => new MediumCarPriceCalculation(),
                CarCategory.Large => new LargeCarPriceCalculation(),
                CarCategory.Suv => new SuvPriceCalculation(),
                CarCategory.Minivan => new MinivanPriceCalculation(),
                _ => throw new ArgumentException("Invalid car category.")
            };
        }
    }
}