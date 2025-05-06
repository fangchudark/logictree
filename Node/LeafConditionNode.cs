using Newtonsoft.Json.Linq;

namespace LogicTree
{
    /// <summary>
    /// 泛型叶子条件节点基类（所有具体叶子节点的抽象父类）<br/>
    /// Json结构：
    /// <code>
    /// {
    ///     "condition_name": 1
    /// }
    /// </code>
    /// </summary>
    /// <typeparam name="TValue">条件值类型</typeparam>
    public abstract class LeafConditionNode<TValue> : LeafConditionNode
    {
        /// <summary>
        /// 条件枚举项
        /// </summary> 
        public string ConditionName { get; set; }
        /// <summary>
        /// 条件值
        /// </summary>
        public abstract TValue Value { get; set; }


        /// <summary>
        /// 将节点序列化为JProperty对象（默认实现）
        /// </summary>
        /// <param name="conditionName">条件枚举项</param>
        /// <param name="value">条件值</param>
        /// <returns>使用蛇形命名的JProperty对象</returns>
        protected virtual JProperty ToJProperty(string conditionName, TValue value)
        {
            return new JProperty(conditionName.ToSnakeCase(), value);
        }

        /// <summary>
        /// 构建嵌套结构的JProperty对象
        /// </summary>
        /// <param name="key">包装键</param>
        /// <param name="conditionName">条件枚举项</param>
        /// <param name="value">条件值</param>
        /// <returns>格式示例：{"key": {"player_level": 5}}</returns>
        protected virtual JProperty ToJPropertyWithKey(string key, string conditionName, TValue value)
        {
            var obj = new JObject
            {
                [conditionName.ToSnakeCase()] = JToken.FromObject(value)    // 蛇形命名键值对
            };

            return new JProperty(key, obj);   // 包装为指定类型的属性
        }

        /// <summary>
        /// 不含包装键的通用反序列化方法（供具体子类调用）
        /// </summary>
        /// <typeparam name="TNode">目标节点类型</typeparam>
        /// <param name="value">JSON输入数据</param>
        /// <param name="createNode">节点构造委托</param>
        /// <returns>实例化的条件节点</returns>
        /// <exception cref="ArgumentException">
        /// 当输入数据不符合以下条件时抛出：
        /// 1. 不是有效的JProperty对象
        /// 2. 使用了包装键
        /// 3. 属性名称不匹配枚举项
        /// 4. 值类型不匹配泛型参数TValue
        /// </exception>
        protected static TNode Deserialize<TNode>(
            JToken value,
            Func<string, TValue, TNode> createNode)
            where TNode : LeafConditionNode<TValue>
        {
            // 验证必须为JProperty类型
            var prop = value as JProperty ?? throw new ArgumentException("value must be a JProperty");

            // 防止用户把 WithKey 的结构误传到普通解析方法
            if (prop.Value.Type == JTokenType.Object || prop.Value.Type == JTokenType.Array)
            {
                throw new ArgumentException(
                    $"Invalid structure for {typeof(TNode).Name}. " +
                    $"Looks like a compound node. Consider using '{nameof(DeserializeWithKey)}' instead."
                );
            }

            // 提取并验证属性值类型与TValue类型一致
            var val = prop.Value.ToObject<TValue>()
                ?? throw new ArgumentException($"Invalid value type: expected {typeof(TValue).Name}");

            return createNode(prop.Name, val);
        }

        /// <summary>
        /// 解析包含包装键的条件节点
        /// </summary>
        /// <typeparam name="T">继承自本基类的具体类型</typeparam>
        /// <param name="value">JSON输入数据</param>
        /// <param name="createNode">节点构造工厂方法</param>
        /// <exception cref="ArgumentException">
        /// 可能抛出以下异常：
        /// 1. JSON结构不符合格式
        /// 2. 包含多个属性值
        /// 3. 值类型不匹配
        /// </exception>
        protected static T DeserializeWithKey<T>(
            JToken value,
            Func<string, TValue, T> createNode)
            where T : LeafConditionNode<TValue>
        {
            // 验证必须为JProperty类型
            var compObj = value as JProperty ?? throw new ArgumentException("value must be an property.");

            // 验证容器类型为对象或数组
            if (compObj.Value.Type is not (JTokenType.Object or JTokenType.Array))
                throw new ArgumentException($"Expected value container to be an object or array, but got {compObj.Value.Type}.");

            // 确保仅包含一个属性
            if (compObj.Values().Count() != 1)
                throw new ArgumentException($"Expected object to have exactly one condition field, but got {compObj.Values().Count()}.");

            // 获取嵌套的属性
            var prop = compObj.Values().First() as JProperty ?? throw new ArgumentException("value must have exactly one property.");

#pragma warning disable CS8604 // 引用类型参数可能为 null。
            return createNode(prop.Name, prop.Value.Value<TValue>());
#pragma warning restore CS8604 // 引用类型参数可能为 null。
        }
    }
    /// <summary>
    /// 所有叶子节点的非泛型基类，用于类型识别<br/>
    /// 所有叶子节点最终都应继承于此类型
    /// </summary>
    public abstract class LeafConditionNode : ConditionNode {}
    
}
