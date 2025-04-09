using System.Collections.Generic;
using Godot;
using Newtonsoft.Json.Linq;

/// <summary>
/// 叶子节点<br/>
/// 数值等于条件节点（当上下文值与期望值近似相等时满足）
/// </summary>
[GlobalClass]
public partial class ValueConditionNode : LeafConditionNode<ChanceNumberConditionType, double>, IConditionNodeDeserializer<ValueConditionNode>
{
    /// <summary>
    /// 数值条件类型枚举
    /// </summary>
    [Export]
    public override ChanceNumberConditionType ConditionName {get; set;}

    /// <summary>
    /// 期望的数值（支持浮点数精度比较）
    /// </summary>
    [Export]
    public override double Value {get; set;}

    /// <summary>
    /// 无参构造函数（引擎使用）
    /// </summary>
    public ValueConditionNode()
    {
        
    }

    /// <summary>
    /// 初始化数值条件
    /// </summary>
    /// <param name="conditionName">数值条件枚举项</param>
    /// <param name="expectedValue">期望的阈值</param>
    public ValueConditionNode(ChanceNumberConditionType conditionName, double expectedValue)
    {
        ConditionName = conditionName;
        Value = expectedValue;
    }

    /// <summary>
    /// 从JSON结构初始化
    /// </summary>
    /// <param name="value">包含条件数据的JToken</param>
    public ValueConditionNode(JToken value)
    {
        FromJson(value);
    }

    /// <summary>
    /// 从JSON字符串初始化
    /// </summary>
    /// <param name="jsonString">符合条件格式的JSON字符串</param>
    public ValueConditionNode(string jsonString)
    {
        FromJson(JToken.Parse(jsonString));
    }

    /// <summary>
    /// 从JSON创建数值条件节点
    /// </summary>
    /// <param name="value">包含条件键值对的JToken</param>
    /// <returns>新实例化的ValueConditionNode</returns>
    public static ValueConditionNode FromJson(JToken value)
    {
        // 使用基类的方法
        return Deserialize(
            value, 
            (con, val) => new ValueConditionNode(con, val)
        );
    }

    /// <summary>
    /// 评估上下文值是否近似等于期望值
    /// </summary>
    /// <param name="context">运行时上下文数据</param>
    /// <returns>当检测到符合条件的数值时返回true</returns>
    public override bool Evaluate(Dictionary<string, object> context)
    {
        if (context.TryGetValue(GetSnakeCase(ConditionName), out object value))
        {
            // 处理所有数值类型的近似比较
            switch (value)
            {
                case float f:
                    return Mathf.IsEqualApprox(f, Value);
                case int i:
                    return Mathf.IsEqualApprox(i, Value);
                case long l:
                    return Mathf.IsEqualApprox(l, Value);
                case double d:
                    return Mathf.IsEqualApprox(d, Value);
                case uint ui:
                    return Mathf.IsEqualApprox(ui, Value);
                case ulong ul:
                    return Mathf.IsEqualApprox(ul, Value);
                case short s:
                    return Mathf.IsEqualApprox(s, Value);
                case ushort us:
                    return Mathf.IsEqualApprox(us, Value);
            }
        }
        return false;
    }

    /// <summary>
    /// 序列化为JSON格式
    /// </summary>
    /// <returns>结构化的JProperty对象</returns>
    public override JProperty ToJson()
    {
        return ToJProperty(ConditionName, Value);
    }
}