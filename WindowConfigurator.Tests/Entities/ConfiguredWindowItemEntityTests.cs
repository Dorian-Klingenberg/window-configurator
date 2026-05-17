using WindowConfigurator.Data.Entities;

namespace WindowConfigurator.Tests.Entities
{
    public class ConfiguredWindowItemEntityTests
    {
        [Fact]
        public void NewItem_HasUniqueId()
        {
            var a = new ConfiguredWindowItemEntity();
            var b = new ConfiguredWindowItemEntity();
            Assert.NotEqual(a.Id, b.Id);
        }

        [Fact]
        public void NewItem_StartsAsDraft()
        {
            var item = new ConfiguredWindowItemEntity();
            Assert.Equal(ConfiguredWindowItemStatus.Draft, item.Status);
        }

        [Fact]
        public void NewItem_RoughOpeningIsNull()
        {
            // Rough opening is null until a contractor remeasure visit populates it.
            var item = new ConfiguredWindowItemEntity();
            Assert.Null(item.RoughOpeningWidthDecimal);
            Assert.Null(item.RoughOpeningHeightDecimal);
        }

        [Fact]
        public void NewItem_AuthoritativePriceIsNull()
        {
            // Price is null until the server pricing service runs (Phase 4).
            var item = new ConfiguredWindowItemEntity();
            Assert.Null(item.AuthoritativePrice);
        }

        [Fact]
        public void NewItem_ConfigurationJsonIsNull()
        {
            // No configuration snapshot until the configurator POSTs one.
            var item = new ConfiguredWindowItemEntity();
            Assert.Null(item.ConfigurationJson);
        }

        [Fact]
        public void Item_CanBeLinkedToSession()
        {
            var sessionId = Guid.NewGuid();
            var item = new ConfiguredWindowItemEntity
            {
                QuoteSessionId = sessionId,
                Location = "Master Bedroom Left",
                LineItemNumber = 1,
                ProductLineKey = "energysaver-2500"
            };

            Assert.Equal(sessionId, item.QuoteSessionId);
            Assert.Equal("Master Bedroom Left", item.Location);
        }

        [Fact]
        public void Item_CanStoreAllThreeMeasurementPairs()
        {
            var item = new ConfiguredWindowItemEntity
            {
                FrameWidthDecimal = 70.25m,
                FrameHeightDecimal = 45.0625m,
                RoughOpeningWidthDecimal = 71.25m,
                RoughOpeningHeightDecimal = 46.0625m,
                OutsideMeasureWidthDecimal = 72.25m,
                OutsideMeasureHeightDecimal = 47.0625m
            };

            Assert.Equal(70.25m, item.FrameWidthDecimal);
            Assert.Equal(71.25m, item.RoughOpeningWidthDecimal);
            Assert.Equal(72.25m, item.OutsideMeasureWidthDecimal);
        }
    }
}
