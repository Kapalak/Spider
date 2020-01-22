namespace Spider.TestbookManager.Helper
{
    using System.Reflection;
    using System.IO;
    using System.Linq;
    using Spider.Common.Helper;
    using Spider.TestbookManager.Models;
    using SiteMapManager.Helpers;
    using System.Collections.Generic;
    using Spider.Common.Enums;
    using Spider.Common.Model;
    using SiteMapManager.Models;
    using NLog;

    public static class TestBookHelper
    {
        private static readonly Logger _log_ = LogManager.GetCurrentClassLogger();
        public static Test ReadTestFromJson(string jsonFile)
        {
            var projectOutputDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var jsonFileAbsolutePath = Path.Combine(
                projectOutputDirectory,
                jsonFile);

            Test test = JsonHelper.DeserializeObject<Test>(jsonFileAbsolutePath);
            test.FilePath = jsonFileAbsolutePath;            
            test.FileName = Path.GetFileName(jsonFileAbsolutePath);

            return test;
        }

        public static Test ConvertFromPageObject(this Test test, ExecutionEnvironment executionEnvironment)
        {
            foreach (var step in test.Steps)
            {
                ((Step)step).ConvertFromPageObject(executionEnvironment);
            }
            return test;
        }

        public static Step ConvertFromPageObject(this Step step, ExecutionEnvironment executionEnvironment)
        {
            foreach (var property in step.GetType().GetProperties())
            {
                if (property.PropertyType == typeof(string))
                {
                    string value = property.GetValue(step, null)?.ToString();

                    if (value != null && value.StartsWith("#") && value.Contains("."))
                    {                        
                        string[] pathElements = value.Substring(1).Split('.');
                        var pageKey = pathElements[0];
                        string propertyPath = value.Substring(1 + 1 + pageKey.Length);
                        var page = SiteMap.Instance.GetPage(pageKey, executionEnvironment);
                        var selectorValue = ExpandoHelper.GetDynamicMember(page, propertyPath);
                        if (selectorValue is string)
                        {
                            step.Param = (string)selectorValue;
                        }
                        if (selectorValue is Selector)
                        {
                            step.Selector = (Selector)selectorValue;
                        }
                        _log_.Trace($"Convert FromPage Object {value} / {step.Param} / {step.Selector}");
                    }
                }
            }
            return step;
        }

        public static void SaveTestToJson(Test test, string filePath = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = test.FilePath;
            }
            var directoryName = Path.GetDirectoryName(filePath);
            _log_.Trace($"Save Test ToJson {filePath}");
            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            JsonHelper.SaveObjectIntoFile(test, filePath);
        }

        public static Test ConvertScenarioToElementarySteps(this Test test, ExecutionEnvironment executionEnvironment)
        {
            //foreach (var step in test.Steps)
            for (int index = 0; index < test.Steps.Count; index++)
            {
                if (test.Steps[index].Type == StepType.EXECUTE_SCENARIO)
                {
                    _log_.Trace($"Convert complex test steps to elementary test steps {test.Steps[index].Name}");
                    var elementarySteps = ((Step)test.Steps[index]).ConvertScenarioToElementarySteps(executionEnvironment);
                    var contextToApply = ContextLoader.Instance.GetContext(test.Steps[index].Value, executionEnvironment.ContextDirectoryLocation);
                    elementarySteps.ForEach(s => s.ApplyScenarioContext(contextToApply));
                    test.Steps.Remove(test.Steps[index]);
                    test.Steps.InsertRange(index, elementarySteps);
                }
            }
            return test;
        }

        public static List<Step> ConvertScenarioToElementarySteps(this Step step, ExecutionEnvironment executionEnvironment)
        {
            var scenario = ScenarioLoader.Instance.GetScenario(step.Param, executionEnvironment.ScenarioDirectoryLocation);
            for (int index = 0; index < scenario.Steps.Count; index++)
            {
                if (scenario.Steps[index].Type == StepType.EXECUTE_SCENARIO)
                {
                    _log_.Trace($"Convert complex test steps to elementary test steps {step.Name}");
                    var elementarySteps = scenario.Steps[index].ConvertScenarioToElementarySteps(executionEnvironment);
                    var contextToApply = ContextLoader.Instance.GetContext(scenario.Steps[index].Value, executionEnvironment.ContextDirectoryLocation);
                    elementarySteps.ForEach(s => s.ApplyScenarioContext(contextToApply));
                    scenario.Steps.Remove(scenario.Steps[index]);
                    scenario.Steps.InsertRange(index, elementarySteps);
                }
            }
            return scenario.Steps;
        }

        public static void ApplyScenarioContext(this Step step, Context context)
        {
            var value = step.Value;
            if (value != null && value.StartsWith("$"))
            {                
                string propertyPath = value.Substring(1);
                var contextValue = (string)ExpandoHelper.GetDynamicMember(context, propertyPath);
                step.Value = contextValue;
                _log_.Trace($"Replacing Scenario Context {step.Name} / {step.Value} / {contextValue}");
            }
            var param = step.Param;
            if (param != null && param.StartsWith("$"))
            {
                string propertyPath = param.Substring(1);
                var contextValue = (string)ExpandoHelper.GetDynamicMember(context, propertyPath);
                step.Param = contextValue;
                _log_.Trace($"Replacing Scenario Context {step.Name} / {step.Param} / {contextValue}");
            }
        }

        public static Test InsertScreenshotSteps(this Test test)
        {
            var steps = new List<IStep>(test.Steps);
            int insertionIndex = 0;
            for (int index = 0; index < steps.Count; index++)
            {
                var currentStep = test.Steps[index];
                if (currentStep.TakeScreenshotBefore && currentStep.Type != StepType.TAKE_SCREENSHOT && currentStep.Type!= StepType.CREATE_SESSION)
                {
                    _log_.Trace($"Insert Screenshot Steps before {currentStep.Name}");
                    test.Steps.Insert(insertionIndex, new Step()
                    {
                        Name = "TAKE_SCREENSHOT",
                        TakeScreenshotBefore = false,
                        TakeScreenshotAfter = false,
                        Description = $"Screenshot before action {currentStep.Name} {currentStep.Param} {currentStep.Value}",
                        Value = $"{test.Name}\\{test.Name}_{index}_0before.png"
                    });
                    insertionIndex++;
                }
                if (currentStep.TakeScreenshotAfter && currentStep.Type != StepType.TAKE_SCREENSHOT)
                {
                    _log_.Trace($"Insert Screenshot Steps after {currentStep.Name}");
                    test.Steps.Insert(insertionIndex + 1, new Step()
                    {
                        Name = "TAKE_SCREENSHOT",
                        TakeScreenshotBefore = false,
                        TakeScreenshotAfter = false,
                        Description = $"Screenshot after action {currentStep.Name} {currentStep.Param}",
                        Value = $"{test.Name}\\{test.Name}_{index}_1after.png"
                    });
                    insertionIndex++;
                }
                insertionIndex++;
            }
            return test;
        }
    }
}
