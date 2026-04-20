using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAllPatients;

public record GetAllPatientsQuery : IRequest<Result<IReadOnlyList<PatientDto>>>;
