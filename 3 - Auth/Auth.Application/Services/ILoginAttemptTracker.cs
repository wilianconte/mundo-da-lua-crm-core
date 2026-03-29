namespace MyCRM.Auth.Application.Services;

public interface ILoginAttemptTracker
{
    bool IsLockedOut(string key);
    void RecordFailure(string key);
    void ResetFailures(string key);
}
