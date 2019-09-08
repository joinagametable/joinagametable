using System.Globalization;
using System.Threading.Tasks;

namespace JoinAGameTable.Services
{
    /// <summary>
    /// All mail service implementations must extend this interface.
    /// </summary>
    public interface ISendMailService
    {
        /// <summary>
        /// Send a single email.
        /// </summary>
        /// <param name="recipient">The person the mail is addressed to</param>
        /// <param name="viewName">View to use for the mail content</param>
        /// <param name="model">View model to use</param>
        /// <param name="cultureInfo">Culture to use during the content rendering</param>
        /// <returns>A void Task</returns>
        Task SendMailAsync<TModel>(string recipient,
                                   string viewName,
                                   TModel model,
                                   CultureInfo cultureInfo);
    }
}
