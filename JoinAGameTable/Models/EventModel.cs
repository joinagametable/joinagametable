using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JoinAGameTable.Models
{
    public class EventModel
    {
        /// <summary>
        /// Unique Id of the event.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Name of the event.
        /// </summary>
        [Required,
         StringLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// Description of the event.
        /// </summary>
        [Required,
         Column(TypeName = "TEXT")]
        public string Description { get; set; }

        /// <summary>
        /// Event banner.
        /// </summary>
        public virtual FileMetaDataModel Banner { get; set; }

        /// <summary>
        /// Event cover.
        /// </summary>
        public virtual FileMetaDataModel Cover { get; set; }

        /// <summary>
        /// Owner of the event.
        /// </summary>
        [Required]
        public virtual UserAccountModel Owner { get; set; }

        /// <summary>
        /// When the event begins.
        /// </summary>
        [Required]
        public DateTimeOffset BeginsAt { get; set; }

        /// <summary>
        /// When the event begins.
        /// </summary>
        [Required]
        public DateTimeOffset EndsAt { get; set; }

        /// <summary>
        /// When the event will be publicly accessible.
        /// </summary>
        public DateTimeOffset? PublicAt { get; set; }

        /// <summary>
        /// Game tables linked to the event.
        /// </summary>
        public virtual IEnumerable<GameTableModel> GameTables { get; set; }

        /// <summary>
        /// When the event has been created.
        /// </summary>
        [Required]
        public DateTimeOffset CreatedAt { get; set; }
    }
}
