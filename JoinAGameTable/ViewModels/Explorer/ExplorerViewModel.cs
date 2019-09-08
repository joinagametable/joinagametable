using System;
using System.Collections.Generic;

namespace JoinAGameTable.ViewModels.Explorer
{
    public class ExplorerViewModel
    {
        /// <summary>
        /// Build a new instance.
        /// </summary>
        public ExplorerViewModel()
        {
            Events = new List<Event>();
        }

        /// <summary>
        /// Events.
        /// </summary>
        public List<Event> Events { get; }

        public class Event
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
            /// Event owner public name (DisplayName).
            /// </summary>
            public string Owner { get; set; }

            /// <summary>
            /// Event cover URL.
            /// </summary>
            public string CoverUrl { get; set; }
        }
    }
}
