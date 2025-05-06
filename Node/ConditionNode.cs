using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace LogicTree
{
    /// <summary>
    /// 条件节点的基类
    /// </summary>
    public abstract class ConditionNode
    {
        /// <summary>
        /// 评估方法
        /// </summary>
        /// <param name="context">上下文字典</param>
        /// <returns>当条件满足时返回true</returns>
        public abstract bool Evaluate(Dictionary<string, object> context);


        /// <summary>
        /// 子类必须实现的序列化逻辑
        /// </summary>
        /// <returns>包含有效属性的JsonToken</returns>
        public virtual JProperty ToJson()
        {
            throw new ConditionNodeSerializationException("Unable to serialize the base node to Json string");
        }

        /// <summary>
        /// 序列化为Json字符串（提供默认实现，调用<see cref="ToJson"/>包装为 JObject 并转化为字符串），可在子类重写
        /// </summary>
        /// <returns>序列化后的Json字符串，失败时返回空字符串</returns>
        public virtual string ToJsonString()
        {
            try
            {
                return new JObject(ToJson()).ToString();
            }
            catch (Exception e)
            {
                throw new ConditionNodeSerializationException("Failed to serialize the node to Json string", e);
            }
        }
    }
}