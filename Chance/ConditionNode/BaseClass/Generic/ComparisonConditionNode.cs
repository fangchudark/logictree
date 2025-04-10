using Godot;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 泛型比较条件节点基类（用于数值比较类型的叶子节点）<br/>
/// Json结构：
/// <code>
/// {
///     "operator": { 
///         "condition_name": 1
///     } 
/// }
/// </code>
/// </summary>
/// <typeparam name="TEnum">条件枚举类型（必须可转换为Variant）</typeparam>
/// <typeparam name="TValue">数值类型（必须可转换为Variant）</typeparam>
public abstract partial class ComparisonConditionNode<[MustBeVariant] TEnum, [MustBeVariant] TValue> : LeafConditionNode<TEnum, TValue>
    where TEnum : struct, Enum
{
    /// <summary>
    /// 比较两个值是否满足条件（虚拟方法）。<br/>
    /// 默认实现返回 true，表示任何值都满足比较条件。<br/>
    /// 当使用默认的 <see cref="Evaluate"/> 实现时，子类应重写此方法以提供自定义比较逻辑。
    /// </summary>
    /// <param name="expectedValue">节点配置的期望值</param>
    /// <param name="actualValue">上下文中的实际值</param>
    /// <returns>返回比较结果</returns>
    protected virtual bool Compare(TValue expectedValue, TValue actualValue)
    {
        return true;    // 默认实现，总是返回 true
    }

    /// <summary>
    /// 定义JSON序列化的操作符键名
    /// </summary>
    protected abstract string Operator {get;}

    /// <summary>
    /// 评估上下文中的实际值是否满足预期的比较条件（默认实现）。<br/>
    /// 默认实现比较逻辑使用<see cref="Compare"/><br/>
    /// 默认实现使用 <see cref="Convert.ChangeType"/> 进行类型转换，
    /// 若转换失败或未找到对应键，则返回 false。<br/>
    /// 如需处理更复杂或特殊的类型匹配逻辑，请在子类中重写此方法。
    /// </summary>
    /// <param name="context">运行时上下文数据</param>
    /// <returns>当实际值成功转换并满足比较条件时返回 true，否则返回 false。</returns>
    public override bool Evaluate(Dictionary<string, object> context)
    {
        if (context.TryGetValue(GetSnakeCase(ConditionName), out var value))
        {
            try
            {
                TValue actualValue = (TValue)Convert.ChangeType(value, typeof(TValue));
                return Compare(Value, actualValue);
            }
            catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException or ArgumentException)
            {
                GD.PrintErr($"Cannot covert '{value.GetType()}' to '{typeof(TValue)}'! ");
            }
        }
        return false;
    }

    
    /// <summary>
    /// 构建嵌套结构的JProperty对象
    /// </summary>
    /// <param name="conditionName">条件枚举项</param>
    /// <param name="threshold">比较阈值</param>
    /// <returns>格式示例：{"more_than": {"player_level": 5}}</returns>
    protected override JProperty ToJProperty(TEnum conditionName, TValue threshold)
    {
        return ToJPropertyWithKey(Operator, conditionName, threshold);   // 包装为指定类型的属性   
    }

    /// <summary>
    /// 解析包含比较操作的条件节点
    /// </summary>
    /// <typeparam name="T">继承自本基类的具体类型</typeparam>
    /// <param name="value">JSON输入数据</param>
    /// <param name="createNode">节点构造工厂方法</param>
    /// <exception cref="ArgumentException">
    /// 可能抛出以下异常：
    /// 1. JSON结构不符合比较条件格式
    /// 2. 包含多个属性值
    /// 3. 值类型不匹配
    /// </exception>
    protected new static T Deserialize<T>(
        JToken value, 
        Func<TEnum, TValue, T> createNode) 
        where T : ComparisonConditionNode<TEnum, TValue>
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
        var prop = compObj.Values().First() as JProperty ?? throw new ArgumentException("Comparison must have exactly one property.");

        // 验证属性值类型为数字
        if (prop.Value.Type is not (JTokenType.Integer or JTokenType.Float))
            throw new ArgumentException("Comparison value must be a number.");
        
        // 验证属性名称是否为枚举的有效名称
        if (!Enum.TryParse<TEnum>(prop.Name.ToPascalCase(), out var conditionName))
            throw new ArgumentException($"Invalid property: {prop.Name}, make sure it's in the ChanceNumberConditionType enum.");

        return createNode(conditionName, prop.Value.Value<TValue>());
    }
}