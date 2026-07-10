using System.Runtime.Serialization;

namespace TodoApp.Core
{
    /// <summary>
    /// タスクの優先度を表します。
    /// </summary>
    [DataContract(Name = "priority")]
    public enum Priority
    {
        [EnumMember(Value = "High")]
        High,

        [EnumMember(Value = "Medium")]
        Medium,

        [EnumMember(Value = "Low")]
        Low
    }
}
