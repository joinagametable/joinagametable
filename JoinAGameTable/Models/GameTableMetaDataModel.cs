using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JoinAGameTable.Enumerations;

namespace JoinAGameTable.Models
{
    public class GameTableMetaDataModel
    {
        /// <summary>
        /// Unique Id of the game table meta data.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Linked game table.
        /// </summary>
        public virtual GameTableModel GameTable { get; set; }

        /// <summary>
        /// Meta data key.
        /// </summary>
        [Required]
        public virtual GameTableMetaDataKeyEnum Key { get; set; }

        /// <summary>
        /// Meta data value.
        /// </summary>
        [Required,
         Column(TypeName = "TEXT")]
        public string Value { get; set; }
    }
}
