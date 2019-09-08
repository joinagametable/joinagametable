using Markdig;
using Markdig.Extensions.Bootstrap;
using Markdig.Extensions.GenericAttributes;

namespace JoinAGameTable.Services
{
    public class MarkdownService : IMarkdownService
    {
        /// <summary>
        /// Handle to the Markdown processing pipeline.
        /// </summary>
        private readonly MarkdownPipeline _markdownPipeline;

        /// <summary>
        /// Build a new instance.
        /// </summary>
        public MarkdownService()
        {
            _markdownPipeline = new MarkdownPipelineBuilder().UseGenericAttributes().Build();
            _markdownPipeline.Extensions.AddIfNotAlready(new BootstrapExtension());
            _markdownPipeline.Extensions.AddIfNotAlready(new GenericAttributesExtension());
        }

        public string ToHtml(string content) => Markdown.ToHtml(content, _markdownPipeline);
    }
}
