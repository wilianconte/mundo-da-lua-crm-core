using MyCRM.CRM.Application.Commands.StudentCourses.UpdateStudentCourse;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using NSubstitute;

namespace MyCRM.UnitTests.StudentCourses;

public sealed class UpdateStudentCourseHandlerTests
{
    private readonly IStudentCourseRepository _repository = Substitute.For<IStudentCourseRepository>();
    private readonly UpdateStudentCourseHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public UpdateStudentCourseHandlerTests()
    {
        _handler = new UpdateStudentCourseHandler(_repository);
    }

    [Fact]
    public async Task Handle_ValidStatusTransition_ReturnsSuccess()
    {
        var enrollmentId = Guid.NewGuid();
        var enrollment = StudentCourse.Create(_tenantId, Guid.NewGuid(), Guid.NewGuid());

        _repository.GetByIdAsync(enrollmentId, default).Returns(enrollment);
        _repository.SaveChangesAsync(default).Returns(1);

        var command = new UpdateStudentCourseCommand(
            enrollmentId,
            EnrollmentDate: null,
            StartDate: null,
            EndDate: null,
            ClassGroup: null,
            Shift: null,
            ScheduleDescription: null,
            UnitId: null,
            Notes: null,
            Status: StudentCourseStatus.Active);

        var result = await _handler.Handle(command, default);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(StudentCourseStatus.Active, result.Value.Status);
    }

    [Fact]
    public async Task Handle_InvalidStatusTransition_ReturnsFailure()
    {
        var enrollmentId = Guid.NewGuid();
        var enrollment = StudentCourse.Create(_tenantId, Guid.NewGuid(), Guid.NewGuid());
        enrollment.Complete();

        _repository.GetByIdAsync(enrollmentId, default).Returns(enrollment);

        var command = new UpdateStudentCourseCommand(
            enrollmentId,
            EnrollmentDate: null,
            StartDate: null,
            EndDate: null,
            ClassGroup: null,
            Shift: null,
            ScheduleDescription: null,
            UnitId: null,
            Notes: null,
            Status: StudentCourseStatus.Active);

        var result = await _handler.Handle(command, default);

        Assert.False(result.IsSuccess);
        Assert.Equal("STUDENT_COURSE_STATUS_TRANSITION_INVALID", result.ErrorCode);
        await _repository.DidNotReceive().SaveChangesAsync(default);
    }
}
