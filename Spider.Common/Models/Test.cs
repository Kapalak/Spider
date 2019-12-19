namespace Spider.Common.Model
{
    using System.Collections.Generic;

    public partial class Test : ITest
    {
        public string Name { get; set; }

        public string Description { get; set; }

        List<Step> _steps;
        public List<Step> Steps
        {
            get
            {
                if (_steps == null)
                {
                    _steps = new List<Step>();
                }
                return _steps;
            }
        }
        
        //add serialization ignore
        public string FileName { get; set; }
        public string FilePath { get; set; }

        private IMeasure _measure = new Measure();
        public IMeasure Measure {
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