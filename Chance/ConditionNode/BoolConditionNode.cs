using Godot;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

using static ConditionNodeDeserializer;

/// <summary>
/// 叶子节点<br/>
/// 布尔条件节点
/// </summary>
[GlobalClass]
public partial class BoolConditionNode : LeafConditionNode<ChanceBoolConditionType, bool>, IConditionNodeDeserializer<BoolConditionNode>
{
    /// <summary>
    /// 条件枚举
    /// </summary>
    [Export]
    public override ChanceBoolConditionType ConditionName {get; set;}

    /// <summary>
    /// 期待值
    /// </summary>
    [Export]
    public override bool Value {get; set;}

    /// <summary>
    /// 无参构造函数（引擎使用）
    /// </summary>
    public BoolConditionNode()
    {
    }

    /// <summary>
    /// 给定条件名称和期望值的构造函数
    /// </summary>
    /// <param name="conditionName">枚举词条</param>
    /// <param name="expectedValue">期望值</param>
    public BoolConditionNode(ChanceBoolConditionType conditionName, bool expectedValue)
    {
        ConditionName = conditionName;
        Value = expectedValue;
    }

    /// <summary>
    /// 从JSON结构创造条件节点
    /// </summary>
    /// <param name="value">包含条件数据的JToken对象</param>
    public BoolConditionNode(JToken value)
    {
        FromJson(value);
    }

    /// <summary>
    /// 从JSON字符串创建条件节点
    /// </summary>
    /// <param name="jsonString">符合条件格式的JSON字符串</param>
    public BoolConditionNode(string jsonString)
    {
        FromJson(JToken.Parse(jsonString));
    }

    /// <summary>
    /// 解析JSON生成条件节点（静态工厂方法）
    /// </summary>
    /// <param name="value">包含条件数据的JToken对象</param>
    /// <returns>新创建的BoolConditionNode实例</returns>
    public static BoolConditionNode FromJson(JToken value)
    {
        // 使用基类提供的工厂方法
       return Deserialize(
            value, 
            (cond, val) => new BoolConditionNode(cond,  val)
        );
    }

    /// <summary>
    /// 评估条件是否满足（C#原生接口）
    /// </summary>
    /// <param name="context">包含运行时参数的C#字典</param>
    /// <returns>当上下文中的条件值等于期待值时返回true</returns>
    public override bool Evaluate(Dictionary<string, object> context)
    {
        // 蛇形命名获取上下文中的条件值
        if (context.TryGetValue(GetSnakeCase(ConditionName), out object value))
        {
            // 如果值是布尔类型，则比较值与期待值
            if (value is bool b)
                return b == Value;
        }
        return false; // 否则返回false
    }

    /// <summary>
    /// 将条件序列化为JSON格式
    /// </summary>
    /// <returns>包含条件名称和期望值的JProperty对象</returns>
    public override JProperty ToJson()
    {
        return ToJProperty(ConditionName, Value);
    }

    public static BoolConditionNode FromJson(string jsonString)
    {
        return FromJsonDefault<BoolConditionNode>(jsonString);
    }
}