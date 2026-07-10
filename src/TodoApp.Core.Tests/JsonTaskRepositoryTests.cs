using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace TodoApp.Core.Tests
{
    [TestFixture]
    public class JsonTaskRepositoryTests
    {
        private string _tempFile;

        [SetUp]
        public void SetUp()
        {
            _tempFile = Path.Combine(Path.GetTempPath(), "todo_test_" + Guid.NewGuid().ToString("N") + ".json");
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_tempFile))
            {
                File.Delete(_tempFile);
            }
        }

        [Test]
        public void LoadAll_FileDoesNotExist_ReturnsEmptyList()
        {
            var repository = new JsonTaskRepository(Path.Combine(Path.GetTempPath(), "not_exist_" + Guid.NewGuid().ToString("N") + ".json"));
            var tasks = repository.LoadAll();

            Assert.That(tasks, Is.Not.Null);
            Assert.That(tasks.Count, Is.EqualTo(0));
        }

        [Test]
        public void SaveAndLoad_PreservesTasks()
        {
            var repository = new JsonTaskRepository(_tempFile);
            var tasks = new System.Collections.Generic.List<TaskItem>
            {
                new TaskItem("買い物", new DateTime(2026, 7, 15), Priority.Medium),
                new TaskItem("レポート", new DateTime(2026, 7, 20), Priority.High)
            };
            tasks[1].ToggleComplete();

            repository.SaveAll(tasks);
            var loaded = repository.LoadAll();

            Assert.That(loaded.Count, Is.EqualTo(2));
            Assert.That(loaded[0].Title, Is.EqualTo("買い物"));
            Assert.That(loaded[0].Priority, Is.EqualTo(Priority.Medium));
            Assert.That(loaded[0].DueDate, Is.EqualTo(new DateTime(2026, 7, 15)));
            Assert.That(loaded[0].IsCompleted, Is.False);
            Assert.That(loaded[1].Title, Is.EqualTo("レポート"));
            Assert.That(loaded[1].IsCompleted, Is.True);
        }

        [Test]
        public void SaveAndLoad_PreservesDates()
        {
            var repository = new JsonTaskRepository(_tempFile);
            var tasks = new System.Collections.Generic.List<TaskItem>
            {
                new TaskItem("ミーティング", new DateTime(2026, 12, 31, 23, 59, 58), Priority.Low)
            };

            repository.SaveAll(tasks);
            var loaded = repository.LoadAll();

            Assert.That(loaded[0].DueDate, Is.EqualTo(new DateTime(2026, 12, 31, 23, 59, 58)));
        }

        [Test]
        public void SaveAll_CreatesDirectoryIfNotExists()
        {
            var directory = Path.Combine(Path.GetTempPath(), "todo_dir_" + Guid.NewGuid().ToString("N"));
            var filePath = Path.Combine(directory, "tasks.json");

            try
            {
                var repository = new JsonTaskRepository(filePath);
                repository.SaveAll(new System.Collections.Generic.List<TaskItem>());

                Assert.That(Directory.Exists(directory), Is.True);
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }
    }
}
