using Newtonsoft.Json.Linq;

namespace LogicTree
{
    using static ConditionNodeDeserializer;
    /// <summary>
    /// 容器节点<br/>
    /// 逻辑或条件节点（当任意子条件满足时返回true）
    /// </summary>
    public class LogicalOrNode : ContainerConditionNode, IConditionNodeDeserializer<LogicalOrNode>
    {
        /// <summary>
        /// 定义JSON序列化的子节点容器键名
        /// </summary>
        protected override string ContainerName => "or";

        /// <summary>
        /// 无参构造函数
        /// </summary>
        public LogicalOrNode()
        {

        }

        /// <summary>
        /// 从JSON结构初始化
        /// </summary>
        /// <param name="value">包含条件数据的JToken</param>
        public LogicalOrNode(JToken value)
        {
            FromJson(value);
        }

        /// <summary>
        /// 从JSON字符串初始化
        /// </summary>
        /// <param name="jsonString">符合条件格式的JSON字符串</param>
        public LogicalOrNode(string jsonString)
        {
            FromJson(JToken.Parse(jsonString));
        }

        /// <summary>
        /// 使用泛型集合初始化子条件
        /// </summary>
        /// <param name="children">任意可枚举的子条件集合</param>
        public LogicalOrNode(IEnumerable<ConditionNode> children)
        {
            Children = new(children);
        }

        /// <summary>
        /// 评估是否存在满足的子条件
        /// </summary>
        /// <param name="context">运行时上下文</param>
        /// <returns>当任意子条件返回true时返回true</returns>
        public override bool Evaluate(Dictionary<string, object> context)
        {
            return Children.Any(node => node.Evaluate(context));    // 使用LINQ Any方法
        }

        /// <summary>
        /// 序列化为JSON格式（自动处理重复条件的JProperty对象）
        /// </summary>
        /// <returns>包含重复的键时，JPropery对象的值为JArray，否则为JObject</returns>
        public override JProperty ToJson()
        {
            return SerializeChildren();
        }

        /// <summary>
        /// 从JSON创建逻辑或节点
        /// </summary>
        /// <param name="value">包含"or"属性的JToken</param>
        /// <returns>包含所有子条件的LogicalOrNode实例</returns>
        /// <exception cref="ArgumentException">当JSON结构无效时抛出</exception>
        [NodeDeserializer("or")]
        public static LogicalOrNode FromJson(JToken value)
        {            
            return Deserialize(value, "or", () => new LogicalOrNode());
        }

        /// <inheritdoc/>
        public static LogicalOrNode FromJson(string jsonString)
        {
            return FromJsonDefault<LogicalOrNode>(jsonString);
        }
    }
}