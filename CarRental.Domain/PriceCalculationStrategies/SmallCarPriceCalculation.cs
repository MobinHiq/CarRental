namespace CarRental.Domain.PriceCalculationStrategies
{
    public class SmallCarPriceCalculation : IPriceCalculationStrategy
    {
        public decimal CalculatePrice(decimal baseDayRental, decimal baseKmPrice, int numberOfDays, int numberOfKm)
        {
            // Formula for Small Car: Price = baseDayRental * numberOfDays
            return baseDayRental * numberOfDays;
        }
    }
}