using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace LogicTree
{
    using static ConditionNodeDeserializer;

    /// <summary>
    /// 根节点/容器节点<br/>
    /// 逻辑与条件节点（所有子条件必须全部满足）
    /// </summary>    
    public partial class LogicalAndNode : ContainerConditionNode, IConditionNodeDeserializer<LogicalAndNode>
    {
        /// <summary>
        /// 定义JSON序列化的子节点容器键名
        /// </summary>
        protected override string ContainerName => "and";

        /// <summary>
        /// 无参构造函数
        /// </summary>
        public LogicalAndNode()
        {

        }

        /// <summary>
        /// 从JSON结构初始化
        /// </summary>
        /// <param name="value">包含条件数据的JToken</param>
        public LogicalAndNode(JToken value)
        {
            FromJson(value);
        }

        /// <summary>
        /// 从JSON字符串初始化
        /// </summary>
        /// <param name="jsonString">符合条件格式的JSON字符串</param>
        public LogicalAndNode(string jsonString)
        {
            FromJson(JToken.Parse(jsonString));
        }

        /// <summary>
        /// 使用泛型集合初始化子条件
        /// </summary>
        /// <param name="children">任意可枚举的子条件集合</param>
        public LogicalAndNode(IEnumerable<ConditionNode> children)
        {
            Children = new(children);
        }


        /// <summary>
        /// 评估所有子条件是否满足
        /// </summary>
        /// <param name="context">运行时上下文</param>
        /// <returns>当所有子条件返回true时返回true</returns>
        public override bool Evaluate(Dictionary<string, object> context)
        {
            return Children.All(c => c.Evaluate(context)); //   LINQ All方法验证全部条件
        }

        /// <summary>
        /// 从JSON创建逻辑与节点
        /// </summary>
        /// <param name="value">包含"and"属性的JToken</param>
        /// <returns>包含所有子条件的LogicalAndNode实例</returns>
        [NodeDeserializer("and")]
        public static LogicalAndNode FromJson(JToken value)
        {            
            // 调用基类方法, 递归反序列化子节点
            return Deserialize(value, "and", () => new LogicalAndNode());
        }

        /// <summary>
        /// 序列化为JSON格式（自动处理重复条件的JProperty对象）
        /// </summary>
        /// <returns>包含重复的键时，JPropery对象的值为JArray，否则为JObject</returns>
        public override JProperty ToJson()
        {
            // 调用基类方法
            return SerializeChildren();
        }

        /// <inheritdoc/>
        public static LogicalAndNode FromJson(string jsonString)
        {            
            return FromJsonDefault<LogicalAndNode>(jsonString);
        }
    }

}