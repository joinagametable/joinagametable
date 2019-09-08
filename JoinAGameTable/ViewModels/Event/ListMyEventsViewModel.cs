using System;
using System.Collections.Generic;

namespace JoinAGameTable.ViewModels.Event
{
    public class ListMyEventsViewModel
    {
        /// <summary>
        /// Build a new instance.
        /// </summary>
        public ListMyEventsViewModel()
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
            /// When the event begins.
            /// </summary>
            public string BeginsAt { get; set; }

            /// <summary>
            /// When the event ends.
            /// </summary>
            public string EndsAt { get; set; }

            /// <summary>
            /// Is event publicly available?
            /// </summary>
            public bool IsPublic { get; set; }
        }
    }
}
