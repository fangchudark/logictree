using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Newtonsoft.Json.Linq;

/// <summary>
/// 条件节点反序列化工具类
/// </summary>
public static class ConditionNodeDeserializer 
{

    private static readonly Dictionary<string, Func<JToken, ConditionNode>> _deserializers = [];
    private static readonly Dictionary<JTokenType, Func<JProperty, ConditionNode>> _valueTypeDeserializers = [];
    static ConditionNodeDeserializer()
    {
        RegisterDeserializer("and", LogicalAndNode.FromJson);      // 逻辑与节点
        RegisterDeserializer("or", LogicalOrNode.FromJson);         // 逻辑或节点
        RegisterDeserializer("not", LogicalNotNode.FromJson);     // 逻辑非节点
        RegisterDeserializer("more_than", MoreThanNode.FromJson);       // 大于比较节点
        RegisterDeserializer("less_than", LessThanNode.FromJson);       // 小于比较节点

        RegisterValueTypeDeserializer(JTokenType.Boolean, BoolConditionNode.FromJson);
        RegisterValueTypeDeserializer(JTokenType.Integer, ValueConditionNode.FromJson);
        RegisterValueTypeDeserializer(JTokenType.Float, ValueConditionNode.FromJson);
    }

    /// <summary>
    /// 反序列化JSON条件节点
    /// </summary>
    /// <param name="key">条件类型标识符</param>
    /// <param name="value">对应的JSON数据</param>
    /// <returns>反序列化后的条件节点对象</returns>
    /// <exception cref="ArgumentException">遇到不支持的条件类型时抛出</exception>
    public static ConditionNode DeserializeNode(string key, JToken value)
    {
        // 调试用反序列化日志
        // Godot.GD.Print("PARSE:"+key);
        // Godot.GD.Print("PARSE:"+value.Type);
        // Godot.GD.Print("PARSE:"+value.ToString());        
        
        if (value == null)
            throw new ArgumentNullException(nameof(value), $"Value for key '{key}' is null.");

        // 优先尝试从注册的反序列化器中查找
        if (_deserializers.TryGetValue(key, out var deserializer))
        {
            return deserializer(value);
        }

        if (value is JProperty prop && _valueTypeDeserializers.TryGetValue(prop.Value.Type, out var typeDeserializer))
        {
            return typeDeserializer(prop);
        }

        throw new ArgumentException($"Unsupported value type for key '{key}'.");
    }

    /// <summary>
    /// 注册包含标识符的节点反序列化器（相同的键会被顶替）
    /// </summary>
    /// <param name="key">条件类型标识符</param>
    /// <param name="deserializer">反序列化器</param>
    public static void RegisterDeserializer(string key, Func<JToken, ConditionNode> deserializer)
    {
        _deserializers[key] = deserializer;
    }

    /// <summary>
    /// 注册不包含标识符的节点反序列化器（相同的类型会被顶替）
    /// </summary>
    /// <param name="valueType">节点的Json类型</param>
    /// <param name="deserializer">反序列化器</param>
    public static void RegisterValueTypeDeserializer(JTokenType valueType, Func<JProperty, ConditionNode> deserializer)
    {
        _valueTypeDeserializers[valueType] = deserializer;
    }

    /// <summary>
    /// 清空节点反序列化器  
    /// </summary>
    public static void Clear()
    {
        _deserializers.Clear();
        _valueTypeDeserializers.Clear();
    }


    /// <summary>
    /// 从JSON对象解析子条件集合
    /// </summary>
    /// <param name="obj">父级JSON对象</param>
    /// <param name="children">用于存储子条件的集合</param>
    public static void ParseChildrenFromObject(JObject obj, Godot.Collections.Array<ConditionNode> children)
    {
        if (obj == null || !obj.HasValues)
            return;

        foreach (var prop in obj.Properties())
        {
            var child = DeserializeNode(prop.Name, prop); // 递归解析子条件
            if (child != null)
                children.Add(child); // 将有效子条件添加到集合中
        }
    }

    /// <summary>
    /// <see cref="IConditionNodeDeserializer{T}.FromJson(string)"/>的默认实现，包含异常处理，调用<see cref="IConditionNodeDeserializer{T}.FromJson(JToken)"/>并转化为字符串
    /// </summary>
    /// <typeparam name="T">继承了反序列化接口的节点</typeparam>
    /// <param name="jsonString">json字符串</param>
    /// <returns>新实例化的节点，失败返回 default</returns>
    public static T FromJsonDefault<T>(string jsonString)
        where T : IConditionNodeDeserializer<T>
    {
        try
        {
            var jobject = JObject.Parse(jsonString);
            var prop = jobject.Properties().First(); // 拿出第一个 property
            return T.FromJson(prop);
        }
        catch (Exception e)
        {
            GD.PrintErr("Failed to parse json:", e);
            return default;
        }
    }
}
