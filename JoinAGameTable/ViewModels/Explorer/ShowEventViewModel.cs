using System;

namespace JoinAGameTable.ViewModels.Explorer
{
    public class ShowEventViewModel
    {
        /// <summary>
        /// Event unique Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Event name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Event description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Event banner URL.
        /// </summary>
        public string BannerUrl { get; set; }
    }
}
