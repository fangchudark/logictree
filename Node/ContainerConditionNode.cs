using System.Collections;
using Newtonsoft.Json.Linq;

namespace LogicTree
{
    /// <summary>
    /// 容器条件节点基类（所有包含子条件的节点父类）<br/>
    /// Json结构（当子节点键不重复时）：
    /// <code>
    /// {
    ///     "container_name": {  
    ///         "condition_a": 1,
    ///         "condition_b": true
    ///     }
    /// }
    /// 或（当子节点键重复时）
    /// {
    ///     "container_name": [ 
    ///         { "condition_a": 1 },
    ///         { "condition_a": 2 }
    ///     ]
    /// }
    /// </code>
    /// </summary>
    public abstract class ContainerConditionNode : ConditionNode, IEnumerable, IEnumerable<ConditionNode>, IList<ConditionNode>, ICollection<ConditionNode>
    {
        /// <summary>
        /// 子条件集合
        /// </summary>
        protected List<ConditionNode> Children { get; set; } = [];

        /// <summary>
        /// 索引器，通过索引访问子条件节点
        /// </summary>
        public virtual ConditionNode this[int index] { get => Children[index]; set => Children[index] = value; }

        /// <summary>
        /// 获取子条件的强类型枚举器
        /// </summary>
        /// <returns>子条件的强类型枚举器</returns>
        public virtual IEnumerator<ConditionNode> GetEnumerator() => Children.GetEnumerator();

        /// <summary>
        /// 获取子条件的非泛型枚举器
        /// </summary>
        /// <returns>子条件的非泛型枚举器</returns>
        IEnumerator IEnumerable.GetEnumerator() => Children.GetEnumerator();
        /// <summary>
        /// 获取所有子节点类型名称(调试用)
        /// </summary>
        /// <param name="recurve">是否递归获取子节点的子节点类型名称</param>
        /// <returns>所有子节点类型名称的数组</returns>
        public virtual string[] GetChildrenTypes(bool recurve = false)
        {
            var types = new List<string>();
            foreach (var child in Children)
            {
                types.Add(child.GetType().Name);
                if (recurve && child is ContainerConditionNode con)
                    types.AddRange(con.GetChildrenTypes(recurve));
            }
            return [.. types];
        }
        /// <summary>
        /// 获取所有子节点
        /// </summary>
        /// <param name="recurve">是否递归获取子节节点的子节点</param>
        /// <returns>所有子节点的数组</returns>
        public virtual IEnumerable<ConditionNode> GetAllChildren(bool recurve = false)
        {
            var all = new List<ConditionNode>();
            foreach (var child in Children)
            {
                all.Add(child);
                if (recurve && child is ContainerConditionNode con)
                    all.AddRange(con.GetAllChildren(recurve));
            }
            return [.. all];
        }
        /// <summary>
        /// 获取整个容器中首次出现指定子节点的索引
        /// </summary>
        /// <param name="item">需要查找的子节点</param>
        /// <returns>如果找到，则返回该节点首次出现在容器的从零开始的索引；否则返回 -1。</returns>
        public virtual int IndexOf(ConditionNode item) => Children.IndexOf(item);
        /// <summary>
        /// 在指定索引处插入子节点
        /// </summary>
        /// <param name="index">目标索引</param>
        /// <param name="item">需要插入的子节点</param>
        public virtual void Insert(int index, ConditionNode item) => Children.Insert(index, item);
        /// <summary>
        /// 移除指定索引处的子节点
        /// </summary>
        /// <param name="index">目标索引</param>
        public virtual void RemoveAt(int index) => Children.RemoveAt(index);
        /// <summary>
        /// 向容器中添加子节点
        /// </summary>
        /// <param name="item">需要添加的子节点</param>
        public virtual void Add(ConditionNode item) => Children.Add(item);
        /// <summary>
        /// 清空容器中的所有子节点
        /// </summary>
        public virtual void Clear() => Children.Clear();
        /// <summary>
        /// 检查容器中是否包含指定的子节点
        /// </summary>
        /// <param name="item">需要查找的子节点</param>
        /// <returns>如果容器中包含目标子节点则返回true，否则返回false</returns>
        public virtual bool Contains(ConditionNode item) => Children.Contains(item);        
        /// <summary>
        /// 从容器中移除第一个出现的指定子节点
        /// </summary>
        /// <param name="item">需要移除的子节点</param>
        /// <returns>如果成功移除，返回true，否则返回false，如果指定子节点不存在于容器中也将返回false</returns>
        public virtual bool Remove(ConditionNode item) => Children.Remove(item);
        /// <summary>
        /// 将容器中的子节点复制到指定数组中，从指定索引开始
        /// </summary>
        /// <param name="array">目标数组</param>
        /// <param name="arrayIndex">目标数组指定索引</param>
        public virtual void CopyTo(ConditionNode[] array, int arrayIndex) => Children.CopyTo(array, arrayIndex);
        /// <summary>
        /// 获取容器中包含的子节点数量
        /// </summary>
        public virtual int Count => Children.Count;
        /// <summary>
        /// 获取容器是否为只读（始终返回false）
        /// </summary>
        public virtual bool IsReadOnly => false;
        /// <summary>
        /// 定义JSON序列化的子节点容器键名（默认"container"）
        /// </summary>
        protected virtual string ContainerName => "container";


        /// <summary>
        /// 序列化子节点集合的通用逻辑
        /// </summary>
        /// <remarks>
        /// 自动处理重复键情况：<br/>
        /// 1. 当子节点键名不重复时，生成JObject结构 <br/>
        /// 2. 当检测到重复键时，自动转换为JArray结构
        /// </remarks>
        /// <returns>包装在指定容器名称下的JProperty对象</returns>
        protected virtual JProperty SerializeChildren()
        {
            var obj = new JObject();
            var array = new JArray();
            bool hasRepeat = false;     // 标记是否有重复的键

            foreach (var child in Children)
            {
                // 跳过空节点
                if (child is null)
                    continue;

                var token = child.ToJson();

                // 跳过无效的子节点
                if (token is not JProperty prop)
                    continue;

                // 检测到重复条件键时切换为数组模式
                if (!hasRepeat && obj.ContainsKey(prop.Name))
                {
                    // 进入 array 模式，先把已有的都拆开放进去
                    foreach (var kv in obj)
                    {
                        array.Add(new JObject(new JProperty(kv.Key, kv.Value)));
                    }
                    hasRepeat = true;
                }

                if (hasRepeat)
                {
                    array.Add(new JObject(prop));  // 数组模式追加新条件
                }
                else
                {
                    obj.Add(prop);  // 对象模式直接添加属性
                }
            }

            return new JProperty(ContainerName, hasRepeat ? array : obj);
        }

        /// <summary>
        /// 容器节点反序列化通用方法
        /// </summary>
        /// <typeparam name="T">继承自ContainerConditionNode的具体类型</typeparam>
        /// <param name="value">输入的JSON数据</param>
        /// <param name="expectedName">预期的容器键名（如"and"/"or"/"not"）</param>
        /// <param name="createNode">节点实例创建方法</param>
        /// <returns>实例化的容器节点</returns>
        /// <exception cref="ArgumentException">
        /// 当遇到以下情况时抛出：
        /// 1. 输入值不是有效的JProperty对象
        /// 2. 属性名称与预期容器名称不匹配
        /// </exception>
        protected static T Deserialize<T>(JToken value, string expectedName, Func<T> createNode)
            where T : ContainerConditionNode
        {
            var prop = value as JProperty
                ?? throw new ArgumentException($"Container must be a property.");

            if (prop.Name != expectedName)
                throw new ArgumentException($"Container must have a '{expectedName}' property");

            var node = createNode();
            switch (prop.Value)
            {
                case JObject obj:    // 处理对象格式的多个条件
                    ConditionNodeDeserializer.ParseChildrenFromObject(obj, node.Children);
                    break;
                case JArray array: // 处理数组格式的条件集合
                    foreach (var item in array)
                    {
                        // 验证数组元素必须为JObject类型
                        if (item is not JObject objItem)
                            throw new ArgumentException("Children must be objects.");

                        ConditionNodeDeserializer.ParseChildrenFromObject(objItem, node.Children);
                    }
                    break;

                default: // 无效的类型
                    throw new ArgumentException("Invalid children container type.");
            }

            return node;
        }

    }
}