using WindowConfigurator.Data.Entities;

namespace WindowConfigurator.Tests.Entities
{
    public class QuoteSessionEntityTests
    {
        [Fact]
        public void NewSession_HasUniqueId()
        {
            var a = new QuoteSessionEntity();
            var b = new QuoteSessionEntity();
            Assert.NotEqual(a.Id, b.Id);
        }

        [Fact]
        public void NewSession_StartsAsDraft()
        {
            var session = new QuoteSessionEntity();
            Assert.Equal(QuoteSessionStatus.Draft, session.Status);
        }

        [Fact]
        public void NewSession_StartsWithEmptyItemList()
        {
            var session = new QuoteSessionEntity();
            Assert.NotNull(session.Items);
            Assert.Empty(session.Items);
        }

        [Fact]
        public void NewSession_DefaultProductLineKeyIsNull()
        {
            var session = new QuoteSessionEntity();
            Assert.Null(session.DefaultProductLineKey);
        }

        [Fact]
        public void NewSession_MagicLinkFieldsAreNull()
        {
            var session = new QuoteSessionEntity();
            Assert.Null(session.MagicLinkToken);
            Assert.Null(session.MagicLinkExpiresAt);
        }

        [Fact]
        public void CrmInitiatedSession_HasExternalReferenceId_AndNoMagicLink()
        {
            var session = new QuoteSessionEntity
            {
                TenantId = Guid.NewGuid(),
                ExternalReferenceId = "CRM-OPP-00492",
                DefaultProductLineKey = "energysaver-2500"
            };

            Assert.NotNull(session.ExternalReferenceId);
            Assert.Null(session.MagicLinkToken);
        }

        [Fact]
        public void WebsiteInitiatedSession_HasMagicLinkAndEmail_AndNoExternalReference()
        {
            var session = new QuoteSessionEntity
            {
                TenantId = Guid.NewGuid(),
                CustomerEmail = "prospect@example.com",
                MagicLinkToken = Guid.NewGuid().ToString("N"),
                MagicLinkExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            Assert.NotNull(session.MagicLinkToken);
            Assert.NotNull(session.CustomerEmail);
            Assert.Null(session.ExternalReferenceId);
        }
    }
}
