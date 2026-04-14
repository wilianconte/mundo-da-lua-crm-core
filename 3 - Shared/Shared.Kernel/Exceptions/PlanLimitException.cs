namespace MyCRM.Shared.Kernel.Exceptions;

/// <summary>
/// Lançada quando uma ação viola o limite ou a restrição do plano do tenant.
/// </summary>
public sealed class PlanLimitException : Exception
{
    public string ErrorCode { get; }

    public PlanLimitException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}
