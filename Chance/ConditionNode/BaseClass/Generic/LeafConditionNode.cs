using System;
using System.Linq;
using Godot;
using Newtonsoft.Json.Linq;

/// <summary>
/// 泛型叶子条件节点基类（所有具体叶子节点的抽象父类）<br/>
/// Json结构：
/// <code>
/// {
///     "condition_name": 1
/// }
/// </code>
/// </summary>
/// <typeparam name="TEnum">条件枚举类型（必须可转换为Variant）</typeparam>
/// <typeparam name="TValue">条件值类型（必须可转换为Variant）</typeparam>
public abstract partial class LeafConditionNode<[MustBeVariant] TEnum, [MustBeVariant] TValue> : LeafConditionNode 
    where TEnum : struct, Enum
{
    /// <summary>
    /// 条件枚举项（通过子类实现具体导出逻辑）
    /// </summary>
    public abstract TEnum ConditionName {get; set;}
 
    /// <summary>
    /// 条件值（通过子类实现具体导出逻辑）
    /// </summary>
    public abstract TValue Value {get; set;}

    /// <summary>
    /// 将节点序列化为JProperty对象（默认实现）
    /// </summary>
    /// <param name="conditionName">条件枚举项</param>
    /// <param name="value">条件值</param>
    /// <returns>使用蛇形命名的JProperty对象</returns>
    protected virtual JProperty ToJProperty(TEnum conditionName, TValue value)
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
    protected virtual JProperty ToJPropertyWithKey(string key, TEnum conditionName, TValue value)
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
        Func<TEnum, TValue, TNode> createNode)
        where TNode : LeafConditionNode<TEnum, TValue>
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

        // 验证属性名称是否为TEnum枚举的有效名称
        if (!Enum.TryParse<TEnum>(prop.Name.ToPascalCase(), out var conditionName))
            throw new ArgumentException($"Invalid property: {prop.Name}, make sure it's is in the {typeof(TEnum).Name}.");

        // 提取并验证属性值类型与TValue类型一致
        var val = prop.Value.ToObject<TValue>() 
            ?? throw new ArgumentException($"Invalid value type: expected {typeof(TValue).Name}");

        return createNode(conditionName, val);
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
        Func<TEnum, TValue, T> createNode) 
        where T : LeafConditionNode<TEnum, TValue>
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
        
        // 验证属性名称是否为枚举的有效名称
        if (!Enum.TryParse<TEnum>(prop.Name.ToPascalCase(), out var conditionName))
            throw new ArgumentException($"Invalid property: {prop.Name}, make sure it's in the  {typeof(TEnum).Name} enum.");

        return createNode(conditionName, prop.Value.Value<TValue>());
    }
 }
