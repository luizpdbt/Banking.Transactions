using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking.Barramento.Api.Tests.ArchitechTests
{
    public class ProjectArchitectedDefinition
    {
        [Fact]
        public void Project_Should_Have_Required_Folders()
        {
            // Arrange
            var projectPath = FindProjectPath();

            string[] requiredFolders =
            [
            "Application",
            "Controllers",
            "Domain",
            "Dto",
            "Enums",
            "Extension",
            "Infrastructure",
            "Middleware",
            "Validator",
            ];

            // Act
            var missingFolders = requiredFolders.Where(folder =>!Directory.Exists(Path.Combine(projectPath, folder))).ToList();

            // Assert
            Assert.True(missingFolders.Count == 0,$"Missing architecture folders: {string.Join(", ", missingFolders)}");
        }

        private static string FindProjectPath()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            var rootPath = directory.ToString().Replace(@"Banking.Barramento.Api.Tests\bin\Debug\net9.0\", "Banking.Transactions");

            return rootPath;
        }
    }
}
