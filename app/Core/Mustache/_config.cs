using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mustache
{
    public static class _config
    {

        public static string BlankParameterName = "An attempt was made to define a parameter with a null or an invalid identifier.";
        public static string BlankTagName = "An attempt was made to define a tag with a null or an invalid identifier.";
        public static string DuplicateParameter = "A parameter with the same name already exists within the tag.";
        public static string DuplicateTagDefinition = "The {0} tag has already been registered.";
        public static string KeyNotFound = "The key {0} could not be found.";
        public static string MissingClosingTag = "Expected a matching {0} tag but none was found.";
        public static string UnknownTag = "Encountered an unknown tag: {0}. It was either not registered or exists in a different context.";
        public static string WrongNumberOfArguments = "The wrong number of arguments were passed to an {0} tag.";

    }
}
