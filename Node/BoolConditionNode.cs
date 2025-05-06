using Newtonsoft.Json.Linq;


namespace LogicTree
{

    using static ConditionNodeDeserializer;

    /// <summary>
    /// 叶子节点<br/>
    /// 布尔条件节点
    /// </summary>
    public class BoolConditionNode : LeafConditionNode<bool>, IConditionNodeDeserializer<BoolConditionNode>
    {

        /// <summary>
        /// 期待值
        /// </summary>    
        public override bool Value { get; set; }

        /// <summary>
        /// 给定条件名称和期望值的构造函数
        /// </summary>
        /// <param name="conditionName">词条字符串</param>
        /// <param name="expectedValue">期望值</param>
        public BoolConditionNode(string conditionName, bool expectedValue)
        {
            ConditionName = conditionName.ToSnakeCase();
            Value = expectedValue;
        }

        /// <summary>
        /// 给定条件名称和期望值的构造函数
        /// </summary>
        /// <param name="conditionName">枚举词条</param>
        /// <param name="expectedValue">期望值</param>
        public BoolConditionNode(Enum conditionName, bool expectedValue)
        {
            ConditionName = conditionName.ToSnakeCase();
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
        [NodeDeserializer(JTokenType.Boolean)]
        public static BoolConditionNode FromJson(JToken value)
        {            
            // 使用基类提供的工厂方法
            return Deserialize(
                 value,
                 (cond, val) => new BoolConditionNode(cond, val)
             );
        }

        /// <inheritdoc/>
        public override bool Evaluate(Dictionary<string, object> context)
        {
            // 蛇形命名获取上下文中的条件值
#pragma warning disable CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
            if (context.TryGetValue(ConditionName.ToSnakeCase(), out object value))
            {
                // 如果值是布尔类型，则比较值与期待值
                if (value is bool b)
                    return b == Value;
            }
#pragma warning restore CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
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

        /// <inheritdoc/>
        public static BoolConditionNode FromJson(string jsonString)
        {
            return FromJsonDefault<BoolConditionNode>(jsonString);
        }
    }
}