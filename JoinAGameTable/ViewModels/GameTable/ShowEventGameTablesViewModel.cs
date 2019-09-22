using System;
using System.Collections.Generic;

namespace JoinAGameTable.ViewModels.GameTable
{
    public class ShowEventGameTablesViewModel
    {
        /// <summary>
        /// Build a new instance.
        /// </summary>
        public ShowEventGameTablesViewModel()
        {
            GameTables = new List<GameTable>();
        }

        /// <summary>
        /// Event unique Id.
        /// </summary>
        public Guid EventId { get; set; }

        /// <summary>
        /// Event name.
        /// </summary>
        public string EventName { get; set; }

        /// <summary>
        /// Game tables.
        /// </summary>
        public List<GameTable> GameTables { get; set; }

        public class GameTable
        {
            /// <summary>
            /// Game table unique Id.
            /// </summary>
            public Guid Id { get; set; }

            /// <summary>
            /// Game table name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Game table type.
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// Current number of booked seat.
            /// </summary>
            public int CurrentSeat { get; set; }

            /// <summary>
            /// Number of seat available.
            /// </summary>
            public int NumberOfSeat { get; set; }
        }
    }
}
