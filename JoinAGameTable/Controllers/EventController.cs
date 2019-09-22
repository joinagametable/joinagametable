using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JoinAGameTable.Extensions;
using JoinAGameTable.Models;
using JoinAGameTable.Services;
using JoinAGameTable.ViewModels.Event;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using SixLabors.ImageSharp;
using SixLabors.Primitives;

namespace JoinAGameTable.Controllers
{
    [Authorize]
    public class EventController : Controller
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
        /// Handle to image manipulation service.
        /// </summary>
        private readonly IImageManipulationService _imageManipulationService;

        /// <summary>
        /// Handle to the localizer.
        /// </summary>
        private readonly IStringLocalizer<EventController> _localizer;

        /// <summary>
        /// Build a new instance.
        /// </summary>
        /// <param name="appDbContext">Handle to the application database context</param>
        /// <param name="fileStorageService">Handle to the file storage service</param>
        /// <param name="localizer">Handle to the localizer</param>
        public EventController(AppDbContext appDbContext,
                               IImageManipulationService imageManipulationService,
                               IFileStorageService fileStorageService,
                               IStringLocalizer<EventController> localizer)
        {
            _appDbContext = appDbContext;
            _fileStorageService = fileStorageService;
            _imageManipulationService = imageManipulationService;
            _localizer = localizer;
        }

        /// <summary>
        /// Show the event creation page.
        /// </summary>
        /// <returns>An html document</returns>
        [HttpGet("/event/create")]
        public IActionResult GET_CreateNewEvent()
        {
            return View("CreateNewEvent");
        }

        /// <summary>
        /// Create new event.
        /// </summary>
        /// <param name="model">Bind authentication view model</param>
        /// <returns>A redirection</returns>
        [HttpPost("/event/create"),
         ValidateAntiForgeryToken,
         ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> POST_CreateNewEvent(CreateNewEventViewModel model)
        {
            // Extra check : BeginAt must be lower than EndAt and after now
            if (model.BeginsAt >= DateTime.Now)
            {
                if (model.BeginsAt >= model.EndsAt)
                {
                    ModelState.AddModelError(
                        "EndsAtDate",
                        _localizer.GetString("error.date-must-be-after", model.BeginsAt)
                    );
                }
            }
            else
            {
                ModelState.AddModelError("BeginsAtDate", _localizer["error.date-must-be-in-future"]);
            }

            // Check if submitted model is valid
            if (!ModelState.IsValid)
            {
                return View("CreateNewEvent", model);
            }

            // Create event
            var eventToCreate = new EventModel
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                Description = model.Description,
                Owner = new UserAccountModel
                {
                    Id = User.GetUniqueId()
                },
                BeginsAt = model.BeginsAt,
                EndsAt = model.EndsAt,
                CreatedAt = DateTime.UtcNow
            };

            // Save on database
            _appDbContext.Attach(eventToCreate.Owner);
            _appDbContext.Add(eventToCreate);
            await _appDbContext.SaveChangesAsync();

            // Success
            HttpContext.MessageFlash("success", _localizer.GetString("event.create.flash-success"));

            // Redirect to newly created event management page
            return RedirectToAction("GET_ShowEventDashboard", new {eventId = eventToCreate.Id});
        }

        /// <summary>
        /// List my created events.
        /// </summary>
        /// <returns>An html document</returns>
        [HttpGet("/event")]
        public async Task<IActionResult> GET_ListMyEvents()
        {
            // Create model
            var model = new ListMyEventsViewModel();

            // Retrieve all events owned by current user
            await _appDbContext.Event
                .AsNoTracking()
                .Where(record => record.Owner.Id.Equals(User.GetUniqueId()))
                .OrderByDescending(record => record.CreatedAt)
                .ForEachAsync(record => model.Events.Add(new ListMyEventsViewModel.Event
                {
                    Id = record.Id,
                    Name = record.Name,
                    BeginsAt = record.BeginsAt.DateTime.ToShortDateString() + " " + record.BeginsAt.DateTime.ToShortTimeString(),
                    EndsAt = record.EndsAt.DateTime.ToShortDateString() + " " + record.EndsAt.DateTime.ToShortTimeString(),
                    IsPublic = record.PublicAt != null && record.PublicAt.Value.CompareTo(DateTime.UtcNow) > 0
                }));

            // Render
            return View("ListMyEvents", model);
        }

        /// <summary>
        /// Show selected event: Insights.
        /// </summary>
        /// <param name="eventId">Event unique Id</param>
        /// <returns>An html document</returns>
        [HttpGet("/event/{eventId:guid}")]
        public async Task<IActionResult> GET_ShowEventDashboard(Guid eventId)
        {
            // Retrieve event
            var evt = await _appDbContext.Event
                .AsNoTracking()
                .Select(record => new
                {
                    record.Id,
                    record.Name,
                    record.Owner
                })
                .Where(record => record.Id.Equals(eventId) && record.Owner.Id.Equals(User.GetUniqueId()))
                .Take(1)
                .FirstAsync();
            if (evt == null)
            {
                return NotFound();
            }

            // Create model
            var model = new ShowEventDashboardViewModel
            {
                EventId = evt.Id,
                EventName = evt.Name,
            };

            // Render
            return View("ShowEventDashboard", model);
        }

        /// <summary>
        /// Show selected event: Information.
        /// </summary>
        /// <param name="eventId">Event unique Id</param>
        /// <returns>An html document</returns>
        [HttpGet("/event/{eventId:guid}/information")]
        public async Task<IActionResult> GET_ShowEventInformation(Guid eventId)
        {
            // Retrieve event
            var evt = await _appDbContext.Event
                .Where(record => record.Id.Equals(eventId) && record.Owner.Id.Equals(User.GetUniqueId()))
                .Include(record => record.Banner)
                .Include(record => record.Cover)
                .Take(1)
                .FirstAsync();
            if (evt == null)
            {
                return NotFound();
            }

            // Create model
            var model = new ShowEventInformationViewModel
            {
                EventId = evt.Id,
                Name = evt.Name,
                Description = evt.Description,
                IsPublicilyAvailable = evt.PublicAt != null,
                BeginsAtDate = evt.BeginsAt.ToString("yyyy-MM-dd"),
                BeginsAtTime = evt.BeginsAt.Hour + ":" + evt.BeginsAt.Minute,
                EndsAtDate = evt.EndsAt.ToString("yyyy-MM-dd"),
                EndsAtTime = evt.EndsAt.Hour + ":" + evt.EndsAt.Minute,
                PubliclyAvailableAtDate = evt.PublicAt?.ToString("yyyy-MM-dd"),
                PubliclyAvailableAtTime = evt.PublicAt == null ? "00:00" : evt.PublicAt?.Hour + ":" + evt.PublicAt?.Minute,
                BannerUrl = evt.Banner != null
                    ? _fileStorageService.GenerateFilePublicUrl(
                        evt.Banner.Directory + "/" + evt.Banner.Id)
                    : null,
                CoverUrl = evt.Cover != null
                    ? _fileStorageService.GenerateFilePublicUrl(
                        evt.Cover.Directory + "/" + evt.Cover.Id)
                    : null
            };

            // Render
            return View("ShowEventInformation", model);
        }

        /// <summary>
        /// Update information of the current selected event.
        /// </summary>
        /// <param name="eventId">Event unique Id</param>
        /// <returns>An html document</returns>
        [HttpPost("/event/{eventId:guid}/information")]
        public async Task<IActionResult> POST_UpdateEventInformation(Guid eventId, ShowEventInformationViewModel model)
        {
            // Retrieve event
            var evt = await _appDbContext.Event
                .Where(record => record.Id.Equals(eventId) && record.Owner.Id.Equals(User.GetUniqueId()))
                .Include(record => record.Banner)
                .Include(record => record.Cover)
                .Take(1)
                .FirstAsync();
            if (evt == null)
            {
                return NotFound();
            }

            // Extra check : BeginAt must be lower than EndAt and after now
            if (model.BeginsAt != evt.BeginsAt)
            {
                if (model.BeginsAt < DateTime.Now)
                {
                    ModelState.AddModelError("BeginsAtDate", _localizer["error.date-must-be-in-future"]);
                }
            }

            if (model.EndsAt != evt.EndsAt)
            {
                if (model.BeginsAt >= model.EndsAt)
                {
                    ModelState.AddModelError(
                        "EndsAtDate",
                        _localizer.GetString("error.date-must-be-after", model.BeginsAt)
                    );
                }
            }

            if (model.IsPublicilyAvailable && model.PubliclyAvailableAt >= model.BeginsAt)
            {
                ModelState.AddModelError(
                    "PubliclyAvailableAtDate",
                    _localizer.GetString("error.date-must-be-before", model.BeginsAt)
                );
            }

            // Check if submitted model is valid
            if (!ModelState.IsValid)
            {
                model.EventId = eventId;
                model.BannerUrl = evt.Banner != null
                    ? _fileStorageService.GenerateFilePublicUrl(
                        evt.Banner.Directory + "/" + evt.Banner.Id)
                    : null;
                model.CoverUrl = evt.Cover != null
                    ? _fileStorageService.GenerateFilePublicUrl(
                        evt.Cover.Directory + "/" + evt.Cover.Id)
                    : null;
                return View("ShowEventInformation", model);
            }

            // Updates event
            evt.Name = model.Name;
            evt.Description = model.Description;
            evt.BeginsAt = model.BeginsAt;
            evt.EndsAt = model.EndsAt;
            if (model.IsPublicilyAvailable)
            {
                evt.PublicAt = model.PubliclyAvailableAt;
            }
            else
            {
                evt.PublicAt = null;
            }

            // Update event cover
            if (model.Cover != null)
            {
                // Resize uploaded cover
                using (var resizedCover = new MemoryStream())
                {
                    using (var uploadedAvatar = model.Cover.OpenReadStream())
                    {
                        var image = Image.Load(uploadedAvatar);
                        await _imageManipulationService.ResizeImageAsync(image, new Size(300, 182));
                        await _imageManipulationService.SaveAsPngAsync(image, resizedCover);
                    }

                    // Remove old cover if needed
                    if (evt.Cover != null)
                    {
                        await _fileStorageService.DeleteFileAsync(
                            evt.Cover.Directory,
                            evt.Cover.Id.ToString()
                        );

                        _appDbContext.Remove(evt.Cover);
                        evt.Cover = null;
                    }

                    // Upload new cover
                    var uid = Guid.NewGuid();
                    var fileMetaData = new FileMetaDataModel()
                    {
                        Id = uid,
                        Bucket = _fileStorageService.BucketName,
                        ContentType = model.Cover.ContentType,
                        Directory = "event",
                        FileName = uid.ToString()
                    };
                    await _fileStorageService.StoreFileAsync(
                        fileMetaData.Directory + "/" + fileMetaData.Id,
                        "image/png",
                        resizedCover.Length,
                        resizedCover
                    );

                    evt.Cover = fileMetaData;
                }
            }

            // Update event banner
            if (model.Banner != null)
            {
                // Resize banner picture
                using (var resizedBanner = new MemoryStream())
                {
                    using (var uploadedAvatar = model.Banner.OpenReadStream())
                    {
                        var image = Image.Load(uploadedAvatar);
                        await _imageManipulationService.ResizeImageAsync(image, new Size(2000, 800));
                        await _imageManipulationService.SaveAsJpegAsync(image, resizedBanner);
                    }

                    // Remove old banner if needed
                    if (evt.Banner != null)
                    {
                        await _fileStorageService.DeleteFileAsync(
                            evt.Banner.Directory,
                            evt.Banner.Id.ToString()
                        );

                        _appDbContext.Remove(evt.Banner);
                        evt.Banner = null;
                    }

                    // Upload new banner
                    var uid = Guid.NewGuid();
                    var fileMetaData = new FileMetaDataModel()
                    {
                        Id = uid,
                        Bucket = _fileStorageService.BucketName,
                        ContentType = model.Banner.ContentType,
                        Directory = "event",
                        FileName = uid.ToString()
                    };
                    await _fileStorageService.StoreFileAsync(
                        fileMetaData.Directory + "/" + fileMetaData.Id,
                        "image/jpeg",
                        resizedBanner.Length,
                        resizedBanner
                    );

                    evt.Banner = fileMetaData;
                }
            }

            // Saves information in databases
            _appDbContext.Update(evt);
            await _appDbContext.SaveChangesAsync();

            // Success
            HttpContext.MessageFlash("success", _localizer.GetString("event.update.flash-success"));

            // Render
            return RedirectToAction("GET_ShowEventInformation", new {eventId = eventId});
        }

        /// <summary>
        /// Show selected event: FAQ.
        /// </summary>
        /// <param name="eventId">Event unique Id</param>
        /// <returns>An html document</returns>
        [HttpGet("/event/{eventId:guid}/faq")]
        public async Task<IActionResult> GET_ShowEventFAQ(Guid eventId)
        {
            // Retrieve event
            var evt = await _appDbContext.Event
                .Select(record => new
                {
                    record.Id,
                    record.Name,
                    record.Owner
                })
                .Where(record => record.Id.Equals(eventId) && record.Owner.Id.Equals(User.GetUniqueId()))
                .Take(1)
                .FirstAsync();
            if (evt == null)
            {
                return NotFound();
            }

            // Create model
            var model = new ShowEventFAQViewModel
            {
                EventId = evt.Id,
                EventName = evt.Name,
            };

            // Render
            return View("ShowEventFAQ", model);
        }
    }
}
