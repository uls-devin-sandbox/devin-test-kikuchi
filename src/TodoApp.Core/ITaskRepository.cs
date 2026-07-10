using System.Collections.Generic;

namespace TodoApp.Core
{
    /// <summary>
    /// タスクの永続化を抽象化します。
    /// </summary>
    public interface ITaskRepository
    {
        /// <summary>
        /// 保存されているすべてのタスクを読み込みます。
        /// </summary>
        List<TaskItem> LoadAll();

        /// <summary>
        /// すべてのタスクを保存します。
        /// </summary>
        void SaveAll(List<TaskItem> tasks);
    }
}
