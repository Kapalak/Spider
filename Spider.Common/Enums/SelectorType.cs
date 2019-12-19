namespace Spider.Common.Enums
{
    using System.ComponentModel;

    public enum SelectorType
    {
        /// <summary>
        /// Xpath
        /// </summary>
        [Description("XPATH")]
        XPATH = 0,

        /// <summary>
        /// Css.
        /// </summary>
        [Description("CSS")]
        CSS = 1,

        /// <summary>
        /// Id.
        /// </summary>
        [Description("ID")]
        ID = 2
    }
}
