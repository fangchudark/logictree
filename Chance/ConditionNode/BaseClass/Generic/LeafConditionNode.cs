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
    /// 通用反序列化方法（供具体子类调用）
    /// </summary>
    /// <typeparam name="TNode">目标节点类型</typeparam>
    /// <param name="value">JSON输入数据</param>
    /// <param name="createNode">节点构造委托</param>
    /// <returns>实例化的条件节点</returns>
    /// <exception cref="ArgumentException">
    /// 当输入数据不符合以下条件时抛出：
    /// 1. 不是有效的JProperty对象
    /// 2. 属性名称不匹配枚举项
    /// 3. 值类型不匹配泛型参数TValue
    /// </exception>
    protected static TNode Deserialize<TNode>(
        JToken value,
        Func<TEnum, TValue, TNode> createNode)
        where TNode : LeafConditionNode<TEnum, TValue>
    {
        // 验证必须为JProperty类型
        var prop = value as JProperty ?? throw new ArgumentException("value must be a JProperty");

        // 验证属性名称是否为TEnum枚举的有效名称
        if (!Enum.TryParse<TEnum>(prop.Name.ToPascalCase(), out var conditionName))
            throw new ArgumentException($"Invalid property: {prop.Name}, make sure it's is in the {typeof(TEnum).Name}.");

        // 提取并验证属性值类型与TValue类型一致
        var val = prop.Value.ToObject<TValue>() 
            ?? throw new ArgumentException($"Invalid value type: expected {typeof(TValue).Name}");

        return createNode(conditionName, val);
    }

 }
