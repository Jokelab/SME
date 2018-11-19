using SME.Factory;
using SME.Shared;
using System.IO;
using System.Linq;

namespace SME.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            var policy = CreatePolicy();
            var fullPath = GetInputFile(args);
            var content = File.ReadAllText(fullPath);

            //create factory based on the file extension
            var factory = FactoryProducer.GetFactory(Path.GetExtension(fullPath));

            //construct transformer and apply transformations
            var transformer = factory.CreateTransformer();
            var transformations = transformer.Transform(content, policy);

            //persist transformed code
            transformations.SaveTransformations(fullPath);

            //construct scheduler and schedule the code for execution
            var scheduler = factory.CreateScheduler();
            scheduler.Schedule(transformations.CodeTransformations, args);

            System.Console.ReadKey();
        }

        /// <summary>
        /// Determine the source file to read the contents from
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static string GetInputFile(string[] args)
        {
            var currentPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var fullPath = Path.Combine(currentPath, "Sample.php");
            if (args.Length > 0)
            {
                var prefix = "-input:";
                var inputArg = args.FirstOrDefault(arg => arg.StartsWith(prefix));
                if (!string.IsNullOrEmpty(inputArg))
                {
                    fullPath = inputArg.Substring(prefix.Length);
                }
            }
            return fullPath;
        }

        public static IPolicy CreatePolicy()
        {
            var policy = new Policy();

            policy.Levels.Add(new SecurityLevel() { Name = "L", Level = 1 });
            policy.Levels.Add(new SecurityLevel() { Name = "H", Level = 2 });

            //input channels
            policy.Input.Add(new ChannelLabel() { Name = "_GET", Level = 1 });
            policy.Input.Add(new ChannelLabel() { Name = "_POST", Level = 1 });
            policy.Input.Add(new ChannelLabel() { Name = "_COOKIE", Level = 1 });

            //output channels
            policy.Output.Add(new ChannelLabel() { Name = "mysql_query", Level = 2 });

            //sanitize channels
            policy.Sanitize.Add(new ChannelLabel() { Name = "escape_input", Level = 1, TargetLevel = 2 });

            return policy;
        }
    }
}
