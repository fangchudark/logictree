# LogicTree - 概率逻辑树系统

基于Godot引擎实现的动态概率计算系统，通过可组合的逻辑树结构实现灵活的条件判断和概率因子计算。支持基于上下文的条件逻辑评估和动态权重调整。通过组合逻辑运算符和业务条件，可构建复杂的决策树，适用于游戏机制、AI决策等场景。

![Godot Version](https://img.shields.io/badge/Godot-4.4%2B-%23478cbf)

## 特性

- 🧩 模块化条件节点

	- 逻辑节点：AND/OR/NOT
  - 叶子节点：
	- 布尔/数值条件节点
	- 比较节点：> / < / =
- 🔄 完整序列化支持
  - 通过JSON配置复杂条件树
  - 运行时动态加载/修改配置
  - 可视化编辑器集成
- 📈 动态因子计算
  - 多层条件嵌套评估
  - 多修正器叠加计算
  - 上下文敏感的参数传递
- ✅ GDScript跨脚本调用支持
  - 提供Godot兼容API


## 快速开始

### 环境要求
- [Godot .NET 4.4+](https://godotengine.org/download/windows/)
- [.NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Newtonsoft.Json 13.0+](https://www.newtonsoft.com/json)

### 导入步骤

1. 克隆仓库
 ```bash
git clone https://github.com/fangchudark/logictree.git
```

2. 安装依赖  

	以下两种方法均可：

	1. 使用NuGet包：
	```bash
	dotnet add package Newtonsoft.Json --version 13.0.3
	```

	2. 直接添加到项目 `.csproj` 文件中：

		在 `.csproj` 文件中添加以下内容：
		```xml
		<ItemGroup>
			<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
		</ItemGroup>
		```

3. 将 `Chance`文件夹复制到项目根目录

4. 配置枚举（示例）：
```csharp
// ChanceConditionType.cs
public enum ChanceBoolConditionType { 
	IsRaining,      // 是否下雨
	HasKeyItem      // 持有关键道具
}
```

## 使用示例
1. 创建条件配置（JSON示例）：
```json
{
  "factor": 1.0,
  "modifiers": [
	{
	  "factor": 2.0,
	  "or": [
		{ "more_than": { "player_level": 5 } },
		{ "condition_met": true }
	  ],
	  "not": { "enemy_alive": true }
	}
  ]
}
```
2. 加载配置并计算概率：
```csharp
// 从JSON创建实例
var json = ResourceLoader.Load<JSON>("res://path/to/json/file.json");
var jsonString = json.Data.ToString();
var chance = Chance.FromJson(JObject.Parse(jsonString));

// 创建上下文参数
// 使用提供的枚举拓展方法 ToSnakeCase()，将枚举值转换为蛇形命名作为字典的键
var context = new Dictionary<string, object>()
{
	[ChanceNumberConditionType.PlayerLevel.ToSnakeCase()] = 10,
	[ChanceBoolConditionType.ConditionMet.ToSnakeCase()] = true,
	[ChanceBoolConditionType.EnemyAlive.ToSnakeCase()] = false
};

// 获取最终因子
float finalFactor = chance.GetFactor(context);
GD.Print($"修正因子: {finalFactor}");

```
> 也可以在编辑器中新建并配置`Chance`资源，并使用`ResourceLoader.Load<Chance>`来直接创建`Chance`实例

## 语义说明

### 逻辑节点

逻辑与不需要显式标记，因为其是整个逻辑树的根节点，其他逻辑节点均需显式标记。

如果逻辑节点的子节点包含相同的键，则逻辑与必须显式标记并使用数组结构  

处于数组结构中时，需要包装为对象

#### 逻辑与

显式标记：
```json
{
  "factor": 1.0,
  "modifiers": [
	{
		"factor": 2.0,
		"and":[
		  { 
			"more_than":{ "player_hp":500 } 
		  },
		  { 
			"enemy_alive":true 
		  },
		  { 
			"more_than":{ "player_level":5 } 
		  }
		]
	},
	{
	  "factor": 1.5,
	  "and":{
		"more_than":{ "player_level":5 },
		"enemy_alive":true
	  }
	}
  ]
}
	
```
隐式标记：
```json
{
  "factor": 1.0,
  "modifiers": [
	{
		"factor": 2.0,
		"enemy_alive":true,
		"more_than":{ "player_level":5 }
	}
  ]
}
```

#### 逻辑或

```json
{
  "factor": 1.0,
  "modifiers": [
	{
		"factor": 2.0,
		"or":[
		  { 
			"more_than":{ "player_hp":500 }
		  },
		  { 
			"enemy_alive":true
		  },
		  { 
			"more_than":{ "player_level":5 }
		  }
		]
	},
	{
	  "factor": 1.5,
	  "or":{
		"more_than":{ "player_level":5 },
		"enemy_alive":true
	  }
	}
  ]
}
```

#### 逻辑非

```json
{
  "factor": 1.0,
  "modifiers": [
	{
		"factor": 2.0,
		"not":[
		  { "more_than":{ "player_hp":500 }},
		  { "enemy_alive":true },
		  { "more_than":{ "player_level":5 } }
		]
	},
	{
	  "factor": 1.5,
	  "not":{
		"more_than":{ "player_level":5 },
		"enemy_alive":true
	  }
	}
  ]
}
```

### 条件节点

最基础的节点，当处于数组模式的容器中时，则该节点必须包装为对象

#### 布尔条件节点


```json
{
  "factor": 1.0,
  "modifiers": [
	{
		"factor": 2.0,
		"enemy_alive":true
	},
	{
	  "factor": 1.5,
	  "or":[
		{ "enemy_alive":false },
		{ "player_alive":false }
	  ]
	}
  ]
}
```

#### 数值条件节点

> 条件与期望值相等或近似相等时返回true

```json
{
  "factor": 1.0,
  "modifiers": [
	{
		"factor": 2.0,
		"enemy_hp":100
	},
	{
	  "factor": 1.5,
	  "or":[
		{ "enemy_hp":400 },
		{ "player_hp":100 }
	  ]
	}
  ]
}
```

#### 数值大于条件节点

> 条件大于期望值时返回true

```json
{
  "factor": 1.0,
  "modifiers": [
	{
		"factor": 2.0,
		"more_than":{ "enemy_hp":100 },
	},
	{
	  "factor": 1.5,
	  "or":[
		{ "more_than": {"enemy_hp":400 }},
		{ "more_than": {"player_hp":100 }}
	  ]
	}
  ]
}
```

#### 数值小于条件节点

> 条件小于期望值时返回true

```json
{
  "factor": 1.0,
  "modifiers": [
	{
		"factor": 2.0,
		"less_than":{ "enemy_hp":100 },
	},
	{
	  "factor": 1.5,
	  "or":[
		{ "less_than": {"enemy_hp":400 }},
		{ "less_than": {"player_hp":100 }}
	  ]
	}
  ]
}
```

### 复杂条件示例 :

**示例 1：玩家生命值大于500且敌人存活，并且玩家等级大于5**

```json
{
  "factor": 1.0,
  "modifiers": [
	{
	  "factor": 2.0,
	  "and": [
		{ "more_than": { "player_hp": 500 } },
		{ "enemy_alive": true },
		{ "more_than": { "player_level": 5 } }
	  ]
	}
  ]
}
```
> 这个示例在玩家生命值高于500，敌人存活且玩家等级高于5时会增加修正因子
 
**示例 2：敌人生命值小于300且玩家等级大于5，或者玩家没有关键道具**

```json
{
  "factor": 1.0,
  "modifiers": [
	{
	  "factor": 2.0,
	  "or":[
		{
		  "and": [
			{ "less_than": { "enemy_hp": 300 } },
			{ "more_than": { "player_level": 5 } }
		  ]
		},
		{
		  "has_key_item": false
		}
	  ]
	}
  ]
}
```
> 这个例子展示了玩家的等级和敌人生命值的组合，另外还考虑了玩家是否持有关键道具，满足任意一个条件即可触发修正因子。

**示例 3：玩家生命值大于500，并且敌人不是存活状态，或者敌人等级低于5并且玩家等级高于10**

```json
{
  "factor": 1.0,
  "modifiers": [
	{
	  "factor": 2.0,
	  "or": [
		{
		  "and": [
			{ "more_than": { "player_hp": 500 } },
			{ "not": { "enemy_alive": true } }
		  ]
		},
		{ 
		  "and": [
			  { "more_than": { "enemy_level": 5 } },
			  { "more_than": { "player_level": 10 } }
			]
		}
	  ]
	}
  ]
}

```

> 这个例子展示了如何通过逻辑与和逻辑非以及逻辑或的结合，设定较为复杂的条件。

**示例 4：在敌人存活的情况下，玩家等级必须大于5且拥有关键道具；如果敌人死亡，则玩家生命值小于100时触发**

```json
{
  "factor": 1.0,
  "modifiers": [
	{
	  "factor": 2.0,
	  "and": [
		{ "enemy_alive": true },
		{ "more_than": { "player_level": 5 } },
		{ "has_key_item": true }
	  ]
	},
	{
	  "factor": 1.5,
	  "and": [
		{ "not": { "enemy_alive": true } },
		{ "less_than": { "player_hp": 100 } }
	  ]
	}
  ]
}

```

> 这个例子展示了如何在满足某些条件的情况下，触发不同的修正因子。敌人死亡且玩家生命值低于100时，会触发1.5倍修正因子；敌人存活在且玩家等级高于5时并且持有关键道具时，会触发2倍修正因子。

## 扩展开发

1. 继承 [`LeafConditionNode<TEnum, TValue>`](/Chance/ConditionNode/BaseClass/Generic/LeafConditionNode.cs) / [`ContainerConditionNode`](/Chance/ConditionNode/BaseClass/ContainerConditionNode.cs) / [`ConditionNode`](/Chance//ConditionNode/BaseClass/ConditionNode.cs)
2. 实现评估逻辑`bool Evaluate(Dictionary<string, object> context)`
3. 实现序列化逻辑`JProperty ToJson()` 
4. 继承[`IConditionNodeDeserializer <TSelf>`](/Chance/Deserializer/IConditionNodeDeserializer.cs)接口
5. 实现反序列化逻辑`static TSelf FromJson(JToken value)` `static TSelf FromJson(string jsonString)`
6. 调用`RegisterDeserializer(string key, Func<JToken, ConditionNode> deserializer)` / `RegisterValueTypeDeserializer(JTokenType valueType, Func<JProperty, ConditionNode> deserializer)` 方法注册反序列化器到 [`ConditionNodeDeserializer`](/Chance/Deserializer/ConditionNodeDeserializer.cs)

> 反序列化节点时会优先在 `键注册表(RegisterDeserializer)` 查找对应反序列化器，并实例化对应实例；  
> 如果`键注册表`没有找到对应反序列化器，则会在 `值类型注册表(RegisterValueTypeDeserializer)` 查找对应反序列化器来创建实例；  
> 如果都找不到，则会抛出`ArgumentException`异常。

例子：
```csharp

// 继承泛型叶子节点和反序列化接口
[GlobalClass]
public class CustomConditionNode : LeafConditionNode<ChanceNumberConditionType, int>, IConditionNodeDeserializer<CustomConditionNode>
{
	// 当有构造函数重载时，必须要有一个无参构造函数！
	// 反序列化用的构造函数
	public CustomConditionNode(ChanceNumberConditionType conditionName, int value)
	{
		ConditionName = conditionName;
		Value = value;
	}

	// 实现评估逻辑
	public override bool Evaluate(System.Collections.Generic.Dictionary<string, object> context)
	{
		// 这里就自己写实现吧，只要条件匹配的情况下返回一个true就行
	}

	// 实现将节点序列化成 Json属性的逻辑
	public JProperty ToJson()
	{
		// 如果没有特殊需求可以直接使用泛型基类提供的序列化方法
		return ToJProperty(ConditionName, Value);
		// 如果要将叶子节点的反序列化器带标识符注册到反序列器注册表中，推荐使用另一个默认实现，以防键名重复顶替掉注册反序列器
		// 将会序列化为如下结构 : {"key":{"condition_name":value}}
		// return ToJPropertyWithKey("key", ConditionName, Value)

	}

	// 实现 JSON 还原成节点的逻辑
	public static CustomConditionNode FromJson(JToken value)
	{
		// 如果没有特殊需求可以直接使用泛型基类提供的反序列化方法
		return Deserialize(
		value, 
		(con, val) => new CustomConditionNode(con, val)
		);
		// 如果序列化逻辑没有使用默认实现，则这里也需要手动实现反序列化逻辑
		// 如果在ToJson方法使用了另一个默认实现，这里也需要更改
		// return DeserializeWithKey(
		//   value, 
		//   (con, val) => new CustomConditionNode(con, val)
		// );
	}


	// 如果没有特殊需求可以直接使用反序列化静态类提供的默认实现
	// 这个方法将会提供给GDScript使用，手动实现时务必进行异常处理
	public static CustomConditionNode FromJson(string jsonString)
	{
		return ConditionNodeDeserializer.FromJsonDefault<CustomConditionNode>(jsonString);
	}

	// 推荐使用 RegisterDeserializer 注册带有明确标识符的条件节点（无论是否为叶子节点）
	// 如果使用 RegisterValueTypeDeserializer 注册相同类型的节点，后者会覆盖前者！务必注意不要冲突！

	// 示例1：使用显式标识符注册，更安全可控，叶子节点如果没有重写序列化逻辑或使用WithKey的默认实现，则使用标识符注册时必须要使用蛇形命名的枚举名作为键
	// 由于同一个叶子节点实例的`ConditionName`属性值可能都不相同，所以这个注册方式必须放在节点的构造函数中，除非重写了节点的序列化逻辑，否则可能导致反序列化失败！
	// 如果节点注册时，存在同样的键，则会顶替掉先前注册的反序列化器，务必注意！
	// 由于在构造函数中注册是运行时注册，所以这样极有可能出现运行时键被顶替的情况！
	public CustomConditionNode()
	{
		ConditionNodeDeserializer.RegisterDeserializer(ConditionName.ToSnakeCase(), CustomConditionNode.FromJson);
		// 如序列化使用了自定义键时，则这里的键名必须和WithKey方法或自定义设置的键名相同
		// ConditionNodeDeserializer.RegisterDeserializer("key", CustomConditionNode.FromJson);
	}

}

// 示例2：使用值类型注册，如果已有注册，同类型会被顶替
// 下面的注册将使所有整数类型的叶子节点都使用 CustomConditionNode 替代原有的 ValueConditionNode
// 使用值类型注册只需要注册一次即可

// 在游戏初始化时，添加到初始化函数中，或添加到ConditionNodeDeserializer.cs的静态构造函数中

// ConditionNodeDeserializer.cs
static ConditionNodeDeserializer()
{
	// ...
	ConditionNodeDeserializer.RegisterValueTypeDeserializer(JTokenType.Integer, CustomConditionNode.FromJson);
	// 序列化使用了自定义键时，拓展的节点也就可以在这里注册，其键名必须和WithKey方法或自定义设置的键名相同
	// ConditionNodeDeserializer.RegisterDeserializer("key", CustomConditionNode.FromJson);
}

```
## 类结构概览

### 条件节点体系
|类名|	类型|	功能描述
|---|---|---
[`ConditionNode`](/Chance/ConditionNode/BaseClass/ConditionNode.cs)|	抽象节点|	所有条件节点的基类
[`ContainerConditionNode`](/Chance/ConditionNode/BaseClass/ContainerConditionNode.cs)| 抽象节点|	所有容器节点的基类
[`LeafConditionNode`](/Chance/ConditionNode/BaseClass/LeafConditionNode.cs)|抽象节点|所有叶子节点的基类
[`LeafConditionNode<TEnum, TValue>`](/Chance/ConditionNode/BaseClass/Generic/LeafConditionNode.cs)|	抽象节点|	所有叶子节点的泛型基类
[`ComparisonConditionNode<TEnum, TValue>`](/Chance/ConditionNode/BaseClass/Generic/ComparisonConditionNode.cs)|抽象叶子节点| 所有数值比较节点的泛型基类
[`LogicalAndNode`](/Chance/ConditionNode/LogicalAndNode.cs)|	容器节点|	所有子条件必须满足
[`LogicalOrNode`](/Chance/ConditionNode/LogicalOrNode.cs)|	容器节点|	任一子条件满足即可
[`LogicalNotNode`](/Chance/ConditionNode/LogicalNotNode.cs)|	容器节点|	所有子条件必须都不满足
[`MoreThanNode`](/Chance/ConditionNode/MoreThanNode.cs)|	比较节点|	数值大于比较
[`LessThanNode`](/Chance/ConditionNode/LessThanNode.cs)|	比较节点|	数值小于比较
[`ValueConditionNode`](/Chance/ConditionNode/ValueConditionNode.cs)|	叶子节点|	数值等于比较（近似判断）
[`BoolConditionNode`](/Chance/ConditionNode/BoolConditionNode.cs)|	叶子节点|	布尔值匹配

#### 容器节点

容器节点允许两种Json结构  
当存在重复的键时，值将会使用数组结构  
默认情况下值会使用对象结构  
容器节点需要其容器名作为键


对象结构：
```json
{
	"container_name":{
		"key":{
			"other_key" : 1
		},
		"another_key": 1,
	}
}
```
数组结构：
```json
{
	"container_name":[
		{
			"key":{
				"other_key":1
			}
		},
		{
			"another_key":1
		}
	]
}
```

####  叶子节点

叶子节点使用最简单的json结构  

```json
{
	"key": 1
}
```

#### 比较节点

比较节点属于叶子节点的子类，值使用对象结构，需要运算符关键字作为键   
**对象结构中只能有一个属性**

```json
{
	"operator": {
		"key": 1
	}
}
```

## 常见问题

#### 编辑器可视化编辑`Chance`资源时多个`ChanceModifier`的`LogicTree`属性同步更改

> *对`LogicTree`属性使用 **唯一化** 或者直接使用`LogicalAndNode`资源文件*

---

#### 无法匹配字典中的上下文

> *如果是使用的 Json 配置，检查 Json 配置中的条件是否是蛇形命名(`player_level`)  
> 确认条件存在于[`ChanceConditionType.cs`](/Chance/ChanceConditionType.cs)对应的枚举中且对应的值类型匹配  
> 确认提供的字典上下文键存在于[`ChanceConditionType.cs`](/Chance/ChanceConditionType.cs)对应的枚举，且是蛇形命名*  
>
> **向字典提供上下文时，最好使用[`ChanceConditionType.cs`](/Chance/ChanceConditionType.cs)提供的枚举的拓展方法`ToSnakeCase()`(见[使用示例](#使用示例))**

---

#### 能否使用 Godot 的字典来提供上下文

> *可以的，节点基类的抽象方法`abstract bool Evaluate(System.Collections.Generic.Dictionary<string, object> context)` 有两个虚方法重载 ：*  
> `virtual bool Evaluate(System.Collections.Generic.Dictionary<string, Variant> context)`  
> `virtual bool Evaluate(Godot.Collections.Dictionary<string, Variant> context)`  
>  
> *当子类实现抽象方法时，其余两个重载会使用另一个虚方法：  
> `virtual Dictionary<string, object> DictConvert(IDictionary<string, Variant> context)`  
> 筛选有效类型并将Godot兼容字典转化为C#原生字典然后使用抽象方法的逻辑，*  
>
> *当然，你也可以重写这两个虚方法以及转换Godot兼容字典的方法，使用自定义逻辑*

---

#### 我可以使用`GDScript`来在代码中创建逻辑树以及使用相关API吗？

> *完全可以！这个项目完全兼容GDScript的跨脚本调用，专为GDScript提供了一系列兼容Godot API*
> *下面是使用GDScript创建[文档中一个示例逻辑树结构](#数值条件节点)的示例*  
> *[更多示例代码](/Test/test.gd)*
>  ```gdscript
>  extends Control
>
>  @export var context : Dictionary[String, Variant] = {}
>
>  func _ready():
>    var root_node_1 = LogicalAndNode.new()
>    var root_node_2 = LogicalAndNode.new()
>    
>    # 必须要和ChanceConditionType.cs中配置的枚举值一致！
>    # public enum ChanceNumberConditionType
>    # {
>    #	  PlayerLevel,
>    #	  PlayerHp,
>    #	  EnemyLevel,
>    #	  EnemyHp,
>    # }
>    var more_node = MoreThanNode.new()
>    more_node.ConditionName = 1 # PlayerHp
>    more_node.Value = 100
>    
>    root_node_1.Children.append(more_node)
>    
>    var modifier_1 = ChanceModifier.new()
>    modifier_1.Factor = 2.0
>    modifier_1.LogicTree = root_node_1
>
>    var or_node = LogicalOrNode.new()
>    
>    var value_node_1 = ValueConditionNode.new()
>    var value_node_2 = ValueConditionNode.new()
>    value_node_1.ConditionName = 1 # PlyaerHp
>    value_node_1.Value = 400
>    value_node_2.ConditionName = 3 # EnemyHp
>    value_node_2.Value = 100
>    
>    or_node.Children = [value_node_1, value_node_2]
>    root_node_2.Children = [or_node]
>
>    var modifier_2 = ChanceModifier.new()
>    modifier_2.Factor = 1.5
>    modifier_2.LogicTree = root_node_2
>
>    var chance = Chance.new()
>    chance.Factor = 1.0
>    chance.Modifiers = [modifier_1, modifier_2]
>    
>    var json = chance.ToJsonString()
>    print("factor:",chance.GetFactor(context))
>    print("chance_to_json_string:", json)
>    print("chance_from_json:", Chance.FromJson(json))
>    print("modifier_1 children:", modifier_1.LogicTree.GetAllChidren(true))
>    print("modifier_1 children(type):", modifier_1.LogicTree.GetChildrenTypes(true))
>    print("modifier_2 children:", modifier_2.LogicTree.GetAllChidren(true))
>    print("modifier_2 children(type):", modifier_2.LogicTree.GetChildrenTypes(true))
>
>  ```

---

#### 我可以使用GDScript来拓展逻辑树节点吗？

> *很遗憾，受限于GDScript无法继承自C#类，故无法使用GDScript来拓展的逻辑树节点([参阅C#拓展逻辑树节点示例代码](#扩展开发))*

## 许可证

[`MIT`](https://mit-license.org/) License