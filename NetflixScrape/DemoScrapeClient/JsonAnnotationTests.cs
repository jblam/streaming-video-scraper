using JBlam.NetflixScrape.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace JBlam.NetflixScrape.DemoScrapeClient
{
    public class JsonAnnotationTests
    {
        static IEnumerable<Type> GetAllModelsAndCommands()
        {
            var arbitraryModelType = typeof(UiStateModel);
            var typesInNamespace = arbitraryModelType.Assembly.DefinedTypes
                .Where(t => t.Namespace == arbitraryModelType.Namespace);
            return typesInNamespace;
        }
        static IEnumerable<PropertyInfo> GetEnumProperties(Type type)
        {
            return type.GetProperties().Where(p => p.PropertyType.IsEnum);
        }
        public static IEnumerable<object[]> GetAllEnumProperties()
        {
            return GetAllModelsAndCommands().SelectMany(GetEnumProperties).Select(pi => new object[] { pi });
        }

        [Theory, MemberData(nameof(GetAllEnumProperties))]
        public void PropertyInfoUsesStringEnumConverter(PropertyInfo propertyInfo)
        {
            // OMG XUnit, why can you not just PRINT THE BLOODY ARGUMENT in the failure message
            var jsonConverterAttribute = propertyInfo.CustomAttributes.SingleOrDefault(a => a.AttributeType == typeof(JsonConverterAttribute));
            Assert.True(jsonConverterAttribute != null, $"{DebugString(propertyInfo)} doesn't use the required 'StringEnumConverter' attribute");
            var hasStringEnumConverter = jsonConverterAttribute.ConstructorArguments.Any(arg => typeof(StringEnumConverter) == (Type)arg.Value);
            var hasCamelCaseParameter = jsonConverterAttribute.ConstructorArguments.Any(arg => IsSingleBoolean(arg, true));
            Assert.True(hasStringEnumConverter && hasCamelCaseParameter, $"{DebugString(propertyInfo)} doesn't use StringEnumConverter in camelCase mode");

            // holy sweet jeebus what is this:
            // [attr(arg1, true)]
            // the "true" part is declared in Newtonsoft.Json as params object[]
            bool IsSingleBoolean(CustomAttributeTypedArgument argument, bool expected) =>
                // "the argument" is actually a collection of its own type
                argument.Value is IReadOnlyCollection<CustomAttributeTypedArgument> innerArgs
                // with a single value
                && innerArgs.Count == 1
                // which should be a boolean
                && expected.Equals(innerArgs.First().Value);
            string DebugString(PropertyInfo info) => $"{info.DeclaringType.Name}.{info.Name}";
        }
    }
}
