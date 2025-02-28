using Dekauto.Export.Service.Domain.Entities;
using Dekauto.Export.Service.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dekauto.Export.Service.API.Controllers
{
    [Route("api/studentcards")]
    [ApiController]
    public class StudentCardsController : ControllerBase
    {
        private readonly IStudentsService _studentsService;
        public StudentCardsController(IStudentsService studentsService) 
        {
            _studentsService = studentsService;
        }
        [HttpPost("student")]
        public async Task<IActionResult> Get([FromBody] Student student) 
        {
            try
            {
                var stream = _studentsService.ConvertStudentToExcel(student);

                string fileName = $"{student.Name} {student.Surname} {student.Pathronymic}";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
