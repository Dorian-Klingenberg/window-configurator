namespace WindowConfigurator.Data.Pricing;

public interface IPricingService
{
    /// <summary>
    /// Computes the authoritative server-side price for a configured window.
    /// The result should be persisted and the client-submitted price ignored.
    /// </summary>
    decimal CalculatePrice(WindowPricingInput input);
}
