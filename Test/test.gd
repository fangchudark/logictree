extends Control

var json : JSON = preload("res://Test/TestNodeJSON.json")

@export var dict : Dictionary[String, Variant] = {}

func _ready():
	var bool_node = BoolConditionNode.new()
	bool_node.ConditionName = 0 # "Condition3"
	bool_node.Value = true
	var bool_json = bool_node.ToJsonString()
	print(bool_json)
	print(BoolConditionNode.FromJson(bool_json))
	
	var value_node = ValueConditionNode.new()
	value_node.ConditionName = 0 # "Condition1"
	value_node.Value = 100
	var value_json = value_node.ToJsonString()
	print(value_json)
	print(ValueConditionNode.FromJson(value_json))
	
	var more_node = MoreThanNode.new()
	more_node.ConditionName = 1 # "Condition2"
	more_node.Value = 10
	var more_json = more_node.ToJsonString()
	print(more_json)
	print(MoreThanNode.FromJson(more_json))
	
	var or_node = LogicalOrNode.new()
	or_node.Children.append(more_node)
	or_node.Children.append(value_node)
	var or_json = or_node.ToJsonString()
	print(or_json)
	print(LogicalOrNode.FromJson(or_json))
	
	var root_node = LogicalAndNode.new()
	root_node.Children = [bool_node, or_node]
	var root_node_json = root_node.ToJsonString()
	print(root_node_json)
	print(LogicalAndNode.FromJson(root_node_json))
	
	var chance_modifier = ChanceModifier.new()
	chance_modifier.Factor = 2.0
	chance_modifier.LogicTree = root_node
	var chance_modifier_json = chance_modifier.ToJsonString()
	print(chance_modifier_json)
	print(ChanceModifier.FromJson(chance_modifier_json))
	
	var chance = Chance.new()
	chance.Factor = 1.0
	chance.Modifiers.append(chance_modifier)
	var chance_json = chance.ToJsonString()
	print(chance_json)
	print(Chance.FromJson(chance_json))
	
	print("factor:",chance.GetFactor(dict))
