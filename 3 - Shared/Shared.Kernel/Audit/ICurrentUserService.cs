namespace MyCRM.Shared.Kernel.Audit;

public interface ICurrentUserService
{
    Guid? UserId { get; }
}
