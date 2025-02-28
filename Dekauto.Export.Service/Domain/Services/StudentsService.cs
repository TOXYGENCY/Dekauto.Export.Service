using Dekauto.Export.Service.Domain.Entities;
using Dekauto.Export.Service.Domain.Interfaces;
using OfficeOpenXml;

namespace Dekauto.Export.Service.Domain.Services
{
    public class StudentsService: IStudentsService
    {
        public MemoryStream ConvertStudentToExcel(Student student)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            if (student == null) throw new ArgumentNullException(nameof(student));

            var templatePath = Path.Combine(Directory.GetCurrentDirectory(),"Шаблон.xlsx"); //Путь шаблона
            Console.WriteLine($"Template path: {templatePath}");
            var stream = new MemoryStream();//Используем временное хранилище
            
            using (var package = new ExcelPackage(new FileInfo(templatePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];//Выбираем первый лист

                worksheet.Cells["A2"].Value = student.Name;
                worksheet.Cells["B2"].Value = student.Surname;
                worksheet.Cells["C2"].Value = student.Pathronymic;
                
                package.SaveAs(stream);//Сохраняем файл
            }
            stream.Position = 0;//Сбрасываем позицию
            return stream;
        }
    }
}
