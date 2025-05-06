namespace LogicTree
{
    /// <summary>
    /// 条件节点序列化异常
    /// </summary>
    public class ConditionNodeSerializationException : Exception
    {
        /// <summary>
        /// 初始化 ConditionNodeSerializationException 类的新实例。
        /// </summary>
        /// <param name="message">描述错误的消息。</param>
        /// <param name="innerException">导致当前异常的异常。</param>
        public ConditionNodeSerializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// 初始化 ConditionNodeSerializationException 类的新实例。
        /// </summary>
        /// <param name="message">描述错误的消息。</param>
        public ConditionNodeSerializationException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// 条件节点反序列化异常
    /// </summary>
    public class ConditionNodeDeserializationException : Exception
    {
        /// <summary>
        /// 初始化 ConditionNodeDeserializationException 类的新实例。
        /// </summary>
        /// <param name="message">描述错误的消息。</param>
        /// <param name="innerException">导致当前异常的异常。</param>
        public ConditionNodeDeserializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// 初始化 ConditionNodeDeserializationException 类的新实例。
        /// </summary>
        /// <param name="message">描述错误的消息。</param>
        public ConditionNodeDeserializationException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// 条件评估异常
    /// </summary>
    public class ConditionEvaluationException : Exception
    {
        /// <summary>
        /// 初始化 ConditionEvaluationException 类的新实例。
        /// </summary>
        /// <param name="message">描述错误的消息。</param>
        /// <param name="innerException">导致当前异常的异常。</param>
        public ConditionEvaluationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// 初始化 ConditionEvaluationException 类的新实例。
        /// </summary>
        /// <param name="message">描述错误的消息。</param>
        public ConditionEvaluationException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// 无效反序列化器方法签名异常
    /// </summary>
    public class InvalidDeserializerSignatureException : Exception
    {
        /// <summary>
        /// 初始化 InvalidDeserializerSignatureException 类的新实例。
        /// </summary>
        /// <param name="message">描述错误的消息。</param>
        /// <param name="innerException">导致当前异常的异常。</param>
        public InvalidDeserializerSignatureException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// 初始化 InvalidDeserializerSignatureException 类的新实例。
        /// </summary>
        /// <param name="message">描述错误的消息。</param>
        public InvalidDeserializerSignatureException(string message) : base(message)
        {
        }
    }
}