using FluentValidation;

namespace MyCRM.CRM.Application.Commands.StudentCourses.UpdateStudentCourse;

public sealed class UpdateStudentCourseValidator : AbstractValidator<UpdateStudentCourseCommand>
{
    public UpdateStudentCourseValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.ClassGroup)
            .MaximumLength(100)
            .When(x => x.ClassGroup is not null);

        RuleFor(x => x.Shift)
            .MaximumLength(100)
            .When(x => x.Shift is not null);

        RuleFor(x => x.ScheduleDescription)
            .MaximumLength(500)
            .When(x => x.ScheduleDescription is not null);

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => x.Notes is not null);
    }
}
