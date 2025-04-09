using Godot;
using Newtonsoft.Json.Linq;

/// <summary>
/// 叶子节点<br/>
/// 小于比较条件节点（当上下文值小于阈值时满足条件）
/// </summary>
[GlobalClass]
public partial class LessThanNode : ComparisonConditionNode<ChanceNumberConditionType, double>, IConditionNodeDeserializer<LessThanNode>
{
    /// <summary>
    /// 数值条件类型枚举
    /// </summary>
    [Export]
    public override ChanceNumberConditionType ConditionName {get; set;}
    
    /// <summary>
    /// 比较阈值
    /// </summary>
    [Export]
    public override double Value  {get; set;}

    /// <summary>
    /// <see cref="ComparisonConditionNode.Evaluate(Dictionary{string, object})"/>使用默认实现，<br/>
    /// 定义比较逻辑（当实际值 < 预期值时返回true）
    /// </summary>
    protected override bool Compare(double expectedValue, double actualValue) =>  actualValue <  expectedValue;

    /// <summary>
    /// 定义JSON序列化的操作符键名
    /// </summary>
    protected override string Operator => "less_than";

    /// <summary>
    /// 无参构造函数（引擎使用）
    /// </summary>
    public LessThanNode()
    {
    }

    /// <summary>
    /// 初始化比较条件
    /// </summary>
    /// <param name="conditionName">数值条件枚举项</param>
    /// <param name="threshold">比较阈值</param>
    public LessThanNode(ChanceNumberConditionType conditionName, double threshold)
    {
        ConditionName = conditionName;
        Value = threshold;
    }

    /// <summary>
    /// 从JSON结构初始化
    /// </summary>
    /// <param name="value">包含条件数据的JToken</param>
    public LessThanNode(JToken value)
    {
        FromJson(value);
    }

    /// <summary>
    /// 从JSON字符串初始化
    /// </summary>
    /// <param name="jsonString">符合条件格式的JSON字符串</param>
    public LessThanNode(string jsonString)
    {
        FromJson(JToken.Parse(jsonString));
    }

    /// <summary>
    /// 从JSON创建比较条件节点
    /// </summary>
    /// <param name="value">JSON输入数据</param>
    /// <returns>新实例化的LessThanNode</returns>
    public static LessThanNode FromJson(JToken value)
    {
        return Deserialize(value, (cond, val) => new LessThanNode(cond, val ));
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