using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Spider.Steps.Helpers
{
    public class StepSerializationBinder : ISerializationBinder
    {
        private IEnumerable<Type> _knownTypes = StepsCollectionHelper.GetAllStepsTypes();

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name;
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            var step = _knownTypes
                .FirstOrDefault(t => t.Name.Equals(typeName, StringComparison.InvariantCultureIgnoreCase));
            if (step == default)
                throw new NotSupportedException($"Unknown step {typeName}");
            return step;
        }
    }
}