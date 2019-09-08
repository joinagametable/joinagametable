namespace JoinAGameTable.ViewModels
{
    public class ErrorViewModel
    {
        /// <summary>
        /// Id of the current request.
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// Can show request Id?
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
