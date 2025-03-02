using Dekauto.Export.Service.API.Controllers;
using Dekauto.Export.Service.Domain.Entities;
using Dekauto.Export.Service.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekauto.Export.Tests
{
    [TestClass]
    public class TestStudents
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
            var students = new List<Student>
            {
                new Student
                { 
                    Id = Guid.NewGuid(),
                    Name = "Иван",
                    Surname = "Иванов",
                    Pathronymic = "Иванович" 
                }
            };
            _studentsService.Setup(s => s.ConvertStudentsToExcel(It.IsAny<List<Student>>())).Returns(new MemoryStream());

            //Act
            var result = await _controller.ExportStudents(students);

            //Assert
            Assert.IsNotNull(result);
            var fileResult = result as FileResult; // Приводим к FileResult
            Assert.IsNotNull(fileResult); // Проверяем, что результат является FileResult

            var fileStreamResult = fileResult as FileStreamResult;
            Assert.AreEqual("application/zip", fileStreamResult.ContentType); // Проверка типа контента
            Assert.AreEqual($"Primer", fileStreamResult.FileDownloadName); // Проверка имени файла
            Assert.IsNotNull(fileStreamResult.FileStream);

        }

        [TestMethod]
        public async Task StudentNotExist()
        {
            //Arrange
            var students = new List<Student>
            {
                new Student
                {
                    Id = Guid.NewGuid(),
                    Name = "Иван",
                    Surname = "Иванов",
                    Pathronymic = "Иванович"
                }
            };
            _studentsService.Setup(s => s.ConvertStudentsToExcel(It.IsAny<List<Student>>())).Throws(new ArgumentNullException());

            //Act
            var result = await _controller.ExportStudents(students);

            //Assert
            Assert.IsNotNull(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [TestMethod]
        public async Task FileNotExist()
        {
            //Arrange
            var students = new List<Student>
            {
                new Student
                {
                    Id = Guid.NewGuid(),
                    Name = "Иван",
                    Surname = "Иванов",
                    Pathronymic = "Иванович"
                }
            };
            _studentsService.Setup(s => s.ConvertStudentsToExcel(It.IsAny<List<Student>>())).Throws(new FileNotFoundException());

            //Act
            var result = await _controller.ExportStudents(students);

            //Assert
            Assert.IsNotNull(result);
            var NotFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(NotFoundResult);
            Assert.AreEqual(StatusCodes.Status404NotFound, NotFoundResult.StatusCode);
        }

        [TestMethod]
        public async Task ListsNotExist()
        {
            //Arrange
            var students = new List<Student>
            {
                new Student
                {
                    Id = Guid.NewGuid(),
                    Name = "Иван",
                    Surname = "Иванов",
                    Pathronymic = "Иванович"
                }
            };
            _studentsService.Setup(s => s.ConvertStudentsToExcel(It.IsAny<List<Student>>())).Throws(new InvalidOperationException());

            //Act
            var result = await _controller.ExportStudents(students);

            //Assert
            Assert.IsNotNull(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [TestMethod]
        public async Task Fatal()
        {
            //Arrange
            var students = new List<Student>
            {
                new Student
                {
                    Id = Guid.NewGuid(),
                    Name = "Иван",
                    Surname = "Иванов",
                    Pathronymic = "Иванович"
                }
            };
            _studentsService.Setup(s => s.ConvertStudentsToExcel(It.IsAny<List<Student>>())).Throws(new Exception());

            //Act
            var result = await _controller.ExportStudents(students);

            //Assert
            Assert.IsNotNull(result);
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            Assert.AreEqual("Неизвестная ошибка, обратитесь к администратору", objectResult.Value);
        }
    }
}
