namespace WindowConfigurator.Controllers.Api.V1.Models
{
    public class ApiErrorResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public List<ApiValidationError> ValidationErrors { get; set; } = [];

        public static ApiErrorResponse Validation(string message, params ApiValidationError[] errors)
        {
            return new ApiErrorResponse
            {
                Code = "validation_error",
                Message = message,
                ValidationErrors = errors.ToList()
            };
        }

        public static ApiErrorResponse NotFound(string message)
        {
            return new ApiErrorResponse
            {
                Code = "not_found",
                Message = message
            };
        }
    }
}
