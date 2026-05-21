using System.Text.Json;
using WindowConfigurator.Data.Entities;

namespace WindowConfigurator.Data.Validation;

public interface ICompletionValidationService
{
    CompletionValidationResult Validate(
        JsonElement payload,
        string productLineKey,
        TenantEntity? tenant,
        string itemTemplateJson);
}
