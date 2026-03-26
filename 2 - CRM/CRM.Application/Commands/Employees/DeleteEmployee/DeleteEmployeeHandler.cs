using MyCRM.CRM.Domain.Repositories;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Employees.DeleteEmployee;

public sealed class DeleteEmployeeHandler : IRequestHandler<DeleteEmployeeCommand, Result>
{
    private readonly IEmployeeRepository _repository;

    public DeleteEmployeeHandler(IEmployeeRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DeleteEmployeeCommand request, CancellationToken ct)
    {
        var employee = await _repository.GetByIdAsync(request.Id, ct);
        if (employee is null)
            return Result.Failure("EMPLOYEE_NOT_FOUND", "Employee not found.");

        _repository.Delete(employee);
        await _repository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
