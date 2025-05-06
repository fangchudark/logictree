using Newtonsoft.Json.Linq;

namespace LogicTree
{
    /// <summary>
    /// 条件节点反序列化接口<br/>
    /// 规定必须实现反序列化方法
    /// </summary>
    /// <typeparam name="TSelf">反序列化目标节点</typeparam>
    public interface IConditionNodeDeserializer<TSelf> where TSelf : IConditionNodeDeserializer<TSelf>
    {
        /// <summary>
        /// 从JSON创建条件节点
        /// </summary>
        /// <param name="value">JSON输入数据</param>
        /// <returns>新实例化的节点</returns>
        static abstract TSelf FromJson(JToken value);

        /// <summary>
        /// 从JSON字符串创建条件节点 <br/>
        /// (在静态类中提供默认实现（<see cref="ConditionNodeDeserializer.FromJsonDefault"/>），调用<see cref="FromJson(JToken)"/>并转化为字符串)
        /// </summary>
        /// <param name="jsonString">JSON字符串</param>
        /// <returns>新实例化的节点</returns>
        static abstract TSelf FromJson(string jsonString);
    }
}