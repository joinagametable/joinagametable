using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JoinAGameTable.Enumerations;
using JoinAGameTable.ViewModels.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JoinAGameTable.ViewModels.GameTable
{
    public class CreateUpdateGameTableViewModel
    {
        /// <summary>
        /// Build a new instance.
        /// </summary>
        public CreateUpdateGameTableViewModel()
        {
            MetaData = new List<GameTableMetaData>();
            NumberOfSeat = 4;
            DurationEstimationLow = 60;
            DurationEstimationHigh = 240;
        }

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
        [Required(ErrorMessage = "error.required")]
        public string Name { get; set; }

        /// <summary>
        /// Table type.
        /// </summary>
        [Required(ErrorMessage = "error.required"),
         MustBePresentIn("TypeAvailables", ErrorMessage = "error.invalid")]
        public GameTypeEnum Type { get; set; }

        /// <summary>
        /// List of available game types.
        /// </summary>
        public List<SelectListItem> TypeAvailables { get; set; }

        /// <summary>
        /// List of available game types.
        /// </summary>
        public List<SelectListItem> GameTableMetaDataKeyEnumAvailables { get; set; }

        /// <summary>
        /// When the event begins (Date).
        /// </summary>
        [Required(ErrorMessage = "error.required")]
        public string BeginsAtDate { get; set; }

        /// <summary>
        /// When the event begins (Time).
        /// </summary>
        [Required(ErrorMessage = "error.required")]
        public string BeginsAtTime { get; set; }

        /// <summary>
        /// Game duration - Low.
        /// </summary>
        [Required(ErrorMessage = "error.required"),
         Range(2, 1440, ErrorMessage = "error.range")]
        public int DurationEstimationLow { get; set; }

        /// <summary>
        /// NumberOfSeat.
        /// </summary>
        [Required(ErrorMessage = "error.required"),
         Range(1, 42, ErrorMessage = "error.range")]
        public int NumberOfSeat { get; set; }

        /// <summary>
        /// Game duration - High.
        /// </summary>
        [Range(2, 1440, ErrorMessage = "error.range")]
        public int DurationEstimationHigh { get; set; }

        /// <summary>
        /// Get when the event begins with date and time.
        /// </summary>
        public DateTime BeginsAt => DateTime.Parse(BeginsAtDate + " " + BeginsAtTime);

        /// <summary>
        /// Game table metadata.
        /// </summary>
        public List<GameTableMetaData> MetaData { get; }

        public class GameTableMetaData
        {
            /// <summary>
            /// The game table meta data key.
            /// </summary>
            [Required(ErrorMessage = "error.required")]
            public GameTableMetaDataKeyEnum Key { get; set; }

            /// <summary>
            /// The game table meta data key.
            /// </summary>
            [Required(ErrorMessage = "error.required")]
            public string Value { get; set; }
        }
    }
}
