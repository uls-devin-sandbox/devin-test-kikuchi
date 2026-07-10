using System;
using System.Collections.Generic;
using System.Linq;

namespace TodoApp.Core
{
    /// <summary>
    /// タスクの CRUD およびフィルタリングを提供するサービスです。
    /// </summary>
    public class TaskService
    {
        private readonly ITaskRepository _repository;
        private readonly List<TaskItem> _tasks;

        public TaskService(ITaskRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            _repository = repository;
            _tasks = _repository.LoadAll();
        }

        /// <summary>
        /// すべてのタスクを取得します。
        /// </summary>
        public IReadOnlyList<TaskItem> GetAll()
        {
            return _tasks.ToList().AsReadOnly();
        }

        /// <summary>
        /// 指定した完了状態でフィルタリングしたタスクを取得します。
        /// </summary>
        /// <param name="isCompleted">null の場合は全件。</param>
        public IReadOnlyList<TaskItem> GetFiltered(bool? isCompleted)
        {
            if (!isCompleted.HasValue)
            {
                return GetAll();
            }

            return _tasks.Where(t => t.IsCompleted == isCompleted.Value).ToList().AsReadOnly();
        }

        /// <summary>
        /// 新しいタスクを追加します。
        /// </summary>
        public TaskItem Add(string title, DateTime dueDate, Priority priority)
        {
            var task = new TaskItem(title, dueDate, priority);
            _tasks.Add(task);
            Save();
            return task;
        }

        /// <summary>
        /// 既存のタスクを更新します。
        /// </summary>
        public TaskItem Update(string id, string title, DateTime dueDate, Priority priority)
        {
            var task = FindById(id);
            task.Update(title, dueDate, priority);
            Save();
            return task;
        }

        /// <summary>
        /// タスクを削除します。
        /// </summary>
        public void Remove(string id)
        {
            var task = FindById(id);
            _tasks.Remove(task);
            Save();
        }

        /// <summary>
        /// タスクの完了/未完了を切り替えます。
        /// </summary>
        public TaskItem ToggleComplete(string id)
        {
            var task = FindById(id);
            task.ToggleComplete();
            Save();
            return task;
        }

        /// <summary>
        /// 現在のタスクを永続化します。
        /// </summary>
        public void Save()
        {
            _repository.SaveAll(_tasks.ToList());
        }

        private TaskItem FindById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("ID を指定してください。", "id");
            }

            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
            {
                throw new InvalidOperationException("指定された ID のタスクが見つかりません: " + id);
            }

            return task;
        }
    }
}
