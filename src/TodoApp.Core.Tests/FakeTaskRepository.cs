using System.Collections.Generic;

namespace TodoApp.Core.Tests
{
    /// <summary>
    /// テスト用のリポジトリ実装です。
    /// </summary>
    public class FakeTaskRepository : ITaskRepository
    {
        public FakeTaskRepository()
        {
            SavedTasks = new List<TaskItem>();
        }

        public List<TaskItem> SavedTasks { get; private set; }

        public List<TaskItem> LoadAll()
        {
            return SavedTasks;
        }

        public void SaveAll(List<TaskItem> tasks)
        {
            SavedTasks = new List<TaskItem>(tasks);
        }
    }
}
