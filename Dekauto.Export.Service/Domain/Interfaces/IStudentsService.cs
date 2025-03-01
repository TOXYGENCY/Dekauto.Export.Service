using Dekauto.Export.Service.Domain.Entities;

namespace Dekauto.Export.Service.Domain.Interfaces
{
    public interface IStudentsService
    {
        Task<MemoryStream> ConvertStudentToExcel(Student student);
        Task<MemoryStream> ConvertStudentsToExcel(List<Student> students);

    }
}
