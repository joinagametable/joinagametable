using System;

namespace JoinAGameTable.ViewModels.Event
{
    public class ShowEventDashboardViewModel
    {
        /// <summary>
        /// Event unique Id.
        /// </summary>
        public Guid EventId { get; set; }

        /// <summary>
        /// Event name.
        /// </summary>
        public string EventName { get; set; }
    }
}
