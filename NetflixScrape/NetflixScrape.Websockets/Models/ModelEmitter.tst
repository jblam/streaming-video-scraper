${
    // Enable extension methods by adding using Typewriter.Extensions.*
    using Typewriter.Extensions.Types;
    using System.Text.RegularExpressions;

    // Uncomment the constructor to change template settings.
    Template(Settings settings)
    {
        settings.OutputExtension = ".d.ts";
    }

    // Custom extension methods can be used in the template by adding a $ prefix e.g. $LoudName
    string SafeDocComment(Property property)
    {
        try
        {
            var comment = property.DocComment;
            if (IsDateTime(property.Type))
            {
                return Regex.Replace(comment, @"\b(time)\b", "$1 in seconds");
            }
            return comment;
        }
        catch
        {
            return "";
        }
    }
    bool IsDateTime(Type type) => type.FullName == "System.TimeSpan";
    string TypeConverter(Parameter parameter)
    {
        if (IsDateTime(parameter.Type))
        {
            // serialise times as fractional seconds
            return "number";
        }
        return parameter.Type.Name;
    }
}
declare namespace JBlam.NetflixScrape.Core.Models {
    $Classes(*Model)[
    export interface $Name {
        $Properties(p => p.HasSetter)[
        /**
		 * $SafeDocComment
		 */
        $name: $Type;]
    }]

    $Enums()[
    export enum $Name {
        $Values[$name,
        ]
    }]
}