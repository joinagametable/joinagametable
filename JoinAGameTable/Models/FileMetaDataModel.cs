using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JoinAGameTable.Models
{
    /// <summary>
    /// Represents a file metadata.
    /// </summary>
    public class FileMetaDataModel
    {
     /// <summary>
        /// Unique Id of the file.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

     /// <summary>
        /// Content type.
        /// </summary>
        [Required,
         StringLength(20)]
        public string ContentType { get; set; }

     /// <summary>
        /// Name of the bucket where the file is stored in.
        /// </summary>
        [Required,
         Column(TypeName = "TEXT")]
        public string Bucket { get; set; }

     /// <summary>
        /// Directory on the bucket where this file is located.
        /// </summary>
        /// [Required,
        [Required,
         Column(TypeName = "TEXT")]
        public string Directory { get; set; }

     /// <summary>
        /// File name.
        /// </summary>
        [Required,
         Column(TypeName = "TEXT")]
        public string FileName { get; set; }
    }
}
