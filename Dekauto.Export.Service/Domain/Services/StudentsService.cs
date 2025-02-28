using Dekauto.Export.Service.Domain.Entities;
using Dekauto.Export.Service.Domain.Interfaces;
using OfficeOpenXml;
using System.IO.Compression;

namespace Dekauto.Export.Service.Domain.Services
{
    public class StudentsService: IStudentsService
    {
        public MemoryStream ConvertStudentsToExcel(List<Student> students)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            if (students == null) throw new ArgumentNullException(nameof(students));

            
            var stream = new MemoryStream();//Используем временное хранилище

            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                foreach (var student in students) 
                {
                    var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Шаблон.xlsx"); //Путь шаблона 
                    using (var package = new ExcelPackage(new FileInfo(templatePath)))
                    {
                        var worksheet = package.Workbook.Worksheets[0];//Выбираем первый лист

                        worksheet.Cells["A2"].Value = student.Name;
                        worksheet.Cells["B2"].Value = student.Surname;
                        worksheet.Cells["C2"].Value = student.Pathronymic;

                        var entry = archive.CreateEntry($"{student.Name}_{student.Surname}_{student.Pathronymic}.xlsx");
                        using (var entryStream = entry.Open()) 
                        { 
                            package.SaveAs(entryStream);//Сохраняем файл
                        }
                        
                    }

                }
            }
            stream.Position = 0;
            return stream;
        }

        public MemoryStream ConvertStudentToExcel(Student student)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            if (student == null) throw new ArgumentNullException(nameof(student));

            var templatePath = Path.Combine(Directory.GetCurrentDirectory(),"Шаблон.xlsx"); //Путь шаблона
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
