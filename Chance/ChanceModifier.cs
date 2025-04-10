using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Newtonsoft.Json.Linq;

/// <summary>
/// 权重修正器
/// </summary>
[GlobalClass]
public partial class ChanceModifier : Resource
{

    /// <summary>
    /// 机会修正带来的因子修正值
    /// </summary>
    [Export]
    public float Factor {get; set;} = 1f;

    /// <summary>
    /// 修正条件逻辑树（根节点使用逻辑与节点）
    /// </summary>
    [Export]
    public LogicalAndNode LogicTree {get; set;} = new();

    /// <summary>
    /// 无参构造函数（引擎使用）
    /// </summary>
    public ChanceModifier()
    {

    }

    /// <summary>
    /// 初始化修正因子和逻辑条件
    /// </summary>
    /// <param name="factor">修正乘数</param>
    /// <param name="LogicTree">修正条件逻辑树（根节点使用逻辑与节点）</param>
    public ChanceModifier(float factor, LogicalAndNode logicTree)
    {
        Factor = factor;
        LogicTree = logicTree;
    }

    /// <summary>
    /// 从JSON对象初始化修正器
    /// </summary>
    /// <param name="json">包含factor和条件树的JObject</param>
    public ChanceModifier(JObject json)
    {
        FromJson(json);
    }

    /// <summary>
    /// 从JSON字符串初始化修正器
    /// </summary>
    /// <param name="jsonString">符合修正器格式的JSON字符串</param>
    public ChanceModifier(string jsonString)
    {
        FromJson(JObject.Parse(jsonString));
    }

    /// <summary>
    /// 评估条件是否满足（C#原生字典接口）
    /// </summary>
    /// <param name="context">运行时上下文数据</param>
    /// <returns>当所有子节点条件满足时返回true</returns>
    public bool Evaluate(System.Collections.Generic.Dictionary<string, object> context)
    {
        return LogicTree.Evaluate(context);     // 委托给逻辑与节点评估
    }

    /// <summary>
    /// 评估条件是否满足（兼容Godot变体值的C#原生字典接口）
    /// </summary>
    /// <param name="context">运行时上下文数据</param>
    /// <returns>当所有子节点条件满足时返回true</returns>
    public bool Evaluate(System.Collections.Generic.Dictionary<string, Variant> context)
    {
        return LogicTree.Evaluate(context);
    }

    /// <summary>
    /// 评估条件是否满足（Godot字典接口）
    /// </summary>
    /// <param name="context">运行时上下文数据</param>
    /// <returns>当所有子节点条件满足时返回true</returns>
    public bool Evaluate(Godot.Collections.Dictionary<string, Variant> context)
    {
        return LogicTree.Evaluate(context);
    }

    /// <summary>
    /// 序列化为JSON格式
    /// </summary>
    /// <returns>包含修正因子和条件树的JObject</returns>
    public JToken ToJson()
    {
        var json = new JObject();
        bool hasRepeat = false;     // 标记是否有重复的键

        foreach (var child in LogicTree.Children)
        {
            var token = child.ToJson();

             // 遇到相同的键停止平铺
            if (json.ContainsKey(token.Name))
            {
                hasRepeat = true;
                break;
            }
            
            json.Add(token);  // 递归序列化子节点（平铺）
        }

        if (hasRepeat)
        {
            json.RemoveAll(); // 清空json
            json.Add(LogicTree.ToJson()); // 递归序列化子节点（包装）
        }
        
        json.AddFirst(new JProperty("factor", Factor));
        return json;
    }

    /// <summary>
    /// 序列化为JSON字符串
    /// </summary>
    /// <returns>包含修正因子和条件树的JSON字符串</returns>
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

    /// <summary>
    /// 从JSON创建修正器实例
    /// </summary>
    /// <param name="obj">包含factor和条件树的JObject</param>
    /// <returns>新创建的ChanceModifier实例</returns>
    public static ChanceModifier FromJson(JObject obj)
    {
        // GD.Print("Parsing modifier:"+obj);
        var modifier = new ChanceModifier();
        if (obj.TryGetValue("factor", out var factor))
            modifier.Factor = (float)factor;

        var conditions = new Array<ConditionNode>();
        obj.Remove("factor"); // 移除factor属性，只保留条件树
        foreach (var prop in obj.Properties())
        {
            // 使用反序列化工具解析每个条件属性
            var node = ConditionNodeDeserializer.DeserializeNode(prop.Name, prop);
            conditions.Add(node);
        }
        modifier.LogicTree = new LogicalAndNode() { Children = conditions };
        return modifier;
    }

    /// <summary>
    /// 从JSON字符串创建修正器实例
    /// </summary>
    /// <param name="obj">包含factor和条件树的Json字符串</param>
    /// <returns>新创建的ChanceModifier实例</returns>
    public static ChanceModifier FromJson(string jsonString)
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
}