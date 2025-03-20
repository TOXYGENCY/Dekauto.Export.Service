using Dekauto.Export.Service.Domain.Entities;
using Dekauto.Export.Service.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace Dekauto.Export.Service.API.Controllers
{
    [Route("api/studentcards")]
    [ApiController]
    public class StudentCardsController : ControllerBase
    {
        private readonly IStudentsService _studentsService;
        private string _defaultLatFileName = "exported_student_card";

        public StudentCardsController(IStudentsService studentsService) 
        {
            _studentsService = studentsService??throw new ArgumentNullException();
        }

        private void _setHeaderFileNames(string fileName, string fileNameStar)
        {
            // Проблема: передается только сам файл, а его название автомат. вписывается в заголовки, но без поддержки кириллицы.
            // Формируем http-заголовок с поддержкой UTF-8 (для поддержки кириллицы в http-заголовках)
            if (Response == null)
            {
                throw new InvalidOperationException("Response не инициализирован.");
            }
            var encodedFileName = Uri.EscapeDataString(fileNameStar);
            Response.Headers.Append(
                "Content-Disposition",
                $"attachment; filename=\"{fileName}.xlsx\"; filename*=UTF-8''{encodedFileName}"
            );
        }

        [HttpPost("student")]
        public async Task<IActionResult> ExportStudent([FromBody] Student student) 
        {
            try
            {
                var stream = await _studentsService.ConvertStudentToExcel(student);
                // INFO: данные в имени файла не должны содержать спецсимволы!
                string fileName = $"{student.Surname} {student.Name} {student.Patronymic}";
                _setHeaderFileNames(_defaultLatFileName, fileName);

                // Возвращаем файл БЕЗ указания имени в третьем параметре
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
            catch (Exception ex) 
            {
                return HandleException(ex);
            }
        }
        [HttpPost("students")]
        public async Task<IActionResult> ExportStudents([FromBody] List<Student> students)
        {
            try
            {
                var stream = await _studentsService.ConvertStudentsToExcel(students);

                // INFO: данные в имени файла не должны содержать спецсимволы
                string fileName = students.First().GroupName ?? throw new ArgumentNullException(nameof(fileName));
                _setHeaderFileNames(_defaultLatFileName, fileName);
                return File(stream, "application/zip");
            }
            catch (Exception ex) 
            {
                return HandleException(ex);
            }
        }
        private IActionResult HandleException(Exception ex) 
        {
            switch (ex) 
            {
                case ArgumentNullException argumentNullException:
                    return BadRequest(argumentNullException.Message);
                case FileNotFoundException fileNotFoundException:
                    return NotFound($"{fileNotFoundException.Message} {fileNotFoundException.FileName}");
                case InvalidOperationException invalidOperationException:
                    return BadRequest(invalidOperationException.Message);
                default:
                    return StatusCode(500, "Неизвестная ошибка, обратитесь к администратору");
            }
        }
    }
}
