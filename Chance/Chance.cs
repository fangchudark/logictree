using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using Newtonsoft.Json.Linq;

/// <summary>
/// 由权重修正器影响的加权器
/// </summary>
[GlobalClass]
public partial class Chance : Resource
{
    /// <summary>
    /// 基础的机会因子
    /// </summary>
    [Export]
    public float Factor {get; set;} = 1f;
    
    /// <summary>
    /// 因子修正
    /// </summary>
    [Export]
    public Array<ChanceModifier> Modifiers {get; set;} = [];

    /// <summary>
    /// 无参构造函数（引擎使用）
    /// </summary>
    public Chance()
    {
        
    }

    /// <summary>
    /// 初始化机会因子和修正器集合
    /// </summary>
    /// <param name="factor">基础概率因子</param>
    /// <param name="modifiers">修正器集合（Godot数组）</param>
    public Chance(float factor, Array<ChanceModifier> modifiers)
    {
        Factor = factor;
        Modifiers = modifiers;
    }

    /// <summary>
    /// 从JSON对象初始化
    /// </summary>
    /// <param name="value">包含factor和modifiers的JObject</param>
    public Chance(JObject value)
    {
        FromJson(value);
    }

    /// <summary>
    /// 从JSON字符串初始化
    /// </summary>
    /// <param name="jsonString">符合Chance格式的JSON字符串</param>
    public Chance(string jsonString)
    {
        FromJson(JObject.Parse(jsonString));
    }

    /// <summary>
    /// 使用泛型集合初始化修正器
    /// </summary>
    /// <param name="factor">基础概率因子</param>
    /// <param name="modifiers">任意可枚举的修正器集合</param>
    public Chance(float factor, IEnumerable<ChanceModifier> modifiers)
    {
        Factor = factor;
        Modifiers = new Array<ChanceModifier>(modifiers);
    }

    /// <summary>
    /// 计算最终概率因子（使用C#原生字典上下文）
    /// </summary>
    /// <param name="context">运行时参数的C#原生字典</param>
    /// <returns>应用所有符合条件的修正后的因子</returns>
    public float GetFactor(System.Collections.Generic.Dictionary<string, object> context)
    {
        float factor = Factor;
        foreach (var modifier in Modifiers)
        {
            if (modifier.Evaluate(context)) // 评估修正器条件
            {
                factor *= modifier.Factor; // 累乘符合条件的修正因子
            }
        }
        return factor;
    }

     /// <summary>
    /// 计算最终概率因子（使用兼容Godot变体值的C#原生字典上下文）
    /// </summary>
    /// <param name="context">运行时参数的兼容Godot变体值的C#原生字典</param>
    /// <returns>应用所有符合条件的修正后的因子</returns>
    public float GetFactor(System.Collections.Generic.Dictionary<string, Variant> context)
    {
        float factor = Factor;
        foreach (var modifier in Modifiers)
        {
            if (modifier.Evaluate(context))
            {
                factor *= modifier.Factor;
            }
        }
        return factor;
    }

    /// <summary>
    /// 计算最终概率因子（使用Godot字典上下文）
    /// </summary>
    /// <param name="context">运行时参数的Godot字典</param>
    /// <returns>应用所有符合条件的修正后的因子</returns>
    public float GetFactor(Godot.Collections.Dictionary<string, Variant> context)
    {
        float factor = Factor;
        foreach (var modifier in Modifiers)
        {
            if (modifier.Evaluate(context))
            {
                factor *= modifier.Factor;
            }
        }
        return factor;
    }

    /// <summary>
    /// 从JSON创建Chance实例（静态工厂方法）
    /// </summary>
    /// <param name="obj">包含序列化数据的JObject</param>
    /// <returns>新创建的Chance实例</returns>
    public static Chance FromJson(JObject obj)
    {
        var chance = new Chance();
        if (obj.TryGetValue("factor", out var factor))
            chance.Factor = (float)factor;
        if (obj.TryGetValue("modifiers", out var modifiers))
        {
            var list = modifiers as JArray ?? [];
            if (list.Count < 1) // 跳过空数组
                return chance;
            
            foreach (var modifier in list)
            {
                if (modifier is not JObject modifierObj)
                    continue;  // 跳过无效的数组元素
                var newModifier = ChanceModifier.FromJson(modifierObj);
                chance.Modifiers.Add(newModifier);
            }
        }
        
        return chance;
    }

    /// <summary>
    /// 从JSON字符串创建Chance实例（静态工厂方法）
    /// </summary>
    /// <param name="jsonString">包含序列化数据的Josn字符串</param>
    /// <returns>新创建的Chance实例</returns>
    public static Chance FromJson(string jsonString)
    {
        try
        {
            return FromJson(JObject.Parse(jsonString));
        }
        catch (Exception e)
        {
            GD.PrintErr("Failed to parse json data:", e);
            return null;
        }
    }

    /// <summary>
    /// 序列化为JSON对象
    /// </summary>
    /// <returns>包含factor和modifiers数组的JObject</returns>
    public JToken ToJson()
    {
        return new JObject
        {
            ["factor"] = Factor,
            ["modifiers"] = new JArray(Modifiers.Select(x => x.ToJson())) // 递归序列化修正器
        };
    }

    /// <summary>
    /// 序列化为JSON字符串
    /// </summary>
    /// <returns>包含factor和modifiers数组的Json字符串</returns>
    public string ToJsonString()
    {
        try
        {
            return ToJson().ToString();
        }
        catch (Exception e)
        {
            GD.PrintErr("Failed to parse instance data:", e);
            return null;
        }
    }
}