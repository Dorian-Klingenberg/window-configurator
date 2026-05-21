namespace WindowConfigurator.Data.Validation;

public sealed class CompletionValidationResult
{
    private CompletionValidationResult(IReadOnlyList<CompletionValidationError> errors)
    {
        Errors = errors;
    }

    public bool IsValid => Errors.Count == 0;
    public IReadOnlyList<CompletionValidationError> Errors { get; }

    public static CompletionValidationResult Success()
        => new([]);

    public static CompletionValidationResult Failed(List<CompletionValidationError> errors)
        => new(errors);
}
