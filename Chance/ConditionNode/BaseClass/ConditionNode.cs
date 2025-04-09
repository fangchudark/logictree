using Godot;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System;

/// <summary>
/// 条件节点基类
/// </summary>
[GlobalClass]
public abstract partial class ConditionNode : Resource
{
    /// <summary>
    /// C#原生评估接口(手动实现方法)
    /// </summary>
    /// <param name="context">C#原生字典上下文</param>
    /// <returns>当条件满足时返回true</returns>
    public abstract bool Evaluate(System.Collections.Generic.Dictionary<string, object> context);

    /// <summary>
    /// Godot引擎专用的评估接口<br/>
    /// （根据<see cref="Evaluate(Dictionary{string, object})"/> 重载的自动实现方法）<br/>
    /// 重写它来实现自定义评估逻辑（如果真的需要，比如 Godot的Variant与C#原生不兼容的类型）
    /// </summary>
    /// <param name="context">Godot原生字典（使用<see cref="DictConvert"/>转换为C#字典）</param>
    /// <returns>自动实现的情况下与<see cref="Evaluate(Dictionary{string, object})"/>的评估结果相同</returns>
    public virtual bool Evaluate(Godot.Collections.Dictionary<string, Variant> context)
    {
       var sysDict = DictConvert(context);
       return Evaluate(sysDict);
    }

    /// <summary>
    /// Godot兼容接口<br/>
    /// （根据<see cref="Evaluate(Dictionary{string, object})"/> 重载的自动实现方法）<br/>
    /// 重写它来实现自定义评估逻辑（如果真的需要，比如 Godot的Variant与C#原生不兼容的类型）
    /// </summary>
    /// <param name="context">包含Godot变体值的C#原生字典（使用<see cref="DictConvert"/>转换为C#原生值字典）</param>
    /// <returns>自动实现的情况下与<see cref="Evaluate(Dictionary{string, object})"/>的评估结果相同</returns>
    public virtual bool Evaluate(Dictionary<string, Variant> context)
    {
        var sysDict = DictConvert(context);
        return Evaluate(sysDict);
    }

    /// <summary>
    /// 类型安全转换方法（Godot Variant → C# 类型）<br/>
    /// 使用于<see cref="Evaluate(Dictionary{string, object})"/> 的自动实现重载中<br/>
    /// 重写它来实现自定义转换逻辑（如果真的需要，比如 Godot的Variant与C#原生不兼容的类型）
    /// </summary>
    /// <param name="context">包含Godot变体值的字典上下文</param>
    /// <returns>包含有效类型的C#字典</returns>
    protected virtual Dictionary<string, object> DictConvert(IDictionary<string, Variant> context)
    {
        // 将Godot字典转换为C#字典时进行类型过滤
        var sysDict = new Dictionary<string, object>();
        foreach (var key in context.Keys)
        {
            // 过滤掉不支持的Variant类型
            if (context[key].VariantType is not (
                Variant.Type.Float 
                or Variant.Type.Int 
                or Variant.Type.Bool))
            {
                GD.PushWarning($"Unsupported Variant type for key '{key}': {context[key].VariantType}, ignored.");
                continue;
            }
            sysDict[key] = context[key].Obj;    // 将Variant转换为C#原生类型
        }        
        return sysDict;
    }

    /// <summary>
    /// 子类必须实现的序列化逻辑
    /// </summary>
    /// <returns>包含有效属性的JsonToken</returns>
    public abstract JProperty ToJson();


    /// <summary>
    /// 获取条件类型的蛇形命名键
    /// </summary>
    /// <param name="conditionName">枚举</param>
    /// <returns>小写蛇形命名字符串</returns>
    protected static string GetSnakeCase(Enum conditionName)
        => conditionName.ToSnakeCase();

}
