using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Students.CreateStudent;

public sealed class CreateStudentHandler : IRequestHandler<CreateStudentCommand, Result<StudentDto>>
{
    private readonly IStudentRepository _repository;
    private readonly IPersonRepository  _personRepository;
    private readonly ITenantService     _tenant;

    public CreateStudentHandler(
        IStudentRepository repository,
        IPersonRepository personRepository,
        ITenantService tenant)
    {
        _repository       = repository;
        _personRepository = personRepository;
        _tenant           = tenant;
    }

    public async Task<Result<StudentDto>> Handle(CreateStudentCommand request, CancellationToken ct)
    {
        // Ensure the Person exists within this tenant
        var person = await _personRepository.GetByIdAsync(request.PersonId, ct);
        if (person is null)
            return Result<StudentDto>.Failure("PERSON_NOT_FOUND", "Person not found.");

        // Prevent duplicate student records for the same person within the tenant
        var alreadyEnrolled = await _repository.PersonAlreadyEnrolledAsync(
            _tenant.TenantId, request.PersonId, ct: ct);
        if (alreadyEnrolled)
            return Result<StudentDto>.Failure(
                "STUDENT_DUPLICATE_PERSON",
                "This person is already registered as a student.");

        var student = Student.Create(
            tenantId: _tenant.TenantId,
            personId: request.PersonId,
            unitId:   request.UnitId,
            notes:    request.Notes);

        await _repository.AddAsync(student, ct);
        await _repository.SaveChangesAsync(ct);

        return Result<StudentDto>.Success(student.Adapt<StudentDto>());
    }
}
