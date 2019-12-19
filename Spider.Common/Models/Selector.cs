namespace Spider.Common.Model
{
    using Spider.Common.Enums;
    using System;

    public class Selector
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }

        public SelectorType SelectorType
        {
            get
            {
                SelectorType type = (SelectorType)Enum.Parse(typeof(SelectorType), Type, true);
                return type;
            }
        }

        public string Text { get; set; }
        public string Args { get; set; }
    }
}