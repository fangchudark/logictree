# LogicTree - 概率逻辑树系统

## 介绍

该分支是集成Godot版本LogicTree系统的纯C#实现版本，部分API有所变动

详细介绍见主分支：[README.md](https://github.com/fangchudark/logictree/tree/main/README.md)

## 差异

- 移除了与Godot有关的API与继承关系，所有API都已原生使用C#进行了实现

- 新增了[`NodeDeserializer`特性](/Serializer/NodeDeserializerAttribute.cs)，用于标记如何反序列化节点，该特性只能用在匹配签名的方法上：[IConditionNodeDeserializer接口的FromJson(JToken)方法](/Serializer/IConditionNodeDeserializer.cs/#L17)

- [ConditionNodeDeserializer](/Serializer/ConditionNodeDeserializer.cs/#L68)类现在可以自动在程序开始时查找并注册所有被NodeDeserializer特性标记的方法，无需手动注册

- 反序列化器注册同样的键不再会被覆盖，如果已存在的键与新的键相同，则不会注册新的反序列化器

- [泛型LeafConditionNode类](/Node/LeafConditionNode.cs)、[ComparisonConditionNode类](/Node/ComparisonConditionNode.cs)不再有两个泛型参数，而是一个泛型参数，用于指定值的类型

- 所有叶子节点和比较节点的`ConditionName`属性都已被更改为`string`类型，而不是泛型`TEnum`的枚举类型

- 叶子节点的`ConditionName`属性不再是抽象属性

- 移除了所有除容器节点外的所有节点的无参构造函数

- 所有具体叶子节点类都新增了`string, TValue`，`Enum, TValue`类型的构造函数

- 原Godot的拓展方法（[ToPascalCase()](/Extensions/Extensions.cs/#L19), [ToSnakeCase()](/Extensions/Extensions.cs/#L82),[Mathf.IsEqualApprox(bool, bool)](/Extensions/Extensions.cs/#L138)）都已原生使用C#进行了实现

## 拓展示例

```csharp
public class StringMatchNode : LogicTree.LeafConditionNode<string>, LogicTree.IConditionNodeDeserializer<StringMatchNode>
{
    public override string Value { get; set; }

    public StringMatchNode(string name, string value)
    {
        ConditionName = name.ToSnakeCase();
        Value = value;
    }

    public StringMatchNode(Enum name, string value)
    {
        ConditionName = name.ToSnakeCase();
        Value = value;
    }

    public override JProperty ToJson()
    {
        return ToJProperty(ConditionName, Value);
    }

    public override string ToJsonString()
    {
        return ToJson().ToString();
    }

    [NodeDeserializer(JTokenType.String)]
    public static StringMatchNode FromJson(JToken value)
    {
        return Deserialize(value, (con, val) => new StringMatchNode(con, val));
    }

    public static StringMatchNode FromJson(string jsonString)
    {
        return ConditionNodeDeserializer.FromJsonDefault<StringMatchNode>(jsonString);
    }

    public override bool Evaluate(Dictionary<string, object> context)
    {
        if (context.TryGetValue(ConditionName.ToSnakeCase(), out var value))
        {
            switch(value)
            {
                case string s:
                    return s == Value;
                case IList<string> list:
                    return list.Contains(Value);
            }
        }
        return false;
    }
}
```

## 许可证

[`MIT`](https://mit-license.org/) License