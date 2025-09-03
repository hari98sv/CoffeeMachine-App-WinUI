namespace CoffeeMachine.Core.Common;

public class ErrorMessage
{
    public string Source
    {
        get;
    }
    public string Message
    {
        get;
    }
    public string StackTrace
    {
        get;
    }
    public DateTime Timestamp
    {
        get;
    }
    public string Severity
    {
        get;
    } // "Error", "Warning", "Critical", "Info"
    public string ErrorCode
    {
        get;
    }
    public Guid? CorrelationId
    {
        get;
    }
    public string Component
    {
        get;
    } // "UI", "Domain", "Infrastructure", "HSM", "Workflow"

    public ErrorMessage(string source, string message, string stackTrace = null,
                      string severity = "Error", string errorCode = null,
                      Guid? correlationId = null, string component = null)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        StackTrace = stackTrace;
        Severity = severity ?? "Error";
        ErrorCode = errorCode;
        CorrelationId = correlationId;
        Component = component;
        Timestamp = DateTime.UtcNow;
    }

    public static ErrorMessage CreateDomainError(string message, string errorCode = null, Guid? correlationId = null)
    {
        return new ErrorMessage("Domain", message, null, "Error", errorCode, correlationId, "Domain");
    }

    public static ErrorMessage CreateInfrastructureError(string message, Exception ex = null, string errorCode = null, Guid? correlationId = null)
    {
        return new ErrorMessage("Infrastructure", message, ex?.StackTrace, "Error", errorCode, correlationId, "Infrastructure");
    }

    public static ErrorMessage CreateHsmError(string message, string errorCode = null, Guid? correlationId = null)
    {
        return new ErrorMessage("HSM", message, null, "Error", errorCode, correlationId, "HSM");
    }

    public static ErrorMessage CreateWorkflowError(string message, Exception ex = null, string errorCode = null, Guid? correlationId = null)
    {
        return new ErrorMessage("Workflow", message, ex?.StackTrace, "Error", errorCode, correlationId, "Workflow");
    }

    public static ErrorMessage CreateWarning(string source, string message, string component = null)
    {
        return new ErrorMessage(source, message, null, "Warning", null, null, component);
    }

    public bool IsCritical => Severity.Equals("Critical", StringComparison.OrdinalIgnoreCase);
    public bool IsWarning => Severity.Equals("Warning", StringComparison.OrdinalIgnoreCase);
    public bool IsInformational => Severity.Equals("Info", StringComparison.OrdinalIgnoreCase);

    public override string ToString()
    {
        return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Severity}] {Source}: {Message}";
    }
}