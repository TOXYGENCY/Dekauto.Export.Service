using Dekauto.Export.Service.API.Controllers;
using Dekauto.Export.Service.Domain.Entities;
using Dekauto.Export.Service.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Decauto.Export.Tests
{
    [TestClass]
    public class TestStudentCardsController
    {
        private Mock<IStudentsService> _studentsService;
        private StudentCardsController _controller;

        [TestInitialize]
        public void Setup() 
        {
            _studentsService = new Mock<IStudentsService>();
            _controller = new StudentCardsController(_studentsService.Object);
        }

        [TestMethod]
        public async Task Success()
        {
            //Arrange
            var student = new Student 
            {
                Id = Guid.NewGuid(), 
                Name = "Иван",
                Surname = "Иванов",
                Pathronymic = "Иванович",
            };
            _studentsService.Setup(s=>s.ConvertStudentToExcel(It.IsAny<Student>())).Returns(new MemoryStream());

            //Act
            var result = await _controller.ExportStudent(student);

            //Assert
            Assert.IsNotNull(result);
            var fileResult = result as FileResult; // Приводим к FileResult
            Assert.IsNotNull(fileResult); // Проверяем, что результат является FileResult

            var fileStreamResult = fileResult as FileStreamResult;
            Assert.AreEqual("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileStreamResult.ContentType); // Проверка типа контента
            Assert.AreEqual($"{student.Name} {student.Surname} {student.Pathronymic}", fileStreamResult.FileDownloadName); // Проверка имени файла
            Assert.IsNotNull(fileStreamResult.FileStream);

        }
    }
}
