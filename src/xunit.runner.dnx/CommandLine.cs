using System;
using System.Collections.Generic;
using System.IO;

namespace Xunit.Runner.Dnx
{
    public class CommandLine
    {
        readonly Stack<string> arguments = new Stack<string>();

        protected CommandLine(string[] args)
        {
            for (var i = args.Length - 1; i >= 0; i--)
                arguments.Push(args[i]);

            TeamCity = Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null;
            DesignTimeTestUniqueNames = new List<string>();
            Project = Parse();
        }

        public bool DiagnosticMessages { get; set; }

        public bool Debug { get; set; }

        public bool DesignTime { get; set; }

        // Used with --designtime - to specify specific tests by uniqueId.
        public List<string> DesignTimeTestUniqueNames { get; private set; }

        public bool List { get; set; }

        public int? MaxParallelThreads { get; set; }

        public bool NoLogo { get; set; }

        public XunitProject Project { get; protected set; }

        public bool? ParallelizeAssemblies { get; set; }

        public bool? ParallelizeTestCollections { get; set; }

        public bool Quiet { get; set; }

        public bool TeamCity { get; protected set; }

        public bool Wait { get; protected set; }

        static XunitProject GetProjectFile(List<Tuple<string, string>> assemblies)
        {
            var result = new XunitProject();

            foreach (var assembly in assemblies)
                result.Add(new XunitProjectAssembly
                {
                    AssemblyFilename = Path.GetFullPath(assembly.Item1),
                    ConfigFilename = assembly.Item2 != null ? Path.GetFullPath(assembly.Item2) : null,
                    ShadowCopy = true
                });

            return result;
        }

        static void GuardNoOptionValue(KeyValuePair<string, string> option)
        {
            if (option.Value != null)
                throw new ArgumentException(String.Format("error: unknown command line option: {0}", option.Value));
        }

        public static CommandLine Parse(params string[] args)
        {
            return new CommandLine(args);
        }

        protected XunitProject Parse()
        {
            var assemblies = new List<Tuple<string, string>>();

            while (arguments.Count > 0)
            {
                if (arguments.Peek().StartsWith("-"))
                    break;

                var assemblyFile = arguments.Pop();
                string configFile = null;

                assemblies.Add(Tuple.Create(assemblyFile, configFile));
            }

            if (assemblies.Count == 0)
                throw new ArgumentException("must specify at least one assembly");

            var project = GetProjectFile(assemblies);

            while (arguments.Count > 0)
            {
                var option = PopOption(arguments);
                var optionName = option.Key.ToLowerInvariant();

                if (!optionName.StartsWith("-"))
                    throw new ArgumentException(String.Format("unknown command line option: {0}", option.Key));

                if (optionName == "-quiet")
                {
                    GuardNoOptionValue(option);
                    Quiet = true;
                }
                else if (optionName == "-nologo")
                {
                    GuardNoOptionValue(option);
                    NoLogo = true;
                }
                else if (optionName == "-debug")
                {
                    GuardNoOptionValue(option);
                    Debug = true;
                }
                else if (optionName == "-wait")
                {
                    GuardNoOptionValue(option);
                    Wait = true;
                }
                else if (optionName == "-diagnostics")
                {
                    GuardNoOptionValue(option);
                    DiagnosticMessages = true;
                }
                else if (optionName == "-maxthreads")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -maxthreads");

                    switch (option.Value)
                    {
                        case "default":
                            MaxParallelThreads = null;
                            break;

                        case "unlimited":
                            MaxParallelThreads = 0;
                            break;

                        default:
                            int threadValue;
                            if (!Int32.TryParse(option.Value, out threadValue) || threadValue < 0)
                                throw new ArgumentException("incorrect argument value for -maxthreads (must be 'default', 'unlimited', or a positive number)");

                            MaxParallelThreads = threadValue;
                            break;
                    }
                }
                else if (optionName == "-parallel")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -parallel");

                    ParallelismOption parallelismOption;
                    if (!Enum.TryParse<ParallelismOption>(option.Value, out parallelismOption))
                        throw new ArgumentException("incorrect argument value for -parallel");

                    switch (parallelismOption)
                    {
                        case ParallelismOption.all:
                            ParallelizeAssemblies = true;
                            ParallelizeTestCollections = true;
                            break;

                        case ParallelismOption.assemblies:
                            ParallelizeAssemblies = true;
                            ParallelizeTestCollections = false;
                            break;

                        case ParallelismOption.collections:
                            ParallelizeAssemblies = false;
                            ParallelizeTestCollections = true;
                            break;

                        case ParallelismOption.none:
                        default:
                            ParallelizeAssemblies = false;
                            ParallelizeTestCollections = false;
                            break;
                    }
                }
                else if (optionName == "-teamcity")
                {
                    GuardNoOptionValue(option);
                    TeamCity = true;
                }
                else if (optionName == "-noshadow")
                {
                    GuardNoOptionValue(option);
                    foreach (var assembly in project.Assemblies)
                        assembly.ShadowCopy = false;
                }
                else if (optionName == "-trait")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -trait");

                    var pieces = option.Value.Split('=');
                    if (pieces.Length != 2 || String.IsNullOrEmpty(pieces[0]) || String.IsNullOrEmpty(pieces[1]))
                        throw new ArgumentException("incorrect argument format for -trait (should be \"name=value\")");

                    var name = pieces[0];
                    var value = pieces[1];
                    project.Filters.IncludedTraits.Add(name, value);
                }
                else if (optionName == "-notrait")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -notrait");

                    var pieces = option.Value.Split('=');
                    if (pieces.Length != 2 || String.IsNullOrEmpty(pieces[0]) || String.IsNullOrEmpty(pieces[1]))
                        throw new ArgumentException("incorrect argument format for -notrait (should be \"name=value\")");

                    var name = pieces[0];
                    var value = pieces[1];
                    project.Filters.ExcludedTraits.Add(name, value);
                }
                else if (optionName == "-class")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -class");

                    project.Filters.IncludedClasses.Add(option.Value);
                }
                else if (optionName == "-method")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -method");

                    project.Filters.IncludedMethods.Add(option.Value);
                }
                else if (optionName == "-test" || optionName == "--test")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for --test");

                    DesignTimeTestUniqueNames.Add(option.Value);
                }
                else if (optionName == "-list" || optionName == "--list")
                {
                    GuardNoOptionValue(option);
                    List = true;
                }
                else if (optionName == "-designtime" || optionName == "--designtime")
                {
                    GuardNoOptionValue(option);
                    DesignTime = true;
                }
                else
                {
                    if (option.Value == null)
                        throw new ArgumentException(String.Format("missing filename for {0}", option.Key));

                    project.Output.Add(optionName.Substring(1), option.Value);
                }
            }

            return project;
        }

        static KeyValuePair<string, string> PopOption(Stack<string> arguments)
        {
            var option = arguments.Pop();
            string value = null;

            if (arguments.Count > 0 && !arguments.Peek().StartsWith("-"))
                value = arguments.Pop();

            return new KeyValuePair<string, string>(option, value);
        }
    }
}
