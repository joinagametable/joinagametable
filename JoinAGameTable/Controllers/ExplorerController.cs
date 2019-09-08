using System;
using System.Linq;
using System.Threading.Tasks;
using JoinAGameTable.Models;
using JoinAGameTable.Services;
using JoinAGameTable.ViewModels.Explorer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JoinAGameTable.Controllers
{
    public class ExplorerController : Controller
    {
        /// <summary>
        /// Handle to the application database context.
        /// </summary>
        private readonly AppDbContext _appDbContext;

        /// <summary>
        /// Handle to the file storage service.
        /// </summary>
        private readonly IFileStorageService _fileStorageService;

        /// <summary>
        /// Handle to the Markdown service.
        /// </summary>
        private readonly IMarkdownService _markdownService;

        /// <summary>
        /// Build a new instance.
        /// </summary>
        /// <param name="appDbContext">Handle to the application database context</param>
        /// <param name="fileStorageService">Handle to the file storage service</param>
        /// <param name="markdownService">Handle to the Markdown service</param>
        public ExplorerController(AppDbContext appDbContext,
                                  IFileStorageService fileStorageService,
                                  IMarkdownService markdownService)
        {
            _appDbContext = appDbContext;
            _fileStorageService = fileStorageService;
            _markdownService = markdownService;
        }

        /// <summary>
        /// Explore all available events.
        /// </summary>
        /// <returns>An html document</returns>
        [HttpGet("/explorer")]
        public async Task<IActionResult> GET_Explorer()
        {
            var now = DateTimeOffset.UtcNow;
            var model = new ExplorerViewModel();

            // Retrieve publicly available events. Some ended events are included too.
            await _appDbContext.Event
                .Where(record => record.PublicAt <= now && record.EndsAt >= now.AddDays(14))
                .Include(record => record.Cover)
                .Include(record => record.Owner)
                .ToAsyncEnumerable()
                .ForEachAsync(record => model.Events.Add(new ExplorerViewModel.Event
                {
                    Id = record.Id,
                    Name = record.Name,
                    Owner = record.Owner.DisplayName,
                    CoverUrl = record.Cover != null
                        ? _fileStorageService.GenerateFilePublicUrl(
                            record.Cover.Directory + "/" + record.Cover.Id)
                        : null
                }));

            // Render
            return View("ListAllEvents", model);
        }

        /// <summary>
        /// Show the given event information.
        /// </summary>
        /// <param name="eventId">Event unique Id</param>
        /// <returns>An html document</returns>
        [HttpGet("/{eventId:guid}", Order = 999)]
        public async Task<IActionResult> GET_ShowEvent(Guid eventId)
        {
            // Retrieve event
            var evt = await _appDbContext.Event
                .Where(record => record.Id.Equals(eventId))
                .Include(record => record.Banner)
                .Take(1)
                .FirstOrDefaultAsync();

            // Build view model
            var model = new ShowEventViewModel
            {
                Id = evt.Id,
                Name = evt.Name,
                Description = _markdownService.ToHtml(evt.Description),
                BannerUrl = evt.Banner != null
                    ? _fileStorageService.GenerateFilePublicUrl(
                        evt.Banner.Directory + "/" + evt.Banner.Id)
                    : null
            };

            // Render
            return View("ShowEvent", model);
        }
    }
}
