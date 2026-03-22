using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Companies.DeleteCompany;

public record DeleteCompanyCommand(Guid Id) : IRequest<Result>;
