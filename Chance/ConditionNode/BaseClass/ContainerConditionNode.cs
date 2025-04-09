using Godot;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

/// <summary>
/// 容器条件节点基类（所有包含子条件的节点父类）<br/>
/// Json结构（当子节点键不重复时）：
/// <code>
/// {
///     "container_name": {  
///         "condition_a": 1,
///         "condition_b": true
///     }
/// }
/// 或（当子节点键重复时）
/// {
///     "container_name": [ 
///         { "condition_a": 1 },
///         { "condition_a": 2 }
///     ]
/// }
/// </code>
/// </summary>
[GlobalClass]
public abstract partial class ContainerConditionNode : ConditionNode
{
    /// <summary>
    /// 子条件集合（使用Godot数组便于编辑器配置）
    /// </summary>
    [Export]
    public Godot.Collections.Array<ConditionNode> Children {get; set;} = [];

    /// <summary>
    /// 获取所有子节点类型名称(调试用)
    /// </summary>
    /// <param name="recurve">是否递归调用</param>
    /// <returns>包含所有子节点类型名称数组</returns>
    public string[] GetChildrenTypes(bool recurve = false)
    {
        var types = new List<string>();
        foreach (var child in Children)
        {
            types.Add(child.GetType().Name);
            if (recurve && child is ContainerConditionNode con)
                types.AddRange(con.GetChildrenTypes(recurve));
        }
        return [.. types];
    }

    /// <summary>
    /// 获取所有子节点
    /// </summary>
    /// <param name="recurve">是否递归调用获取容器节点的子节点</param>
    /// <returns>包含所有子节点的数组</returns>
    public ConditionNode[] GetAllChidren(bool recurve = false)
    {
        var all = new List<ConditionNode>();
        foreach (var child in Children)
        {
            all.Add(child);
           if (recurve && child is ContainerConditionNode con)
                all.AddRange(con.GetAllChidren(recurve));
        }
        return [.. all];
    }

    /// <summary>
    /// 定义JSON序列化的子节点容器键名
    /// </summary>
    protected virtual string ContainerName => "container";

    /// <summary>
    /// 序列化子节点集合的通用逻辑
    /// </summary>
    /// <remarks>
    /// 自动处理重复键情况：<br/>
    /// 1. 当子节点键名不重复时，生成JObject结构 <br/>
    /// 2. 当检测到重复键时，自动转换为JArray结构
    /// </remarks>
    /// <returns>包装在指定容器名称下的JProperty对象</returns>
    protected virtual JProperty SerializeChildren()
    {
        var obj = new JObject();
        var array = new JArray();
        bool hasRepeat = false;     // 标记是否有重复的键

        foreach (var child in Children)
        {
            // 跳过空节点
            if (child is null)
                continue;

            var token = child.ToJson();
            
            // 跳过无效的子节点
            if (token is not JProperty prop)
                continue;

             // 检测到重复条件键时切换为数组模式
            if (!hasRepeat && obj.ContainsKey(prop.Name))
            {
                // 进入 array 模式，先把已有的都拆开放进去
                foreach (var kv in obj)
                {
                    array.Add(new JObject(new JProperty(kv.Key, kv.Value)));
                }
                hasRepeat = true;
            }

            if (hasRepeat)
            {
                array.Add(new JObject(prop));  // 数组模式追加新条件
            }
            else
            {
                obj.Add(prop);  // 对象模式直接添加属性
            }
        }

        return new JProperty(ContainerName,  hasRepeat ? array : obj);
    }

    /// <summary>
    /// 容器节点反序列化通用方法
    /// </summary>
    /// <typeparam name="T">继承自ContainerConditionNode的具体类型</typeparam>
    /// <param name="value">输入的JSON数据</param>
    /// <param name="containerName">预期的容器键名（如"and"/"or"/"not"）</param>
    /// <param name="factory">节点实例创建方法</param>
    /// <returns>实例化的容器节点</returns>
    /// <exception cref="ArgumentException">
    /// 当遇到以下情况时抛出：
    /// 1. 输入值不是有效的JProperty对象
    /// 2. 属性名称与预期容器名称不匹配
    /// </exception>
    protected static T Deserialize<T>(JToken value, string expectedName, Func<T> createNode) 
        where T : ContainerConditionNode
    {
        var prop = value as JProperty 
            ?? throw new ArgumentException($"Container must be a property.");
        
        if (prop.Name != expectedName)
            throw new ArgumentException($"Container must have a '{expectedName}' property");

        var node = createNode();
        switch (prop.Value)
        {
            case JObject obj:    // 处理对象格式的多个条件
                ConditionNodeDeserializer.ParseChildrenFromObject(obj, node.Children);
                break;
            case JArray array: // 处理数组格式的条件集合
                foreach (var item in array)
                {
                     // 验证数组元素必须为JObject类型
                    if (item is not JObject objItem)
                        throw new ArgumentException("Children must be objects.");

                    ConditionNodeDeserializer.ParseChildrenFromObject(objItem, node.Children);
                }
                break;

            default: // 无效的类型
                throw new ArgumentException("Invalid children container type.");
        }

        return node;
    }
}
