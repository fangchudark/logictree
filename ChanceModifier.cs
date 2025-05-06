using Newtonsoft.Json.Linq;

namespace LogicTree
{

    /// <summary>
    /// Ȩ��������
    /// </summary>
    public class ChanceModifier
    {

        /// <summary>
        /// ����������������������ֵ
        /// </summary>    
        public float Factor { get; set; } = 1f;

        /// <summary>
        /// ���������߼��������ڵ�ʹ���߼���ڵ㣩
        /// </summary>
        public LogicalAndNode LogicTree { get; set; } = [];

        /// <summary>
        /// �޲ι��캯��
        /// </summary>
        public ChanceModifier()
        {

        }

        /// <summary>
        /// ��ʼ���������Ӻ��߼�����
        /// </summary>
        /// <param name="factor">��������</param>
        /// <param name="logicTree">���������߼��������ڵ�ʹ���߼���ڵ㣩</param>
        public ChanceModifier(float factor, LogicalAndNode logicTree)
        {
            Factor = factor;
            LogicTree = logicTree;
        }

        /// <summary>
        /// ��JSON�����ʼ��������
        /// </summary>
        /// <param name="json">����factor����������JObject</param>
        public ChanceModifier(JObject json)
        {
            FromJson(json);
        }

        /// <summary>
        /// ��JSON�ַ�����ʼ��������
        /// </summary>
        /// <param name="jsonString">������������ʽ��JSON�ַ���</param>
        public ChanceModifier(string jsonString)
        {
            FromJson(JObject.Parse(jsonString));
        }

        /// <summary>
        /// ���������Ƿ�����
        /// </summary>
        /// <param name="context">����ʱ����������</param>
        /// <returns>�������ӽڵ���������ʱ����true</returns>
        public bool Evaluate(System.Collections.Generic.Dictionary<string, object> context)
        {
            return LogicTree.Evaluate(context);     // ί�и��߼���ڵ�����
        }

        /// <summary>
        /// ���л�ΪJSON��ʽ
        /// </summary>
        /// <returns>�����������Ӻ���������JObject</returns>
        public JToken ToJson()
        {
            var json = new JObject();
            bool hasRepeat = false;     // ����Ƿ����ظ��ļ�

            foreach (var child in LogicTree)
            {
                var token = child.ToJson();

                // ������ͬ�ļ�ֹͣƽ��
                if (json.ContainsKey(token.Name))
                {
                    hasRepeat = true;
                    break;
                }

                json.Add(token);  // �ݹ����л��ӽڵ㣨ƽ�̣�
            }

            if (hasRepeat)
            {
                json.RemoveAll(); // ���json
                json.Add(LogicTree.ToJson()); // �ݹ����л��ӽڵ㣨��װ��
            }

            json.AddFirst(new JProperty("factor", Factor));
            return json;
        }

        /// <summary>
        /// ���л�ΪJSON�ַ���
        /// </summary>
        /// <returns>�����������Ӻ���������JSON�ַ���</returns>
        /// <exception cref="ConditionNodeSerializationException">���л�ʧ��ʱ�׳�</exception>
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
        /// ��JSON����������ʵ��
        /// </summary>
        /// <param name="obj">����factor����������JObject</param>
        /// <returns>�´�����ChanceModifierʵ��</returns>
        public static ChanceModifier FromJson(JObject obj)
        {
            // GD.Print("Parsing modifier:"+obj);
            var modifier = new ChanceModifier();
            if (obj.TryGetValue("factor", out var factor))
                modifier.Factor = (float)factor;

            var conditions = new List<ConditionNode>();
            obj.Remove("factor"); // �Ƴ�factor���ԣ�ֻ����������
            foreach (var prop in obj.Properties())
            {
                // ʹ�÷����л����߽���ÿ����������
                var node = ConditionNodeDeserializer.DeserializeNode(prop.Name, prop);
                conditions.Add(node);
            }
            modifier.LogicTree = new(conditions);
            return modifier;
        }

        /// <summary>
        /// ��JSON�ַ�������������ʵ��
        /// </summary>
        /// <param name="jsonString">����factor����������Json�ַ���</param>
        /// <returns>�´�����ChanceModifierʵ��</returns>
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