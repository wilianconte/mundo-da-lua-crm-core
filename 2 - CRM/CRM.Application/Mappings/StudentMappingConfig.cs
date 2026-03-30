using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using Mapster;

namespace MyCRM.CRM.Application.Mappings;

public sealed class StudentMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Student, StudentDto>()
            .Map(dest => dest.EnrollmentStatus,
                 src => src.Courses.Any(c => c.Status == StudentCourseStatus.Active)
                     ? StudentEnrollmentStatus.Active
                     : StudentEnrollmentStatus.Inactive);
    }
}
