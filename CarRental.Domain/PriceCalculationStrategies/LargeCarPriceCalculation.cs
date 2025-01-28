namespace CarRental.Domain.PriceCalculationStrategies;

public class LargeCarPriceCalculation : IPriceCalculationStrategy
{
    public decimal CalculatePrice(decimal baseDayRental, decimal baseKmPrice, int numberOfDays, int numberOfKm)
    {
        return baseDayRental * numberOfDays * 1.5m + baseKmPrice * numberOfKm;
    }
}