using Dekauto.Export.Service.Domain.Entities;

namespace Dekauto.Export.Service.Domain.Interfaces
{
    public interface IStudentsService
    {
        MemoryStream ConvertStudentToExcel(Student student);
        MemoryStream ConvertStudentsToExcel(List<Student> students);

    }
}
