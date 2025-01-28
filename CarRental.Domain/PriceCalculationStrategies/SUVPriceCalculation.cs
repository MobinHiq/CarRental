namespace CarRental.Domain.PriceCalculationStrategies;

public class SuvPriceCalculation : IPriceCalculationStrategy
{
    public decimal CalculatePrice(decimal baseDayRental, decimal baseKmPrice, int numberOfDays, int numberOfKm)
    {
        return baseDayRental * numberOfDays * 1.5m + baseKmPrice * numberOfKm * 1.5m;
    }
}