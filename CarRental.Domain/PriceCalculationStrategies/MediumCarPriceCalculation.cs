namespace CarRental.Domain.PriceCalculationStrategies;

public class MediumCarPriceCalculation : IPriceCalculationStrategy
{
    public decimal CalculatePrice(decimal baseDayRental, decimal baseKmPrice, int numberOfDays, int numberOfKm)
    {
        return baseDayRental * numberOfDays * 1.3m + baseKmPrice * numberOfKm;
    }
}