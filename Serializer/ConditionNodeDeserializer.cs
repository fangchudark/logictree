using System.Reflection;
using Newtonsoft.Json.Linq;

namespace LogicTree
{
    /// <summary>
    /// 条件节点反序列化工具类
    /// </summary>
    public static class ConditionNodeDeserializer
    {

        private static readonly Dictionary<string, Func<JToken, ConditionNode>> _deserializers = [];
        private static readonly Dictionary<JTokenType, Func<JProperty, ConditionNode>> _valueTypeDeserializers = [];

        private static bool _isRegistered = false;

        /// <summary>
        /// 反序列化JSON条件节点
        /// </summary>
        /// <param name="key">条件类型标识符</param>
        /// <param name="value">对应的JSON数据</param>
        /// <returns>反序列化后的条件节点对象</returns>
        /// <exception cref="ConditionNodeDeserializationException">遇到不支持的条件类型时抛出</exception>
        public static ConditionNode DeserializeNode(string key, JToken value)
        {
            if (!_isRegistered)
            {
                try
                {
                    RegisterAllDeserializers();
                }
                catch (InvalidDeserializerSignatureException ex)
                {
                    // 包装异常并提供更详细的上下文信息
                    throw new InvalidDeserializerSignatureException("Failed to register deserializers due to an invalid method signature.", ex);
                }
                catch (Exception ex)
                {
                    // 捕获其他可能的异常
                    throw new InvalidOperationException("An unexpected error occurred while registering deserializers.", ex);
                }
            }

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

            throw new ConditionNodeDeserializationException($"Unsupported value type for key '{key}'.");
        }

        /// <summary>
        /// 注册所有反序列化器<br/>
        /// 首次调用 <see cref="DeserializeNode"/>时会自动注册
        /// </summary>
        private static void RegisterAllDeserializers()
        {
            if (_isRegistered) return;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                // 遍历所有类型
                foreach (var type in assembly.GetTypes())
                {
                    // 遍历类型中的所有方法
                    foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        // 获取所有 NodeDeserializerAttribute
                        var attributes = method.GetCustomAttributes<NodeDeserializerAttribute>();
                        foreach (var attribute in attributes)
                        {
                            try
                            {
                                if (!string.IsNullOrEmpty(attribute.Key))
                                {
                                    // 验证方法签名是否符合 Func<JToken, ConditionNode>
                                    ValidateMethodSignature(method, typeof(JToken));

                                    // 创建 Func<JToken, ConditionNode> 委托
                                    var deserializer = (Func<JToken, ConditionNode>)Delegate.CreateDelegate(
                                        typeof(Func<JToken, ConditionNode>), method);

                                    RegisterDeserializer(attribute.Key, deserializer);
                                }
                                else if (attribute.JTokenType != default)
                                {
                                    // 验证方法签名是否符合 Func<JProperty, ConditionNode>
                                    ValidateMethodSignature(method, typeof(JToken));

                                    // 创建 Func<JProperty, ConditionNode> 委托
                                    var deserializer = (Func<JProperty, ConditionNode>)Delegate.CreateDelegate(
                                        typeof(Func<JProperty, ConditionNode>), method);

                                    RegisterValueTypeDeserializer(attribute.JTokenType, deserializer);
                                }
                            }
                            catch (InvalidDeserializerSignatureException ex)
                            {
                                // 处理签名错误的异常
                                throw new InvalidDeserializerSignatureException($"Method '{method.Name}' in type '{type.Name}' has an invalid signature.", ex);
                            }
                        }
                    }
                }
            }

            _isRegistered = true;
        }


        /// <summary>
        /// 验证方法签名是否符合预期
        /// </summary>
        /// <param name="method">要验证的方法</param>
        /// <param name="expectedParameterType">预期的参数类型</param>
        private static void ValidateMethodSignature(MethodInfo method, Type expectedParameterType)
        {
            var parameters = method.GetParameters();

            // 检查参数数量
            if (parameters.Length != 1)
            {
                throw new InvalidDeserializerSignatureException($"Method '{method.Name}' must have exactly one parameter.");
            }

            // 检查参数类型
            if (parameters[0].ParameterType != expectedParameterType)
            {
                throw new InvalidDeserializerSignatureException($"Method '{method.Name}' must have a parameter of type '{expectedParameterType.Name}'.");
            }

            // 检查返回类型
            if (!typeof(ConditionNode).IsAssignableFrom(method.ReturnType))
            {
                throw new InvalidDeserializerSignatureException($"Method '{method.Name}' must return a type that is a subclass of '{nameof(ConditionNode)}'.The method return type is {method.ReturnType.Name}");
            }
        }



        /// <summary>
        /// 注册包含标识符的节点反序列化器（相同的键会被顶替）
        /// </summary>
        /// <param name="key">条件类型标识符</param>
        /// <param name="deserializer">反序列化器</param>
        public static void RegisterDeserializer(string key, Func<JToken, ConditionNode> deserializer)
        {
            if (!_deserializers.ContainsKey(key))
                _deserializers[key] = deserializer;
        }

        /// <summary>
        /// 注册不包含标识符的节点反序列化器（相同的类型会被顶替）
        /// </summary>
        /// <param name="valueType">节点的Json类型</param>
        /// <param name="deserializer">反序列化器</param>
        public static void RegisterValueTypeDeserializer(JTokenType valueType, Func<JProperty, ConditionNode> deserializer)
        {
            if (!_valueTypeDeserializers.ContainsKey(valueType))
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
        public static void ParseChildrenFromObject(JObject obj, List<ConditionNode> children)
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
        /// <see cref="IConditionNodeDeserializer{T}.FromJson(string)"/>的默认实现，包含异常处理<br/>
        /// 尝试将jsonString解析为JObject，取出第一个property，然后转换为字符串，再调用<see cref="IConditionNodeDeserializer{T}.FromJson(string)"/>
        /// </summary>
        /// <typeparam name="T">继承了反序列化接口的节点</typeparam>
        /// <param name="jsonString">json字符串</param>
        /// <returns>新实例化的节点</returns>
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
                throw new ConditionNodeSerializationException("Failed to serialize the node to Json string", e);
            }
        }
    }
}