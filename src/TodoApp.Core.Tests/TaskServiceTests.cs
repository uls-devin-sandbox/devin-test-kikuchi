using System;
using NUnit.Framework;

namespace TodoApp.Core.Tests
{
    [TestFixture]
    public class TaskServiceTests
    {
        private FakeTaskRepository _repository;
        private TaskService _service;

        [SetUp]
        public void SetUp()
        {
            _repository = new FakeTaskRepository();
            _service = new TaskService(_repository);
        }

        [Test]
        public void Add_TaskIsAdded()
        {
            var task = _service.Add("買い物", new DateTime(2026, 7, 15), Priority.Medium);

            Assert.That(task, Is.Not.Null);
            Assert.That(task.Title, Is.EqualTo("買い物"));
            Assert.That(task.DueDate, Is.EqualTo(new DateTime(2026, 7, 15)));
            Assert.That(task.Priority, Is.EqualTo(Priority.Medium));
            Assert.That(task.IsCompleted, Is.False);
            Assert.That(_service.GetAll().Count, Is.EqualTo(1));
        }

        [Test]
        public void Add_WithEmptyTitle_ThrowsArgumentException()
        {
            Assert.That(() => _service.Add("  ", DateTime.Today, Priority.Low), Throws.ArgumentException);
        }

        [Test]
        public void Update_TaskIsUpdated()
        {
            var task = _service.Add("買い物", new DateTime(2026, 7, 15), Priority.Medium);
            var updated = _service.Update(task.Id, "レポート", new DateTime(2026, 7, 20), Priority.High);

            Assert.That(updated.Title, Is.EqualTo("レポート"));
            Assert.That(updated.DueDate, Is.EqualTo(new DateTime(2026, 7, 20)));
            Assert.That(updated.Priority, Is.EqualTo(Priority.High));
        }

        [Test]
        public void Remove_TaskIsRemoved()
        {
            var task = _service.Add("買い物", DateTime.Today, Priority.Medium);
            _service.Remove(task.Id);

            Assert.That(_service.GetAll().Count, Is.EqualTo(0));
        }

        [Test]
        public void ToggleComplete_TaskStatusIsToggled()
        {
            var task = _service.Add("買い物", DateTime.Today, Priority.Medium);
            var toggled = _service.ToggleComplete(task.Id);

            Assert.That(toggled.IsCompleted, Is.True);

            toggled = _service.ToggleComplete(task.Id);
            Assert.That(toggled.IsCompleted, Is.False);
        }

        [Test]
        public void GetFiltered_ReturnsOnlyMatchingTasks()
        {
            var task1 = _service.Add("買い物", DateTime.Today, Priority.Medium);
            var task2 = _service.Add("レポート", DateTime.Today, Priority.High);
            _service.ToggleComplete(task2.Id);

            Assert.That(_service.GetFiltered(false).Count, Is.EqualTo(1));
            Assert.That(_service.GetFiltered(true).Count, Is.EqualTo(1));
            Assert.That(_service.GetFiltered(null).Count, Is.EqualTo(2));
        }

        [Test]
        public void Save_PersistsTasksToRepository()
        {
            _service.Add("買い物", DateTime.Today, Priority.Medium);
            _service.Add("レポート", DateTime.Today, Priority.High);

            Assert.That(_repository.SavedTasks.Count, Is.EqualTo(2));
        }
    }
}
