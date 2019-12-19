using Spider.Steps.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Spider.Steps.Helpers
{
    public class StepsCollectionHelper
    {
        public static IEnumerable<Type> GetAllStepsTypes()
        {
            var type = typeof(IStep);
            return type.Assembly.GetTypes()
                .Where(p => type.IsAssignableFrom(p));
        }
    }
}