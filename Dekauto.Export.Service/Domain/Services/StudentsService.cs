using Dekauto.Export.Service.Domain.Entities;
using Dekauto.Export.Service.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.IO.Compression;

namespace Dekauto.Export.Service.Domain.Services
{
    public class StudentsService: IStudentsService
    {
        private IConfiguration _configuration;
        private string exportCardName;
        public StudentsService (IConfiguration configuration) 
        {
            _configuration = configuration;
            exportCardName = _configuration.GetValue<string>("ExportCardName")??throw new ArgumentNullException(nameof(exportCardName));

        }
        public async Task<MemoryStream> ConvertStudentsToExcel(List<Student> students)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            if (students == null || students.Count == 0) throw new ArgumentNullException(nameof(students));


            var stream = new MemoryStream();//Используем временное хранилище

            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                foreach (var student in students)
                {
                    var templatePath = Path.Combine(Directory.GetCurrentDirectory(), exportCardName); //Путь шаблона 

                    if (!File.Exists(templatePath))
                    {
                        throw new FileNotFoundException("Файл шаблона не найден. Обратитесь к администратору");
                    }

                    using (var package = new ExcelPackage(new FileInfo(templatePath)))
                    {
                        FillExcel(student, package);

                        var entry = archive.CreateEntry($"{student.Name} {student.Surname} {student.Pathronymic}.xlsx");
                        using (var entryStream = entry.Open())
                        {
                            await package.SaveAsAsync(entryStream);//Сохраняем файл
                        }

                    }

                }
            }
            stream.Position = 0;
            return stream;
        }

        public async Task<MemoryStream> ConvertStudentToExcel(Student student)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            if (student == null) throw new ArgumentNullException(nameof(student));

            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), exportCardName); //Путь шаблона

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException("Файл шаблона не найден. Обратитесь к администратору");
            }

            var stream = new MemoryStream();//Используем временное хранилище
            
            using (var package = new ExcelPackage(new FileInfo(templatePath)))
            {
                FillExcel(student, package);
                await package.SaveAsAsync(stream);//Сохраняем файл
            }
            stream.Position = 0;//Сбрасываем позицию
            return stream;
        }
        public void FillExcel(Student student, ExcelPackage package) 
        {
            if (package.Workbook.Worksheets.Count == 0)
            {
                throw new InvalidOperationException("Файл шаблона не содержит листов");
            }

            var worksheet = package.Workbook.Worksheets[0];//Выбираем первый лист

            //Персональные данные
            worksheet.Cells["B4"].Value = student.Name;
            worksheet.Cells["B3"].Value = student.Surname;
            worksheet.Cells["B5"].Value = student.Pathronymic;
            worksheet.Cells["G3"].Value = student.GradeBook;
            worksheet.Cells["G4"].Value = student.GradeBook;
            if (student.Gender == true) worksheet.Cells["B6"].Value = "М";
            else worksheet.Cells["B6"].Value = "Ж";
            worksheet.Cells["E6"].Value = student.BirthdayDate;
            worksheet.Cells["A7"].Value = student.BirthdayPlace;

            //Контактные данные
            worksheet.Cells["C9"].Value = student.PhoneNumber;
            worksheet.Cells["G9"].Value = student.Email;

            //Паспортные данные
            worksheet.Cells["D10"].Value = student.PassportSerial;
            worksheet.Cells["F10"].Value = student.PassportNumber;
            worksheet.Cells["A11"].Value = student.PassportIssuancePlace;
            worksheet.Cells["B12"].Value = student.PassportIssuanceDate;
            worksheet.Cells["F12"].Value = student.PassportIssuanceCode;

            //Адрес регистрации
            worksheet.Cells["C13"].Value = student.Citizenship;
            worksheet.Cells["B15"].Value = student.AddressRegistrationIndex;
            worksheet.Cells["G15"].Value = student.AddressRegistrationOblKrayAvtobl;
            worksheet.Cells["B16"].Value = student.AddressRegistrationDistrict;
            worksheet.Cells["E16"].Value = student.AddressRegistrationType;
            worksheet.Cells["G16"].Value = student.AddressRegistrationCity;
            worksheet.Cells["B17"].Value = student.AddressRegistrationStreet;
            worksheet.Cells["F17"].Value = student.AddressRegistrationHouse;
            worksheet.Cells["G17"].Value = student.AddressRegistrationHousingType;
            worksheet.Cells["H17"].Value = student.AddressRegistrationHousing;
            worksheet.Cells["J17"].Value = student.AddressRegistrationApartment;

            //Адрес проживания
            worksheet.Cells["B19"].Value = student.AddressRegistrationIndex;
            worksheet.Cells["G19"].Value = student.AddressRegistrationOblKrayAvtobl;
            worksheet.Cells["B20"].Value = student.AddressRegistrationDistrict;
            worksheet.Cells["E20"].Value = student.AddressRegistrationType;
            worksheet.Cells["G20"].Value = student.AddressRegistrationCity;
            worksheet.Cells["B21"].Value = student.AddressRegistrationStreet;
            worksheet.Cells["F21"].Value = student.AddressRegistrationHouse;
            worksheet.Cells["G21"].Value = student.AddressRegistrationHousingType;
            worksheet.Cells["H21"].Value = student.AddressRegistrationHousing;
            worksheet.Cells["J21"].Value = student.AddressRegistrationApartment;
            if (student.LivingInDormitory == true) worksheet.Cells["D22"].Value = "да";
            else worksheet.Cells["D22"].Value = "нет";

            //Основания зачисления в МГППУ
            worksheet.Cells["A34"].Value = student.EnrollementOrderNum;
            worksheet.Cells["C34"].Value = student.EnrollementOrderDate;

            worksheet.Cells["B37"].Value = student.GiaExam1Name;
            worksheet.Cells["E37"].Value = student.GiaExam1Score;
            worksheet.Cells["F37"].Value = student.GiaExam1Note;
            worksheet.Cells["B38"].Value = student.GiaExam2Name;
            worksheet.Cells["E38"].Value = student.GiaExam2Score;
            worksheet.Cells["F38"].Value = student.GiaExam2Note;
            worksheet.Cells["B39"].Value = student.GiaExam3Name;
            worksheet.Cells["E39"].Value = student.GiaExam3Score;
            worksheet.Cells["F39"].Value = student.GiaExam3Note;

            if (student.EducationReceived == "Среднее общее") worksheet.Cells["D40"].Value = "среднее общее образование";
            else if (student.EducationReceived == "Высшее образование") worksheet.Cells["D40"].Value = "высшее образование";
            else worksheet.Cells["D40"].Value = "среднее профессиональное образование";

            worksheet.Cells["B42"].Value = student.EducationReceivedNum;
            worksheet.Cells["D42"].Value = student.EducationReceivedSerial;
            worksheet.Cells["H42"].Value = student.EducationReceivedDate;
            worksheet.Cells["C44"].Value = student.OOName;
            worksheet.Cells["C45"].Value = student.OOAddress;
            worksheet.Cells["C46"].Value = student.EducationReceivedEndYear;
            if (student.BonusScores != 0) worksheet.Cells["C47"].Value = "да";
            else worksheet.Cells["C47"].Value = "нет";

            //Доп данные
            if (student.MaritalStatus == true) worksheet.Cells["D56"].Value = "да";
            else worksheet.Cells["D56"].Value = "нет";
            if (student.MilitaryService == true) worksheet.Cells["J56"].Value = "да";
            else worksheet.Cells["J56"].Value = "нет";

            //Текущие координаты обучения и статус			
            worksheet.Cells["C76"].Value = student.Education;
            worksheet.Cells["H76"].Value = student.EducationForm;
            worksheet.Cells["B77"].Value = student.Faculty;
            worksheet.Cells["C78"].Value = student.CourseOfTraining;
            worksheet.Cells["C79"].Value = student.Course;
            worksheet.Cells["C80"].Value = "адаптированная для лиц с ОВЗ"; // Данные статичны
            worksheet.Cells["B81"].Value = student.GroupName;
            worksheet.Cells["F81"].Value = student.EducationStartYear;
            worksheet.Cells["J81"].Value = student.EducationFinishYear;
            worksheet.Cells["I82"].Value = student.EducationTime;
            worksheet.Cells["C84"].Value = student.EducationBase;
            worksheet.Cells["C85"].Value = student.EducationRelationForm;
            worksheet.Cells["G85"].Value = student.EducationRelationNum;
            worksheet.Cells["I85"].Value = student.EducationRelationDate;

        }
    }
}
