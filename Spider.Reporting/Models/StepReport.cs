namespace Spider.Reporting.Models
{
    using Spider.Common.Model;
    using System;
    using System.IO;

    public class StepReport : Step
    {
        public TestReport TestParent { get; set; }
        public string Status
        {
            get
            {
                return Failed.HasValue && Failed.Value ? "Failed" : (Failed.HasValue && !Failed.Value ? "Success" : "Skipped");
            }
        }

        public string ImageBase64
        {
            get
            {
                if (string.IsNullOrEmpty(ImageFullPath))
                {
                    return null;
                }

                if (File.Exists(ImageFullPath))
                {
                    byte[] imageArray = File.ReadAllBytes(ImageFullPath);
                    string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                    return base64ImageRepresentation;
                }
                return null;
            }
        }
        public string ImageFullPath
        {
            get
            {
                return Name == "TAKE_SCREENSHOT" ? Path.Combine(TestParent.OutputDirectory, Value) : string.Empty;
            }
        }
    }
}
