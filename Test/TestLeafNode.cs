using System.Collections.Generic;
using Godot;
using Newtonsoft.Json.Linq;

/// <summary>
/// 测试用例节点
/// </summary>
[GlobalClass]
public partial class TestLeafNode : LeafConditionNode<ChanceNumberConditionType, int>, IConditionNodeDeserializer<TestLeafNode>
{
    [Export]
    public override ChanceNumberConditionType ConditionName {get; set;}
    [Export]
    public override int Value { get ; set ; }
    public TestLeafNode()
    {
        ConditionNodeDeserializer.RegisterDeserializer(ConditionName.ToSnakeCase(), FromJson);
    }

    public TestLeafNode(ChanceNumberConditionType conditionName, int value)
    {
        ConditionName = conditionName;
        Value = value;
    }

    public static TestLeafNode FromJson(JToken value)
    {
        return Deserialize(
            value, 
            (con, val) => new TestLeafNode(con, val)
        );
    }

    public override bool Evaluate(Dictionary<string, object> context)
    {
        return true;
    }

    public override JProperty ToJson()
    {
        return ToJProperty(ConditionName, Value);
    }
}