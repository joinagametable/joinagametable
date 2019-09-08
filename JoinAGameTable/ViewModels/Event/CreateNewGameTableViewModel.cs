using System;
using System.Collections.Generic;
using JoinAGameTable.Enumerations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JoinAGameTable.ViewModels.Event
{
    public class CreateNewGameTableViewModel
    {
        /// <summary>
        /// Unique Id of the current event.
        /// </summary>
        public Guid EventId { get; set; }

        /// <summary>
        /// Name of the current event.
        /// </summary>
        public string EventName { get; set; }

        /// <summary>
        /// Name of the table to create.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Table type.
        /// </summary>
        public GameTypeEnum Type { get; set; }

        /// <summary>
        /// List of available game types.
        /// </summary>
        public List<SelectListItem> TypeAvailables { get; set; }
    }
}
