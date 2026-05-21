namespace WindowConfigurator.Data.Validation;

public sealed class CompletionValidationError
{
    public CompletionValidationError(string field, string message)
    {
        Field = field;
        Message = message;
    }

    public string Field { get; }
    public string Message { get; }
}
