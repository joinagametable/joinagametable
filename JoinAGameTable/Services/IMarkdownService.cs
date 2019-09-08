namespace JoinAGameTable.Services
{
    /// <summary>
    /// Provides methods to handle Markdown data.
    /// </summary>
    public interface IMarkdownService
    {
        /// <summary>
        /// Transform given markdown content to html.
        /// </summary>
        /// <param name="content">Markdown content to transform</param>
        /// <returns>An html document</returns>
        string ToHtml(string content);
    }
}
