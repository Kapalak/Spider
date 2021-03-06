﻿namespace Spider.Common.Enums
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
        /// Click.
        /// </summary>
        [Description("MOUSE_HOVER")]
        MOUSE_HOVER = 3,

        /// <summary>
        /// SET_TEXT.
        /// </summary>
        [Description("SET_TEXT")]
        SET_TEXT = 4,

        /// <summary>
        /// ASSERT TEXT.
        /// </summary>
        [Description("ASSERT_TEXT")]
        ASSERT_TEXT = 11,

        /// <summary>
        /// ASSERT Element exists.
        /// </summary>
        [Description("ASSERT_EXISTS")]
        ASSERT_EXISTS = 12,

        /// <summary>
        /// Takescreenshot.
        /// </summary>
        [Description("TAKE_SCREENSHOT")]
        TAKE_SCREENSHOT = 21,


        /// <summary>
        /// Takescreenshot.
        /// </summary>
        [Description("RESIZE_WINDOW")]
        RESIZE_WINDOW = 31,

        /// <summary>
        /// EXECUTE_SCENARIO.
        /// </summary>
        [Description("EXECUTE_SCENARIO")]
        EXECUTE_SCENARIO = 51,

        /// <summary>
        /// Takescreenshot.
        /// </summary>
        [Description("EXECUTE_JAVASCRIPT")]
        EXECUTE_JAVASCRIPT = 61,
    }
}
