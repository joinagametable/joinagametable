namespace JoinAGameTable.Enumerations
{
    public enum GameTableMetaDataKeyEnum
    {
        /// <summary>
        /// Universe or game name.
        /// </summary>
        UNIVERSE = 0,

        /// <summary>
        /// Themes (ie: Horror, Fantasy, ...).
        /// </summary>
        THEMES = 1,

        /// <summary>
        /// Synopsis.
        /// </summary>
        SYNOPSIS = 2,

        /// <summary>
        /// Name or pseudonym of the game facilitator.
        /// </summary>
        GAME_FACILITATOR = 3,

        /// <summary>
        /// Game facilitator can introduce game.
        /// </summary>
        GAME_FACILITATOR_WORD = 4,

        /// <summary>
        /// Name or pseudonym of the game master.
        /// </summary>
        GAME_MASTER = 5,

        /// <summary>
        /// Game master can introduce his scenario.
        /// </summary>
        GAME_MASTER_WORD = 6,

        /// <summary>
        /// Game or scenario style (ie: Occult Investigation).
        /// </summary>
        GAME_STYLE = 7,

        /// <summary>
        /// Used game system (ie: D6, D100, Gumshoe, Storytelling, ...).
        /// </summary>
        GAME_SYSTEM = 8,

        /// <summary>
        /// Useful to indicate extra game rules.
        /// </summary>
        RULES = 9,

        /// <summary>
        /// When game take place (ie: 1920).
        /// </summary>
        TIME_PERIOD = 10,
    }
}
