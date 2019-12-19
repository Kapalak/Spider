namespace Spider.Common.Model
{
    using Spider.Common.Enums;
    using System;

    public class Step
    {
        public string Name { get; set; }

        public StepType Type
        {
            get
            {
                StepType type = (StepType)Enum.Parse(typeof(StepType), Name, true);
                return type;
            }
        }

        public string Description { get; set; }

        public string Param { get; set; }

        public string Value { get; set; }

        public string Page { get; set; }

        public bool TakeScreenshotBefore { get; set; }
        public bool TakeScreenshotAfter { get; set; }

        public Selector Selector { get; set; }

        public string SessionId { get; set; }

        private Measure _measure = new Measure();
        public Measure Measure
        {
            get
            {
                if (_measure == null)
                {
                    _measure = new Measure();
                }
                return _measure;
            }
        }

        public bool Failed { get; set; }
        public string StackTrace { get; set; }
    }
}