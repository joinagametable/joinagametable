using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JoinAGameTable.Enumerations;

namespace JoinAGameTable.Models
{
    public class GameTableModel
    {
        /// <summary>
        /// Unique Id of the game table.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Name of the game table.
        /// </summary>
        [Required,
         StringLength(35)]
        public string Name { get; set; }

        /// <summary>
        /// When the registration for the game table begins.
        /// </summary>
        [Required]
        public DateTimeOffset BeginAt { get; set; }

        /// <summary>
        /// Lowest duration estimation in minutes.
        /// </summary>
        [Required]
        public int DurationEstimationLow { get; set; }

        /// <summary>
        /// Highest duration estimation in minutes.
        /// </summary>
        [Required]
        public int DurationEstimationHigh { get; set; }

        /// <summary>
        /// Number of available seats.
        /// </summary>
        [Required]
        public int NumberOfSeat { get; set; }

        /// <summary>
        /// Type of game played on the table.
        /// </summary>
        [Required]
        public GameTypeEnum GameType { get; set; }

        /// <summary>
        /// Age classification for the game played on the table.
        /// </summary>
        [Required]
        public GameClassificationAgeEnum GameClassificationAge { get; set; }

        /// <summary>
        /// Content classification for the game played on the table.
        /// </summary>
        [Required]
        public List<int> GameClassificationContent { get; set; }

        /// <summary>
        /// Game table meta data.
        /// </summary>
        public virtual IEnumerable<GameTableMetaDataModel> MetaData { get; set; }

        /// <summary>
        /// Event linked to the game table.
        /// </summary>
        [Required]
        public virtual EventModel Event { get; set; }

        /// <summary>
        /// When the game table has been created.
        /// </summary>
        [Required]
        public DateTimeOffset CreatedAt { get; set; }
    }
}
