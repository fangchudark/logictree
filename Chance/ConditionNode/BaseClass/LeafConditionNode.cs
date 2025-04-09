using Godot;

/// <summary>
/// 所有叶子节点的非泛型基类（不要在这里实现具体逻辑！）<br/>
/// 所有叶子节点最终都应继承于此类型
/// </summary>
[GlobalClass]
public abstract partial class LeafConditionNode : ConditionNode 
{
    // 不实现任何内容，只作为类型识别用途
}