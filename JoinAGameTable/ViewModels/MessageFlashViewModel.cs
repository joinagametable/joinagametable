namespace JoinAGameTable.ViewModels
{
    public sealed class MessageFlashViewModel
    {
        /// <summary>
        /// Message severity.
        /// </summary>
        public string Severity { get; set; }

        /// <summary>
        /// Message
        /// </summary>
        public string Message { get; set; }

        public override string ToString()
        {
            return GetType().Name + "[Severity=" + Severity + ", Message=" + Message + "]";
        }
    }
}
