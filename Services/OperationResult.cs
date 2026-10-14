namespace FootballTrainingManagement.Services;

// Simple result object returned by the services so the UI can
// show a clear success or error message without try/catch everywhere.
public record OperationResult(bool Success, string Message)
{
    public static OperationResult Ok(string message) => new(true, message);

    public static OperationResult Fail(string message) => new(false, message);
}
