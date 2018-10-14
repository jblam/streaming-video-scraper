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
    bool RequiresReference(Type type) => type.FullName.StartsWith("JBlam") && !type.IsEnum;
    bool IsModelOrCommand(Class type) => type.Namespace.StartsWith("JBlam.NetflixScrape.Core.Models") && type.FullName.EndsWith("Model") || type.FullName.EndsWith("Command");
    string TypeConverter(Property parameter)
    {
        if (IsDateTime(parameter.Type))
        {
            // serialise times as fractional seconds
            return "number";
        }
        return parameter.Type.Name;
    }
    string Identifier(Class c)
    {
        if (c.Name.EndsWith("Command"))
        {
            return $"command: \"{c.name.Replace("Command", "")}\";";
        }
        if (c.Name.EndsWith("Model"))
        {
            return $"$type: \"{c.name.Replace("Model", "")}\";";
        }
        return "";
    }
}
$Classes(*Model)[$Properties(p => RequiresReference(p.Type))[
/// <reference path="$Type.d.ts" />]
$Properties[$Type[$TypeArguments(arg => RequiresReference(arg))[
/// <reference path="$Name.d.ts" />]]]]
declare namespace JBlam.NetflixScrape.Core.Models {
    $Classes(c => IsModelOrCommand(c))[
    export interface $Name {
        $Identifier
        $Properties[
        /**
		 * $SafeDocComment
		 */
        $name: $TypeConverter;]
    }]

    $Enums()[
    export type $Name = $Values["$name"][|];]
}