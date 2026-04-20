using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetPatientById;

public record GetPatientByIdQuery(Guid Id) : IRequest<Result<PatientDto>>;
