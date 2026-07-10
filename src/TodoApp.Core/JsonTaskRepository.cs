using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;

namespace TodoApp.Core
{
    /// <summary>
    /// JSON ファイルを使用したタスクの永続化を行います。
    /// </summary>
    public class JsonTaskRepository : ITaskRepository
    {
        private readonly string _filePath;

        public JsonTaskRepository(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException("filePath");
            }

            _filePath = filePath;
        }

        public List<TaskItem> LoadAll()
        {
            if (!File.Exists(_filePath))
            {
                return new List<TaskItem>();
            }

            try
            {
                using (var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (stream.Length == 0)
                    {
                        return new List<TaskItem>();
                    }

                    var serializer = CreateSerializer();
                    var result = serializer.ReadObject(stream) as List<TaskItem>;
                    return result ?? new List<TaskItem>();
                }
            }
            catch (Exception)
            {
                return new List<TaskItem>();
            }
        }

        public void SaveAll(List<TaskItem> tasks)
        {
            if (tasks == null)
            {
                throw new ArgumentNullException("tasks");
            }

            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var stream = new FileStream(_filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var serializer = CreateSerializer();
                serializer.WriteObject(stream, tasks);
            }
        }

        private static DataContractJsonSerializer CreateSerializer()
        {
            var settings = new DataContractJsonSerializerSettings
            {
                DateTimeFormat = new System.Runtime.Serialization.DateTimeFormat("yyyy-MM-ddTHH:mm:ss")
            };
            return new DataContractJsonSerializer(typeof(List<TaskItem>), settings);
        }
    }
}
