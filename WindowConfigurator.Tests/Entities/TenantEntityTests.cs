using WindowConfigurator.Data.Entities;

namespace WindowConfigurator.Tests.Entities
{
    public class TenantEntityTests
    {
        [Fact]
        public void NewTenant_HasUniqueId()
        {
            var a = new TenantEntity();
            var b = new TenantEntity();
            Assert.NotEqual(a.Id, b.Id);
        }

        [Fact]
        public void NewTenant_DefaultsToMixedProductLinesNotAllowed()
        {
            var tenant = new TenantEntity();
            Assert.False(tenant.MixedProductLinesAllowed);
        }

        [Fact]
        public void NewTenant_StartsWithEmptyAllowedProductLineKeys()
        {
            var tenant = new TenantEntity();
            Assert.NotNull(tenant.AllowedProductLineKeys);
            Assert.Empty(tenant.AllowedProductLineKeys);
        }

        [Fact]
        public void NewTenant_HasBrandingObject()
        {
            var tenant = new TenantEntity();
            Assert.NotNull(tenant.Branding);
        }
    }
}
