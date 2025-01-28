namespace CarRental.Domain
{
    public interface IPriceCalculationStrategy
    {
        decimal CalculatePrice(decimal baseDayRental, decimal baseKmPrice, int numberOfDays, int numberOfKm);
    }
}