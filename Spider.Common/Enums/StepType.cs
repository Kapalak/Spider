namespace Spider.Common.Enums
{
    using System.ComponentModel;

    public enum StepType
    {
        /// <summary>
        /// Create Session
        /// </summary>
        [Description("CREATE_SESSION")]
        CREATE_SESSION = 0,

        /// <summary>
        /// Navigate.
        /// </summary>
        [Description("NAVIGATE_URL")]
        NAVIGATE_URL = 1,

        /// <summary>
        /// Click.
        /// </summary>
        [Description("CLICK_BUTTON")]
        CLICK_BUTTON = 2,

        /// <summary>
        /// SET_TEXT.
        /// </summary>
        [Description("SET_TEXT")]
        SET_TEXT = 3,

        /// <summary>
        /// ASSERT TEXT.
        /// </summary>
        [Description("ASSERT_TEXT")]
        ASSERT_TEXT = 4,        

        /// <summary>
        /// Takescreenshot.
        /// </summary>
        [Description("TAKE_SCREENSHOT")]
        TAKE_SCREENSHOT = 12,


        /// <summary>
        /// Takescreenshot.
        /// </summary>
        [Description("RESIZE_WINDOW")]
        RESIZE_WINDOW = 13,

        /// <summary>
        /// EXECUTE_SCENARIO.
        /// </summary>
        [Description("EXECUTE_SCENARIO")]
        EXECUTE_SCENARIO = 14,

        /// <summary>
        /// Takescreenshot.
        /// </summary>
        [Description("EXECUTE_JAVASCRIPT")]
        EXECUTE_JAVASCRIPT = 15,
    }
}
