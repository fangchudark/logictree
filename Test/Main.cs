using Godot;
using Newtonsoft.Json.Linq;
using Godot.Collections;

public partial class Main : Control
{
    [Export]
    Chance chance;

    [Export]
    Json json;

    [Export]
    Dictionary<string, Variant> context = new()
    {
        [ChanceNumberConditionType.Condition1.ToSnakeCase()] = 1,
        [ChanceNumberConditionType.Condition2.ToSnakeCase()] = 1,
        [ChanceBoolConditionType.Condition3.ToSnakeCase()] = true,
        [ChanceBoolConditionType.Condition4.ToSnakeCase()] = true,
        [ChanceNumberConditionType.Condition5.ToSnakeCase()] = 1,
        [ChanceNumberConditionType.Condition6.ToSnakeCase()] = 1
    };

    public override void _Ready()
    {
        chance ??= ResourceLoader.Load<Chance>("res://Test/chance.tres");
        json ??= ResourceLoader.Load<Json>("res://Test/JSON.json");
        var testNodeJson = ResourceLoader.Load<Json>("res://Test/TestNodeJSON.json");

        TestAndNode(context);
        TestOrNode(context);
        TestNotNode(context);
        TestMoreThanNode(context);
        TestLessThanNode(context);
        TestNestingNode(context);
        TestSerialization(chance);
        TestDeserialization(testNodeJson.Data.ToString());
        TestDeserialization(json.Data.ToString());
    }

    void TestAndNode(Dictionary<string, Variant> context)
    {
        string json = @"{
            ""factor"" : 1,
            ""modifiers"": [
                {
                    ""factor"": 2,
                    ""condition_1"" : 100,
                    ""condition_2"" : 200,
                    ""condition_3"" : true,
                    ""condition_4"" : false
                }
            ]
        }";

        Chance chance = Chance.FromJson(JObject.Parse(json));
        GD.Print("TestAndNode:" + chance.ToJson().ToString());
        GD.Print($"Apply factor {chance.GetFactor(context)}");
        
    }

    void TestOrNode(Dictionary<string, Variant> context)
    {
        string json = @"{
            ""factor"" : 1,
            ""modifiers"": [
                {
                    ""factor"": 2,
                    ""or"": {
                        ""condition_1"" : 100,
                        ""condition_2"" : 200,
                        ""condition_3"" : true,
                        ""condition_4"" : false
                    }
                }
            ]
        }";

        Chance chance = Chance.FromJson(JObject.Parse(json));
        GD.Print("TestOrNode:" + chance.ToJson().ToString());
        GD.Print($"Apply factor {chance.GetFactor(context)}");
    }

    void TestNotNode(Dictionary<string, Variant> context)
    {
        string json = @"{
            ""factor"" : 1,
            ""modifiers"" : [
                {
                    ""factor"" : 2,
                    ""not"" : {
                        ""condition_1"" : 100,
                        ""condition_2"" : 200,
                        ""condition_3"" : true,
                        ""condition_4"" : false
                    }
                }
            ]
        }";

        Chance chance = Chance.FromJson(JObject.Parse(json));
        GD.Print("TestNotNode:" + chance.ToJson().ToString());
        GD.Print($"Apply factor {chance.GetFactor(context)}");
    }

    void TestMoreThanNode(Dictionary<string, Variant> context)
    {
        string json = @"{
            ""factor"" : 1,
            ""modifiers"" : [
                {
                    ""factor"" : 2,
                    ""more_than"" : {
                        ""condition_1"" : 100,                        
                    }
                }
            ]
        }";        
        var chance = Chance.FromJson(JObject.Parse(json));
        GD.Print("TestMoreThanNode:" + chance.ToJson().ToString());
        GD.Print($"Apply factor {chance.GetFactor(context)}");
    }

    void TestLessThanNode(Dictionary<string, Variant> context)
    {
        string json = @"{
            ""factor"" : 1,
            ""modifiers"" : [
                {
                    ""factor"" : 2,
                    ""less_than"" : {
                        ""condition_1"" : 100,                        
                    }
                }
            ]
        }";        
        var chance = Chance.FromJson(JObject.Parse(json));
        GD.Print("TestLessThanNode:" + chance.ToJson().ToString());
        GD.Print($"Apply factor {chance.GetFactor(context)}");
    }

    void TestNestingNode(Dictionary<string, Variant> context)
    {
        var json = @"{
            ""factor"" : 1,
            ""modifiers"" : [
                {
                    ""factor"" : 2,
                    ""or"":{
                        ""not"":{
                            ""condition_1"":100
                        },
                        ""more_than"":{
                            ""condition_2"":100
                        }
                    }
                },
                {
                    ""factor"" : 3,
                    ""or"":{
                        ""condition_3"":true,
                        ""condition_4"":true
                    },
                    ""not"":{
                        ""condition_5"":100,
                    },
                    ""less_than"":{
                        ""condition_6"":100
                    },
                    ""condition_1"":100
                },
                {
                    ""factor"":4,
                    ""or"":{
                        ""more_than"":{
                            ""condition_2"":100
                        },
                        ""less_than"":{
                            ""condition_5"":100
                        },
                        ""condition_6"":100
                    }
                }
            ]
        }";

        var chance = Chance.FromJson(JObject.Parse(json));
        GD.Print("TestNestingNode:" + chance.ToJson().ToString());
        GD.Print($"Apply factor {chance.GetFactor(context)}");
    }

    void TestComplexNode(Dictionary<string, Variant> context)
    {
        var json = @"{
            ""factor"" : 1,
            ""modifiers"" : [
                {
                    ""factor"": 2,
                    ""or"": [
                        {
                            ""not"" : {
                                ""more_than"":{
                                    ""condition_1"":100,
                                },
                            }
                        },
                        {
                            ""not"" : {
                                ""more_than"":{
                                    ""condition_2"":100,
                                }
                            }
                        }
                    ]
                },
                {
                    ""factor"": 3,
                    ""or"": [
                        {
                            ""more_than"":{
                                ""condition_5"":100,
                            },
                        },
                        {
                            ""more_than"":{
                                ""condition_6"":100,
                            }
                        }
                    ]
                },
                {
                    ""factor"": 4,
                     ""not"": { 
                        ""and"" : [
                            {
                                ""more_than"":{
                                    ""condition_1"":100
                                }
                            },
                            {
                                ""more_than"":{
                                    ""condition_2"":100
                                }
                            }
                        ]
                    }                   
                }
            ]
        }";        

        var chance = Chance.FromJson(JObject.Parse(json));
        GD.Print("TestNestingNode:" + chance.ToJson().ToString());
        GD.Print($"Apply factor {chance.GetFactor(context)}");
    }

    void TestSerialization(Chance chance)
    {
        GD.Print("TestSerialization:" + chance.ToJson().ToString());
    }

    void TestDeserialization(string json)
    {
        var chance1 = Chance.FromJson(JObject.Parse(json));
        GD.Print("TestDeserialization:" + chance1.ToJson().ToString());
        foreach (var m in chance1.Modifiers)
        {
            GD.Print($"factor: {m.Factor}");
            GD.Print($"root node type :{m.LogicTree.GetType().Name}");
            GD.Print($"children node type :{new Array<string>(m.LogicTree.GetChildrenTypes(true))}");
            GD.Print($"root node instance : {m.LogicTree}");
            GD.Print($"children node instance : {new Array<ConditionNode>(m.LogicTree.GetAllChidren(true))}");
        }
        GD.Print($"Apply factor {chance1.GetFactor(context)}");
    }
}
