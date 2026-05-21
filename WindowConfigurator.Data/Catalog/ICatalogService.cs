using WindowConfigurator.Data.Entities;

namespace WindowConfigurator.Data.Catalog
{
    public interface ICatalogService
    {
        /// <summary>Returns the catalog entry for the given product line key, or null if not found.</summary>
        ProductLineCatalog? GetProductLine(string key);

        /// <summary>Returns all registered product lines.</summary>
        IReadOnlyList<ProductLineCatalog> ListAll();

        /// <summary>
        /// Returns the product lines the given tenant is allowed to use.
        /// When AllowedProductLineKeys is empty, all product lines are returned (no restriction).
        /// </summary>
        IReadOnlyList<ProductLineCatalog> GetAllowedForTenant(TenantEntity tenant);
    }
}
