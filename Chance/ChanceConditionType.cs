using System;
using Godot;

/// <summary>
/// 布尔型条件类型枚举
/// </summary>
public enum ChanceBoolConditionType
{
    //（请将Condition3/Condition4替换为实际业务相关的名称）
    Condition3,
    Condition4
}

/// <summary>
/// 数值型条件类型枚举
/// </summary>
public enum ChanceNumberConditionType
{
    // （请将Condition1/Condition2等替换为实际业务相关的名称）
    Condition1,
    Condition2,
    Condition5,
    Condition6,
}

/// <summary>
/// 条件类型扩展方法
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// 将枚举转换为蛇形命名格式
    /// </summary>
    /// <param name="value"> 枚举值</param>
    /// <returns>小写蛇形命名字符串（示例：condition_1）</returns>
    public static string ToSnakeCase(this Enum value)
    {
        return value.ToString().ToSnakeCase(); // 调用Godot提供的扩展方法转换为蛇形命名
    }

}
