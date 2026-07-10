using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TodoApp.Core;

namespace TodoApp.E2E.Tests
{
    /// <summary>
    /// SQL テストデータファイルを読み込み、TaskItem のリストに変換します。
    /// </summary>
    public static class SqlTestDataLoader
    {
        /// <summary>
        /// SQL ファイルを読み込み、TaskItem のリストを返します。
        /// </summary>
        public static List<TaskItem> LoadTasks(string path)
        {
            var rows = SqlTestDataParser.ParseFile(path);
            var tasks = new List<TaskItem>();

            foreach (var row in rows)
            {
                tasks.Add(CreateTask(row));
            }

            return tasks;
        }

        /// <summary>
        /// SQL 行データを TaskItem に変換します。
        /// </summary>
        private static TaskItem CreateTask(Dictionary<string, string> row)
        {
            var task = new TaskItem();

            if (row.ContainsKey("Id"))
            {
                task.Id = row["Id"];
            }

            if (row.ContainsKey("Title"))
            {
                task.Title = row["Title"];
            }

            if (row.ContainsKey("DueDate"))
            {
                task.DueDate = ParseDateTime(row["DueDate"]);
            }

            if (row.ContainsKey("Priority"))
            {
                task.Priority = (Priority)Enum.Parse(typeof(Priority), row["Priority"]);
            }

            if (row.ContainsKey("IsCompleted"))
            {
                task.IsCompleted = ParseInt(row["IsCompleted"]) != 0;
            }

            if (row.ContainsKey("CreatedAt"))
            {
                task.CreatedAt = ParseDateTime(row["CreatedAt"]);
            }

            if (row.ContainsKey("UpdatedAt"))
            {
                task.UpdatedAt = ParseDateTime(row["UpdatedAt"]);
            }

            return task;
        }

        /// <summary>
        /// 文字列を DateTime に変換します。解析に失敗した場合は DateTime.MinValue を返します。
        /// </summary>
        private static DateTime ParseDateTime(string value)
        {
            DateTime result;
            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return result;
            }
            return DateTime.MinValue;
        }

        /// <summary>
        /// 文字列を整数に変換します。解析に失敗した場合は 0 を返します。
        /// </summary>
        private static int ParseInt(string value)
        {
            int result;
            if (int.TryParse(value, out result))
            {
                return result;
            }
            return 0;
        }

        /// <summary>
        /// SQL テストデータから JSON ファイルを生成します。
        /// </summary>
        public static void SeedJson(string sqlFilePath, string jsonFilePath)
        {
            var tasks = LoadTasks(sqlFilePath);
            var repository = new JsonTaskRepository(jsonFilePath);
            repository.SaveAll(tasks);
        }
    }
}
