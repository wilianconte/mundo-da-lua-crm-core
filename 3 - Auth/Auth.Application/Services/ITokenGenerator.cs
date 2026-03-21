using MyCRM.Auth.Domain.Entities;

namespace MyCRM.Auth.Application.Services;

public interface ITokenGenerator
{
    (string Token, DateTimeOffset ExpiresAt) Generate(User user);
}
