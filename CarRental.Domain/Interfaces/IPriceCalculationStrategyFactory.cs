using CarRental.Domain.Enums;

namespace CarRental.Domain.Interfaces
{
    public interface IPriceCalculationStrategyFactory
    {
        IPriceCalculationStrategy GetStrategy(CarCategory carCategory);
    }
}