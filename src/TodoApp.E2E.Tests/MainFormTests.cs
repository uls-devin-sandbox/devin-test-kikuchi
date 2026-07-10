using System;
using System.IO;
using NUnit.Framework;
using TodoApp.Core;

namespace TodoApp.E2E.Tests
{
    /// <summary>
    /// ToDo 管理画面の E2E テストです。
    /// </summary>
    [TestFixture]
    public class MainFormTests
    {
        private string _exePath;
        private string _sqlFilePath;
        private WindowDriver _driver;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var testDir = TestContext.CurrentContext.TestDirectory;
            _exePath = Path.Combine(testDir, "TodoApp.WinForms.exe");
            _sqlFilePath = Path.Combine(testDir, "test-data", "e2e-tasks.sql");
        }

        [SetUp]
        public void SetUp()
        {
            var testDir = TestContext.CurrentContext.TestDirectory;
            var tasksJson = Path.Combine(testDir, "tasks.json");
            if (File.Exists(tasksJson))
            {
                File.Delete(tasksJson);
            }

            var testName = TestContext.CurrentContext.Test.Name;
            if (testName.Contains("Filter") || testName.Contains("Persistence"))
            {
                SqlTestDataLoader.SeedJson(_sqlFilePath, tasksJson);
            }

            _driver = new WindowDriver(_exePath);
        }

        [TearDown]
        public void TearDown()
        {
            if (_driver != null)
            {
                _driver.Dispose();
                _driver = null;
            }
        }

        [Test]
        public void TestAddTask()
        {
            _driver.EnterTitle("買い物");
            _driver.SetDueDate(new DateTime(2026, 7, 15));
            _driver.SelectPriority("中");
            _driver.ClickAdd();

            Assert.That(_driver.GetRowCount(), Is.EqualTo(1));
            Assert.That(_driver.GetCellText(0, 0), Is.EqualTo("買い物"));
            Assert.That(_driver.GetCellText(0, 2), Is.EqualTo("中"));
            Assert.That(_driver.GetCellText(0, 4), Is.EqualTo("未完了"));
            Assert.That(File.Exists(_driver.TasksJsonPath), Is.True);
            Assert.That(File.ReadAllText(_driver.TasksJsonPath), Does.Contain("買い物"));
        }

        [Test]
        public void TestEditTask()
        {
            _driver.EnterTitle("買い物");
            _driver.SetDueDate(new DateTime(2026, 7, 15));
            _driver.SelectPriority("中");
            _driver.ClickAdd();

            _driver.SelectRow(0);
            _driver.ClickEdit();
            _driver.EnterTitle("買い物 → 変更済み");
            _driver.SelectPriority("高");
            _driver.ClickAdd();

            Assert.That(_driver.GetRowCount(), Is.EqualTo(1));
            Assert.That(_driver.GetCellText(0, 0), Is.EqualTo("買い物 → 変更済み"));
            Assert.That(_driver.GetCellText(0, 2), Is.EqualTo("高"));
            Assert.That(_driver.GetCellText(0, 4), Is.EqualTo("未完了"));
        }

        [Test]
        public void TestToggleComplete()
        {
            _driver.EnterTitle("買い物");
            _driver.SetDueDate(new DateTime(2026, 7, 15));
            _driver.SelectPriority("中");
            _driver.ClickAdd();

            _driver.SelectRow(0);
            _driver.ClickToggle();
            Assert.That(_driver.GetCellText(0, 4), Is.EqualTo("完了"));

            _driver.SelectRow(0);
            _driver.ClickToggle();
            Assert.That(_driver.GetCellText(0, 4), Is.EqualTo("未完了"));
        }

        [Test]
        public void TestDeleteTask()
        {
            _driver.EnterTitle("買い物");
            _driver.SetDueDate(new DateTime(2026, 7, 15));
            _driver.SelectPriority("中");
            _driver.ClickAdd();

            _driver.SelectRow(0);
            _driver.ClickDelete();
            _driver.ClickMessageBoxYes("確認");

            Assert.That(_driver.GetRowCount(), Is.EqualTo(0));
            Assert.That(File.Exists(_driver.TasksJsonPath), Is.True);
            Assert.That(File.ReadAllText(_driver.TasksJsonPath), Does.Not.Contain("買い物"));
        }

        [Test]
        public void TestFilter()
        {
            _driver.SelectFilter("未完了");
            Assert.That(_driver.GetRowCount(), Is.EqualTo(2));

            _driver.SelectFilter("完了");
            Assert.That(_driver.GetRowCount(), Is.EqualTo(1));

            _driver.SelectFilter("全て");
            Assert.That(_driver.GetRowCount(), Is.EqualTo(3));
        }

        [Test]
        public void TestPersistence()
        {
            Assert.That(_driver.GetRowCount(), Is.EqualTo(3));

            _driver.Dispose();
            _driver = null;

            _driver = new WindowDriver(_exePath);
            Assert.That(_driver.GetRowCount(), Is.EqualTo(3));
        }

        [Test]
        public void TestValidation()
        {
            _driver.ClickAdd();
            _driver.ClickMessageBoxOk("入力エラー");

            Assert.That(_driver.GetRowCount(), Is.EqualTo(0));
        }

        [Test]
        public void TestNoSelection()
        {
            _driver.ClickEdit();
            _driver.ClickMessageBoxOk("選択エラー");

            _driver.ClickDelete();
            _driver.ClickMessageBoxOk("選択エラー");

            _driver.ClickToggle();
            _driver.ClickMessageBoxOk("選択エラー");

            Assert.That(_driver.GetRowCount(), Is.EqualTo(0));
        }
    }
}
