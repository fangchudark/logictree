using System;
using Newtonsoft.Json.Linq;

namespace LogicTree
{
    /// <summary>
    /// 将目标方法注册为指定键或JToken类型的节点反序列化器
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class NodeDeserializerAttribute : Attribute
    {
        /// <summary>
        /// 指定的键
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// 指定的JToken类型
        /// </summary>
        public JTokenType JTokenType { get; }

        /// <summary>
        /// 将该方法以指定键注册到反序列化器中
        /// </summary>
        /// <param name="key">指定的键</param>
        public NodeDeserializerAttribute(string key)
        {
            Key = key;
        }

        /// <summary>
        /// 将该方法以指定JToken类型注册到反序列化器中<br/>
        /// 所有匹配的JToken类型都将使用该反序列化器
        /// </summary>
        /// <param name="jTokenType">指定的JToken类型</param>
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
        public NodeDeserializerAttribute(JTokenType jTokenType)
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
        {
            JTokenType = jTokenType;
        }
    }
}
