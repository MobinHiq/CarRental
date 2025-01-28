namespace CarRental.Domain.PriceCalculationStrategies;

public class MinivanPriceCalculation : IPriceCalculationStrategy
{
    public decimal CalculatePrice(decimal baseDayRental, decimal baseKmPrice, int numberOfDays, int numberOfKm)
    {
        return baseDayRental * numberOfDays * 1.7m + baseKmPrice * numberOfKm * 1.7m;
    }
}