namespace Spider.Common.Model
{
    public interface ITest
    {
        string Name { get; set; }
        string StackTrace { get; set; }
        bool Failed { get; set; }

        IMeasure Measure { get; }

    }
}
