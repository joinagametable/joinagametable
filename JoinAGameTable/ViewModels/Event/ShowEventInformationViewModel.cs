using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace JoinAGameTable.ViewModels.Event
{
    public class ShowEventInformationViewModel
    {
        /// <summary>
        /// Description. This variable handle a sanitized
        /// version: value has been trimmed and titled.
        /// </summary>
        /// <see cref="Description"/>
        private string _description;

        /// <summary>
        /// Name. This variable handle a sanitized
        /// version: value has been trimmed and titled.
        /// </summary>
        /// <see cref="Name"/>
        private string _name;

        /// <summary>
        /// Name.
        /// </summary>
        /// <see cref="_name"/>
        [Required(ErrorMessage = "error.required"),
         StringLength(50, MinimumLength = 5, ErrorMessage = "error.length")]
        public string Name
        {
            get => _name;
            set => _name = value != null ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.Trim()) : null;
        }

        /// <summary>
        /// Description.
        /// </summary>
        /// <see cref="_name"/>
        [Required(ErrorMessage = "error.required"),
         MinLength(10, ErrorMessage = "error.min-length")]
        public string Description
        {
            get => _description;
            set => _description = value?.Trim();
        }

        /// <summary>
        /// Event unique Id.
        /// </summary>
        public Guid EventId { get; set; }

        /// <summary>
        /// Is event publicly available ?
        /// </summary>
        public bool IsPublicilyAvailable { get; set; }

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
        /// When the event ends (Date).
        /// </summary>
        [Required(ErrorMessage = "error.required")]
        public string EndsAtDate { get; set; }

        /// <summary>
        /// When the event ends (Time).
        /// </summary>
        [Required(ErrorMessage = "error.required")]
        public string EndsAtTime { get; set; }

        /// <summary>
        /// When the event is publicly available (Date).
        /// </summary>
        public string PubliclyAvailableAtDate { get; set; }

        /// <summary>
        /// When the event is publicly available (Time).
        /// </summary>
        public string PubliclyAvailableAtTime { get; set; }

        /// <summary>
        /// Get when the event begins with date and time.
        /// </summary>
        public DateTime BeginsAt => DateTime.Parse(BeginsAtDate + " " + BeginsAtTime);

        /// <summary>
        /// Get when the event ends with date and time.
        /// </summary>
        public DateTime EndsAt => DateTime.Parse(EndsAtDate + " " + EndsAtTime);

        /// <summary>
        /// Get when the event is publicly available with date and time.
        /// </summary>
        public DateTime PubliclyAvailableAt => DateTime.Parse(PubliclyAvailableAtDate + " " + PubliclyAvailableAtTime);

        /// <summary>
        /// Banner Url.
        /// </summary>
        public string BannerUrl { get; set; }

        /// <summary>
        /// Banner.
        /// </summary>
        public IFormFile Banner { get; set; }

        /// <summary>
        /// Cover Url.
        /// </summary>
        public string CoverUrl { get; set; }

        /// <summary>
        /// Cover.
        /// </summary>
        public IFormFile Cover { get; set; }
    }
}
