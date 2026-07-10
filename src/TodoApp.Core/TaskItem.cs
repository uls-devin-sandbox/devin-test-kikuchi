using System;
using System.Runtime.Serialization;

namespace TodoApp.Core
{
    /// <summary>
    /// ToDo タスクを表すエンティティです。
    /// </summary>
    [DataContract(Name = "task")]
    public class TaskItem
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "dueDate")]
        public DateTime DueDate { get; set; }

        [DataMember(Name = "priority")]
        public Priority Priority { get; set; }

        [DataMember(Name = "isCompleted")]
        public bool IsCompleted { get; set; }

        [DataMember(Name = "createdAt")]
        public DateTime CreatedAt { get; set; }

        [DataMember(Name = "updatedAt")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// シリアライザ用の既定コンストラクタです。
        /// </summary>
        public TaskItem()
        {
        }

        /// <summary>
        /// タイトル、期限、優先度を指定して新しいタスクを作成します。
        /// </summary>
        public TaskItem(string title, DateTime dueDate, Priority priority)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("タイトルを入力してください。", "title");
            }

            var now = DateTime.Now;
            Id = Guid.NewGuid().ToString("N");
            Title = title.Trim();
            DueDate = dueDate;
            Priority = priority;
            IsCompleted = false;
            CreatedAt = now;
            UpdatedAt = now;
        }

        /// <summary>
        /// タスクを更新します。
        /// </summary>
        public void Update(string title, DateTime dueDate, Priority priority)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("タイトルを入力してください。", "title");
            }

            Title = title.Trim();
            DueDate = dueDate;
            Priority = priority;
            UpdatedAt = DateTime.Now;
        }

        /// <summary>
        /// 完了/未完了を切り替えます。
        /// </summary>
        public void ToggleComplete()
        {
            IsCompleted = !IsCompleted;
            UpdatedAt = DateTime.Now;
        }
    }
}
