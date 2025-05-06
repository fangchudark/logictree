using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace LogicTree
{

    /// <summary>
    /// 权重修正器
    /// </summary>
    public class ChanceModifier
    {
        /// <summary>
        /// 机会修正带来的因子修正值
        /// </summary>    
        public float Factor { get; set; } = 1f;

        /// <summary>
        /// 修正条件逻辑树（根节点使用逻辑与节点）
        /// </summary>
        public LogicalAndNode LogicTree { get; set; } = [];

        /// <summary>
        /// 无参构造函数
        /// </summary>
        public ChanceModifier() { }
        /// <summary>
        /// 初始化修正因子
        /// </summary>
        /// <param name="factor">修正乘数</param>
        public ChanceModifier(float factor)
        {
            Factor = factor;
            LogicTree = [];
        }

        /// <summary>
        /// 初始化修正因子和逻辑条件
        /// </summary>
        /// <param name="factor">修正乘数</param>
        /// <param name="logicTree">修正条件逻辑树（根节点使用逻辑与节点）</param>
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

        /// <summary>
        /// 评估条件是否满足
        /// </summary>
        /// <param name="context">运行时上下文数据</param>
        /// <returns>当所有子节点条件满足时返回true</returns>
        public bool Evaluate(Dictionary<string, object> context)
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

            foreach (var child in LogicTree)
            {
                var token = child.ToJson();

                //  遇到相同的键停止平铺
                if (json.ContainsKey(token.Name))
                {
                    hasRepeat = true;
                    break;
                }

                json.Add(token);   // 递归序列化子节点（平铺）
            }

            if (hasRepeat)
            {
                json.RemoveAll(); //  清空json
                json.Add(LogicTree.ToJson()); // 递归序列化子节点（包装）
            }

            json.AddFirst(new JProperty("factor", Factor));
            return json;
        }

        /// <summary>
        /// 序列化为JSON字符串
        /// </summary>
        /// <returns>包含修正因子和条件树的JSON字符串</returns>
        /// <exception cref="ConditionNodeSerializationException">序列化失败时抛出</exception>
        public string ToJsonString()
        {
            try
            {
                return ToJson().ToString();
            }
            catch (Exception e)
            {
                throw new ConditionNodeSerializationException("Failed to serialize the ChanceModifier instance to Json string", e);
            }
        }

        /// <summary>
        /// 从JSON创建修正器实例
        /// </summary>
        /// <param name="obj">包含factor和条件树的JObject</param>
        /// <returns>新创建的ChanceModifier实例</returns>
        public static ChanceModifier FromJson(JObject obj)
        {
            var modifier = new ChanceModifier();
            if (obj.TryGetValue("factor", out var factor))
                modifier.Factor = (float)factor;

            var conditions = new List<ConditionNode>();
            obj.Remove("factor"); // 移除factor属性，只保留条件树
            foreach (var prop in obj.Properties())
            {
                // 使用反序列化工具解析每个条件属性
                var node = ConditionNodeDeserializer.DeserializeNode(prop.Name, prop);
                conditions.Add(node);
            }
            modifier.LogicTree = new(conditions);
            return modifier;
        }

        /// <summary>
        /// 从JSON字符串创建修正器实例
        /// </summary>
        /// <param name="jsonString">包含factor和条件树的Json字符串</param>
        /// <returns>新创建的ChanceModifier实例</returns>
        /// <exception cref="ConditionNodeSerializationException">反序列化过程中发生错误时抛出</exception>
        public static ChanceModifier FromJson(string jsonString)
        {
            try
            {
                return FromJson(JObject.Parse(jsonString));
            }
            catch (Exception e)
            {
                throw new ConditionNodeSerializationException("Failed to deserialize the json string to a ChaneModifier instance", e);
            }
        }
    }
}