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
            var fullPath = GetFileArgument(args, "-input:", "Sample.php");
            var content = File.ReadAllText(fullPath);
            var policy = GetPolicy(args);

            //create factory based on the file extension
            var factory = FactoryProducer.GetFactory(Path.GetExtension(fullPath));

            //construct transformer and apply transformations
            var transformer = factory.CreateTransformer();
            var transformations = transformer.Transform(content, policy);
            if (!TransformationsOk(transformations)) return;

            //persist transformed code
            transformations.SaveTransformations(fullPath);

            //construct scheduler and schedule the code for execution
            var scheduler = factory.CreateScheduler();
            scheduler.Schedule(transformations.CodeTransformations, args);

            //determine verdict after execution
            var verdict = scheduler.GetVerdict(transformations.OutputChannels);
            ShowVerdict(verdict, policy, content);

            System.Console.ReadKey();
        }

        /// <summary>
        /// Get path argument from program arguments.
        /// If an absolute path is provided for the argument, then this will be returned. 
        /// Else the relative path is assumed to point to the current directory.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static string GetFileArgument(string[] args, string prefix, string defaultFile)
        {
            var currentPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var fullPath = Path.Combine(currentPath, defaultFile);
            if (args.Length > 0)
            {
                var inputArg = args.FirstOrDefault(arg => arg.StartsWith(prefix));
                if (!string.IsNullOrEmpty(inputArg))
                {
                    fullPath = inputArg.Substring(prefix.Length);
                    if (!Path.IsPathRooted(fullPath))
                    {
                        //relative path provided, so append it to the current directory
                        fullPath = Path.Combine(currentPath, fullPath);
                    }
                }
            }
            return fullPath;
        }

        private static IPolicy GetPolicy(string[] args)
        {
            var policyPath = GetFileArgument(args, "-policy:", "policy.xml");
            if (File.Exists(policyPath))
            {
                return PolicyReader.ReadXml<Policy>(policyPath);
            }
            //if path doesn't exist, create a default policy object in code
            return CreateDefaultPolicy();
        }

        private static bool TransformationsOk(TransformationResult transformations)
        {
            if (transformations.InputChannels.Count == 0)
            {
                System.Console.WriteLine("No input channels found that match the active policy.");
                return false;
            }

            if (transformations.OutputChannels.Count == 0)
            {
                System.Console.WriteLine("No output channels found that match the active policy.");
                return false;
            }

            if (transformations.CodeTransformations.Count == 0)
            {
                System.Console.WriteLine("No code transformations available.");
                return false;
            }
            return true;
        }


        private static IPolicy CreateDefaultPolicy()
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
            policy.Sanitize.Add(new ChannelLabel() { Name = "mysql_real_escape_string", Level = 1, TargetLevel = 2 });

            return policy;
        }

        /// <summary>
        /// Show verdict about the input code
        /// </summary>
        /// <param name="verdict"></param>
        private static void ShowVerdict(Verdict verdict, IPolicy policy, string document)
        {
            System.Console.WriteLine("===== VERDICT =====");
            if (verdict.InterferentChannels.Any())
            {
                System.Console.WriteLine($"Observed {verdict.InterferentChannels.Count} channel(s) with different output values between the SME exection and the original execution.\nThe code is thus interferent.");
                foreach (var chan in verdict.InterferentChannels)
                {
                    var levelName = policy.Levels.Where(l => l.Level == chan.Label.Level).Select(l => l.Name).FirstOrDefault();
                    System.Console.Write($"\n=> Channel ID {chan.Id} (level {levelName})\n");
                    var code = chan.Location.GetText(document);
                    System.Console.Write($"Code: {code}");
                    System.Console.Write($"\nPosition in original code:  {chan.Location.GetLocation(document)}\n");
                    System.Console.Write($"Captured differences:\n{verdict.Messages[chan.Id]}");
                }

            }
            else
            {
                System.Console.WriteLine("No interferent channels detected for the provided input values!");
            }
        }
    }
}
