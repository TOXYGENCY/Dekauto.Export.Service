using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dekauto.Export.Tests
{
    [TestClass]
    public class TestAppsettings
    {
        private IConfiguration _configuration;

        [TestInitialize]
        public void Setup()
        {
            // Конфигурация из файла appsettings.json
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Путь к файлу 
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
        }
        [TestMethod]
        public void TestCardName() 
        {
            //Arrange
            var expectedCardName = "УЧЕБНАЯ_КАРТОЧКА_студента.xlsx";

            //Act
            var actualCardName = _configuration["ExportCardName"];

            //Assert
            Assert.AreEqual(expectedCardName, actualCardName);
        }
    }
}
