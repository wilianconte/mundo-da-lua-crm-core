using FluentValidation;

namespace MyCRM.CRM.Application.Commands.StudentCourses.CreateStudentCourse;

public sealed class CreateStudentCourseValidator : AbstractValidator<CreateStudentCourseCommand>
{
    public CreateStudentCourseValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.CourseId).NotEmpty();

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
