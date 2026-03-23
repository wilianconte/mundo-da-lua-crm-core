using FluentValidation;

namespace MyCRM.CRM.Application.Commands.Courses.UpdateCourse;

public sealed class UpdateCourseValidator : AbstractValidator<UpdateCourseCommand>
{
    public UpdateCourseValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(300);

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.Code)
            .MaximumLength(100)
            .When(x => x.Code is not null);

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => x.Description is not null);

        RuleFor(x => x.ScheduleDescription)
            .MaximumLength(500)
            .When(x => x.ScheduleDescription is not null);

        RuleFor(x => x.Capacity)
            .GreaterThan(0)
            .When(x => x.Capacity is not null);

        RuleFor(x => x.Workload)
            .GreaterThan(0)
            .When(x => x.Workload is not null);

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => x.Notes is not null);
    }
}
