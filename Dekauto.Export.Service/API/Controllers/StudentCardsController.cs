using Dekauto.Export.Service.Domain.Entities;
using Dekauto.Export.Service.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using Serilog;

namespace Dekauto.Export.Service.API.Controllers
{
    [Route("api/studentcards")]
    [ApiController]
    [Authorize]
    public class StudentCardsController : ControllerBase
    {
        private readonly IStudentsService studentsService;
        private string defaultLatFileName = "exported_student_card";
        private readonly ILogger<StudentCardsController> logger;
        public StudentCardsController(IStudentsService studentsService, ILogger<StudentCardsController> logger) 
        {
            this.studentsService = studentsService??throw new ArgumentNullException();
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private void SetHeaderFileNames(string fileName, string fileNameStar)
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
        public async Task<IActionResult> ExportStudentAsync([FromBody] Student student) 
        {
            try
            {
                logger.LogInformation($"Начало экспорта студента: {student.Surname} {student.Name}");
                var stream = await studentsService.ConvertStudentToExcel(student);
                // INFO: данные в имени файла не должны содержать спецсимволы!
                string fileName = $"{student.Surname} {student.Name} {student.Patronymic}";
                SetHeaderFileNames(defaultLatFileName, fileName);

                // Возвращаем файл БЕЗ указания имени в третьем параметре
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
            catch (Exception ex) 
            {
                logger.LogError(ex, $"Ошибка при экспорте студента {student.Surname} {student.Name}: {ex.Message}");
                return HandleException(ex);
            }
        }
        [HttpPost("students")]
        public async Task<IActionResult> ExportStudentsAsync([FromBody] List<Student> students)
        {
            try
            {
                logger.LogInformation($"Начало экспорта группы. Количество студентов: {students.Count}");
                var stream = await studentsService.ConvertStudentsToExcel(students);

                // INFO: данные в имени файла не должны содержать спецсимволы
                string fileName = students.First().GroupName ?? throw new ArgumentNullException(nameof(fileName));
                SetHeaderFileNames(defaultLatFileName, fileName);
                return File(stream, "application/zip");
            }
            catch (Exception ex) 
            {
                logger.LogError(ex, $"Ошибка при групповом экспорте: {ex.Message}");
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
