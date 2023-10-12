using CommandLine;
using System.Reflection;

namespace ManPageMangler
{
    public static class Mangler
    {
        public static ClassManual[] GetManuals(string assemblyName)
        {
            return Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.Namespace == assemblyName)
                .Where(t => t.GetProperties().Any(p => p.GetCustomAttribute<OptionAttribute>() != null))
                .Select(GetClassManual)
                .ToArray();
        }

        private static ClassManual GetClassManual(Type classType)
        {
            var options = classType
                .GetProperties()
                .Where(p => p.GetCustomAttribute<OptionAttribute>() != null)
                .Select(p => (p, p.GetCustomAttribute<OptionAttribute>()!))
                .Select(p => new OptionManual(p.p.Name, p.Item2.ShortName, p.Item2.LongName, p.Item2.HelpText))
                .ToArray();

            var verb = classType.GetCustomAttribute<VerbAttribute>();
            return new ClassManual(classType.Name, options, verb?.Name, verb?.HelpText);
        }

        private static string[] GetReadMeLines(ClassManual classMan)
        {
            var result = new List<string>
            {
                classMan.VerbName ?? classMan.ClassName,
                "",
                classMan.HelpText ?? "No help text provided.",
                ""
            };
            // todo
            return result.ToArray();
        }

        public record ClassManual(string ClassName, OptionManual[] Options, string? VerbName, string? HelpText);
        public record OptionManual(string PropertyName, string? ShortName, string? LongName, string HelpText);

    }
}
