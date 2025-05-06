using Newtonsoft.Json.Linq;

namespace LogicTree
{
    using static ConditionNodeDeserializer;
    /// <summary>
    /// 容器节点<br/>
    /// 逻辑非条件节点（当所有子条件都不满足时返回true）
    /// </summary>
    public class LogicalNotNode : ContainerConditionNode, IConditionNodeDeserializer<LogicalNotNode>
    {
        /// <summary>
        /// 定义JSON序列化的子节点容器键名
        /// </summary>
        protected override string ContainerName => "not";

        /// <summary>
        /// 无参构造函数
        /// </summary>
        public LogicalNotNode()
        {

        }

        /// <summary>
        /// 从JSON结构初始化
        /// </summary>
        /// <param name="value">包含条件数据的JToken</param>
        public LogicalNotNode(JToken value)
        {
            FromJson(value);
        }

        /// <summary>
        /// 从JSON字符串初始化
        /// </summary>
        /// <param name="jsonString">符合条件格式的JSON字符串</param>
        public LogicalNotNode(string jsonString)
        {
            FromJson(JToken.Parse(jsonString));
        }

        /// <summary>
        /// 使用泛型集合初始化子条件
        /// </summary>
        /// <param name="children">任意可枚举的子条件集合</param>
        public LogicalNotNode(IEnumerable<ConditionNode> children)
        {
            Children = new(children);
        }

        /// <summary>
        /// 从JSON创建逻辑非节点
        /// </summary>
        /// <param name="value">包含"not"属性的JToken</param>
        /// <returns>包含所有子条件的LogicalNotNode实例</returns>
        /// <exception cref="ArgumentException">当JSON结构无效时抛出</exception>
        [NodeDeserializer("not")]
        public static LogicalNotNode FromJson(JToken value)
        {            
            return Deserialize(value, "not", () => new LogicalNotNode());
        }

        /// <summary>
        /// 评估所有子条件是否都不满足
        /// </summary>
        /// <param name="context">运行时上下文</param>
        /// <returns>当所有子条件返回false时返回true</returns>
        public override bool Evaluate(Dictionary<string, object> context)
        {
            return Children.All(node => !node.Evaluate(context)); // 取反逻辑判断
        }

        /// <summary>
        /// 序列化为JSON格式（自动处理重复条件的JProperty对象）
        /// </summary>
        /// <returns>包含重复的键时，JPropery对象的值为JArray，否则为JObject</returns>
        public override JProperty ToJson()
        {
            return SerializeChildren();
        }

        /// <inheritdoc/>
        public static LogicalNotNode FromJson(string jsonString)
        {
            return FromJsonDefault<LogicalNotNode>(jsonString);
        }
    }
}