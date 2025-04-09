
using Newtonsoft.Json.Linq;

/// <summary>
/// 条件节点反序列化接口<br/>
/// 规定必须实现反序列化方法
/// </summary>
/// <typeparam name="TSelf">反序列化目标节点</typeparam>
public interface IConditionNodeDeserializer <TSelf> where TSelf : IConditionNodeDeserializer <TSelf>
{
    /// <summary>
    /// 从JSON创建条件节点
    /// </summary>
    /// <param name="value">JSON输入数据</param>
    /// <returns>新实例化的节点</returns>
    static abstract TSelf FromJson(JToken value);

}