using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JoinAGameTable.Enumerations;
using JoinAGameTable.Extensions;
using JoinAGameTable.Models;
using JoinAGameTable.ViewModels.GameTable;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace JoinAGameTable.Controllers
{
    [Authorize]
    public class GameTableController : Controller
    {
        /// <summary>
        /// Handle to the application database context.
        /// </summary>
        private readonly AppDbContext _appDbContext;

        /// <summary>
        /// Handle to the localizer.
        /// </summary>
        private readonly IStringLocalizer<EventController> _localizer;

        /// <summary>
        /// Build a new instance.
        /// </summary>
        /// <param name="appDbContext">Handle to the application database context</param>
        /// <param name="localizer">Handle to the localizer</param>
        public GameTableController(AppDbContext appDbContext,
                                   IStringLocalizer<EventController> localizer)
        {
            _appDbContext = appDbContext;
            _localizer = localizer;
        }

        /// <summary>
        /// Show selected event: Game Tables.
        /// </summary>
        /// <param name="eventId">Event unique Id</param>
        /// <returns>An html document</returns>
        [HttpGet("/event/{eventId:guid}/gametable")]
        public async Task<IActionResult> GET_ShowEventGameTables(Guid eventId)
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

            // Retrieve game tables
            var gameTables = await _appDbContext.GameTable
                .Where(record => record.Event.Id.Equals(eventId))
                .ToListAsync();

            // Create model
            var model = new ShowEventGameTablesViewModel
            {
                EventId = evt.Id,
                EventName = evt.Name,
            };
            foreach (var gameTable in gameTables)
            {
                model.GameTables.Add(new ShowEventGameTablesViewModel.GameTable
                {
                    Id = gameTable.Id,
                    Name = gameTable.Name,
                    Type = _localizer.GetString("enum.GameTypeEnum." + gameTable.GameType),
                    CurrentSeat = 0,
                    NumberOfSeat = gameTable.NumberOfSeat
                });
            }

            // Render
            return View("ShowEventGameTables", model);
        }

        /// <summary>
        /// Show the game table creation page.
        /// </summary>
        /// <param name="eventId">Event unique Id</param>
        /// <returns>An html document</returns>
        [HttpGet("/event/{eventId:guid}/gametable/create")]
        public async Task<IActionResult> GET_CreateNewGameTable(Guid eventId)
        {
            // Retrieve event
            var evt = await _appDbContext.Event
                .Where(record => record.Id.Equals(eventId) && record.Owner.Id.Equals(User.GetUniqueId()))
                .Take(1)
                .FirstAsync();
            if (evt == null)
            {
                return NotFound();
            }

            // Create model
            var model = new CreateUpdateGameTableViewModel()
            {
                EventId = evt.Id,
                EventName = evt.Name,
                BeginsAtDate = evt.BeginsAt.ToString("yyyy-MM-dd"),
                BeginsAtTime = evt.BeginsAt.Hour + ":" + evt.BeginsAt.Minute,
                TypeAvailables = Enum.GetValues(typeof(GameTypeEnum))
                    .Cast<GameTypeEnum>()
                    .Select(value => new SelectListItem
                    {
                        Value = value.ToString(),
                        Text = _localizer.GetString("enum.GameTypeEnum." + value)
                    })
                    .ToList(),
                GameTableMetaDataKeyEnumAvailables = Enum.GetValues(typeof(GameTableMetaDataKeyEnum))
                    .Cast<GameTableMetaDataKeyEnum>()
                    .Select(value => new SelectListItem
                    {
                        Value = value.ToString(),
                        Text = _localizer.GetString("enum.GameTableMetaDataKeyEnum." + value)
                    })
                    .ToList()
            };

            // Render
            return View("CreateNewGameTable", model);
        }

        /// <summary>
        /// Show the game table creation page.
        /// </summary>
        /// <param name="eventId">Event unique Id</param>
        /// <param name="model">Game table information</param>
        /// <returns>An html document</returns>
        [HttpPost("/event/{eventId:guid}/gametable/create"),
         ValidateAntiForgeryToken,
         ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> POST_CreateNewGameTable(Guid eventId, CreateUpdateGameTableViewModel model)
        {
            // Retrieve event
            var evt = await _appDbContext.Event
                .Where(record => record.Id.Equals(eventId) && record.Owner.Id.Equals(User.GetUniqueId()))
                .Take(1)
                .FirstAsync();
            if (evt == null)
            {
                return NotFound();
            }

            // Add extra model data
            PrepopulateModel(model, eventId, evt.Name);

            // Validate model
            ValidateModelWithExtraRules(model, evt.BeginsAt, evt.EndsAt);
            if (!ModelState.IsValid)
            {
                return View("CreateNewGameTable", model);
            }

            // Save new game table on database
            var newGameTable = new GameTableModel
            {
                Id = Guid.NewGuid(),
                Event = evt,
                Name = model.Name,
                GameType = model.Type,
                BeginAt = model.BeginsAt,
                DurationEstimationLow = model.DurationEstimationLow,
                DurationEstimationHigh = model.DurationEstimationHigh,
                NumberOfSeat = model.NumberOfSeat,
                GameClassificationAge = GameClassificationAgeEnum.AGE_THREE,
                GameClassificationContent = GameClassificationContentExt.ToIntegerList(new List<GameClassificationContentEnum>()),
                CreatedAt = DateTimeOffset.UtcNow
            };
            newGameTable.MetaData = model.MetaData.Select(meta => new GameTableMetaDataModel
            {
                Id = Guid.NewGuid(),
                Key = meta.Key,
                Value = meta.Value,
                GameTable = newGameTable
            }).ToList();

            _appDbContext.Attach(evt);
            _appDbContext.Add(newGameTable);
            await _appDbContext.SaveChangesAsync();

            // Success
            HttpContext.MessageFlash("success", _localizer.GetString("gametable.create.flash-success"));

            // Redirect
            return RedirectToAction("GET_ShowEventGameTables", new {eventId = eventId});
        }

        /// <summary>
        /// Show the page to update game table information.
        /// </summary>
        /// <param name="eventId">Event unique Id</param>
        /// <param name="gameTableId">Game table unique Id</param>
        /// <returns>An html document</returns>
        [HttpGet("/event/{eventId:guid}/gametable/{gameTableId:guid}")]
        public async Task<IActionResult> GET_UpdateGameTable(Guid eventId, Guid gameTableId)
        {
            // Retrieve game table
            var gameTable = await _appDbContext.GameTable
                .Where(record => record.Id.Equals(gameTableId)
                    && record.Event.Id.Equals(eventId)
                    && record.Event.Owner.Id.Equals(User.GetUniqueId()))
                .Take(1)
                .Include(record => record.Event)
                .Include(record => record.MetaData)
                .FirstAsync();
            if (gameTable == null)
            {
                return NotFound();
            }

            // Create model
            var model = new CreateUpdateGameTableViewModel
            {
                EventId = gameTable.Event.Id,
                EventName = gameTable.Event.Name,
                Name = gameTable.Name,
                Type = gameTable.GameType,
                NumberOfSeat = gameTable.NumberOfSeat,
                BeginsAtDate = gameTable.BeginAt.ToString("yyyy-MM-dd"),
                BeginsAtTime = gameTable.BeginAt.Hour + ":" + gameTable.BeginAt.Minute,
                DurationEstimationLow = gameTable.DurationEstimationLow,
                DurationEstimationHigh = gameTable.DurationEstimationHigh,
                TypeAvailables = Enum.GetValues(typeof(GameTypeEnum))
                    .Cast<GameTypeEnum>()
                    .Select(value => new SelectListItem
                    {
                        Value = value.ToString(),
                        Text = _localizer.GetString("enum.GameTypeEnum." + value)
                    })
                    .ToList(),
                GameTableMetaDataKeyEnumAvailables = Enum.GetValues(typeof(GameTableMetaDataKeyEnum))
                    .Cast<GameTableMetaDataKeyEnum>()
                    .Select(value => new SelectListItem
                    {
                        Value = value.ToString(),
                        Text = _localizer.GetString("enum.GameTableMetaDataKeyEnum." + value)
                    })
                    .ToList()
            };
            foreach (var gameTableMetaDataModel in gameTable.MetaData ?? new List<GameTableMetaDataModel>())
            {
                model.MetaData.Add(new CreateUpdateGameTableViewModel.GameTableMetaData
                {
                    Key = gameTableMetaDataModel.Key,
                    Value = gameTableMetaDataModel.Value
                });
            }

            return View("UpdateGameTable", model);
        }

        /// <summary>
        /// Update game table information.
        /// </summary>
        /// <param name="eventId">Event unique Id</param>
        /// <param name="gameTableId">Game table unique Id</param>
        /// <param name="model">Game table information</param>
        /// <returns>An html document</returns>
        [HttpPost("/event/{eventId:guid}/gametable/{gameTableId:guid}"),
         ValidateAntiForgeryToken,
         ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> POST_UpdateGameTable(Guid eventId, Guid gameTableId, CreateUpdateGameTableViewModel model)
        {
            // Retrieve game table
            var gameTable = await _appDbContext.GameTable
                .Where(record => record.Id.Equals(gameTableId)
                    && record.Event.Id.Equals(eventId)
                    && record.Event.Owner.Id.Equals(User.GetUniqueId()))
                .Take(1)
                .Include(record => record.Event)
                .Include(record => record.MetaData)
                .FirstAsync();
            if (gameTable == null)
            {
                return NotFound();
            }

            // Add extra model data
            PrepopulateModel(model, eventId, gameTable.Event.Name);

            // Validate model
            ValidateModelWithExtraRules(model, gameTable.Event.BeginsAt, gameTable.Event.EndsAt);
            if (!ModelState.IsValid)
            {
                return View("UpdateGameTable", model);
            }

            // Update game table
            gameTable.Name = model.Name;
            gameTable.GameType = model.Type;
            gameTable.BeginAt = model.BeginsAt;
            gameTable.DurationEstimationLow = model.DurationEstimationLow;
            gameTable.DurationEstimationHigh = model.DurationEstimationHigh;
            gameTable.NumberOfSeat = model.NumberOfSeat;
            gameTable.GameClassificationAge = GameClassificationAgeEnum.AGE_THREE;
            gameTable.GameClassificationContent = GameClassificationContentExt.ToIntegerList(new List<GameClassificationContentEnum>());
            foreach (var metaData in model.MetaData)
            {
                var m = gameTable.MetaData.FirstOrDefault(x => x.Key == metaData.Key);
                if (m != null)
                {
                    m.Value = metaData.Value;
                }
                else
                {
                    gameTable.MetaData.Add(new GameTableMetaDataModel
                    {
                        Id = Guid.NewGuid(),
                        Key = metaData.Key,
                        Value = metaData.Value
                    });
                }
            }

            // Retrieve metadata to delete
            var toRemove = gameTable.MetaData.Where(p => model.MetaData.All(p2 => p2.Key != p.Key)).ToList();

            // Save modification on database
            _appDbContext.RemoveRange(toRemove);
            _appDbContext.Update(gameTable);
            await _appDbContext.SaveChangesAsync();

            // Success
            HttpContext.MessageFlash("success", _localizer.GetString("gametable.update.flash-success"));

            // Redirect
            return RedirectToAction("GET_UpdateGameTable", new {eventId = eventId, gameTableId = gameTableId});
        }

        /// <summary>
        /// Delete game table.
        /// </summary>
        /// <param name="eventId">Event unique Id</param>
        /// <param name="gameTableId">Game table unique Id</param>
        /// <returns>An html document</returns>
        [HttpPost("/event/{eventId:guid}/gametable/{gameTableId:guid}/delete"),
         ValidateAntiForgeryToken,
         ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> POST_DeleteGameTable(Guid eventId, Guid gameTableId)
        {
            // Retrieve game table
            var gameTable = await _appDbContext.GameTable
                .Where(record => record.Id.Equals(gameTableId)
                    && record.Event.Id.Equals(eventId)
                    && record.Event.Owner.Id.Equals(User.GetUniqueId()))
                .Take(1)
                .Include(record => record.MetaData)
                .FirstAsync();
            if (gameTable == null)
            {
                return NotFound();
            }

            // Delete
            _appDbContext.Remove(gameTable);
            await _appDbContext.SaveChangesAsync();

            // Success
            HttpContext.MessageFlash("success", _localizer.GetString("gametable.delete.flash-success", gameTable.Name));

            // Redirect
            return RedirectToAction("GET_ShowEventGameTables", new {eventId = eventId});
        }

        /// <summary>
        /// Prepopulate model with common data.
        /// </summary>
        /// <param name="model">Instance of the model to prepopulate</param>
        /// <param name="eventId">Event unique Id</param>
        /// <param name="eventName">Event name</param>
        private void PrepopulateModel(CreateUpdateGameTableViewModel model, Guid eventId, string eventName)
        {
            model.EventId = eventId;
            model.EventName = eventName;
            model.TypeAvailables = Enum.GetValues(typeof(GameTypeEnum))
                .Cast<GameTypeEnum>()
                .Select(value => new SelectListItem
                {
                    Value = value.ToString(),
                    Text = _localizer.GetString("enum.GameTypeEnum." + value)
                })
                .ToList();
            model.GameTableMetaDataKeyEnumAvailables = Enum.GetValues(typeof(GameTableMetaDataKeyEnum))
                .Cast<GameTableMetaDataKeyEnum>()
                .Select(value => new SelectListItem
                {
                    Value = value.ToString(),
                    Text = _localizer.GetString("enum.GameTableMetaDataKeyEnum." + value)
                })
                .ToList();
        }

        /// <summary>
        /// Perform extra validation on the model.
        /// </summary>
        /// <param name="model">Instance of the model to validate</param>
        /// <param name="eventBeginsAt">When the event begins</param>
        /// <param name="eventEndsAt">When the event ends</param>
        private void ValidateModelWithExtraRules(CreateUpdateGameTableViewModel model,
                                                 DateTimeOffset eventBeginsAt,
                                                 DateTimeOffset eventEndsAt)
        {
            // DurationEstimationLow must be equal or higher than DurationEstimationHigh
            if (model.DurationEstimationHigh < model.DurationEstimationLow)
            {
                ModelState.AddModelError(
                    "DurationEstimationHigh",
                    _localizer.GetString("error.min", "", model.DurationEstimationLow)
                );
            }

            // Game table cannot begin before the beginning of the event and end after it
            if (model.BeginsAt < eventBeginsAt)
            {
                ModelState.AddModelError(
                    "BeginsAtDate",
                    _localizer.GetString("error.min", "", eventBeginsAt)
                );
            }
            else if (model.BeginsAt > eventEndsAt)
            {
                ModelState.AddModelError(
                    "BeginsAtDate",
                    _localizer.GetString("error.max", "", eventEndsAt)
                );
            }
            else if (model.BeginsAt.AddMinutes(model.DurationEstimationHigh) > eventEndsAt)
            {
                ModelState.AddModelError("DurationEstimationHigh", _localizer.GetString("error.invalid"));
            }

            // Meta data "key" must be unique
            var metaDataKeyUsed = new List<GameTableMetaDataKeyEnum>();
            foreach (var metaData in model.MetaData)
            {
                if (metaDataKeyUsed.Contains(metaData.Key))
                {
                    ModelState.AddModelError(
                        "MetaData[" + model.MetaData.IndexOf(metaData) + "].Key",
                        _localizer.GetString("error.already-in-use")
                    );
                }
                else
                {
                    metaDataKeyUsed.Add(metaData.Key);
                }
            }
        }
    }
}
