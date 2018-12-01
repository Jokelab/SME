using System;
using System.IO;
using System.Linq;
using SME.Factory;
using SME.Shared;


namespace SME.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            //read input file content
            var inputFilePath = GetFileArgument(args, "-input:", "Sample.php");
            var content = GetInput(inputFilePath);
            if (string.IsNullOrEmpty(content)) return; //exit if there is no input

            //read policy
            var policyPath = GetFileArgument(args, "-policy:", "policy.xml");
            var policy = GetPolicy(policyPath);
            if (policy == null) return; //exit if there is no policy

            //create factory based on the file extension
            var factory = FactoryProducer.GetFactory(Path.GetExtension(inputFilePath));

            //construct transformer and apply transformations
            var transformer = factory.CreateTransformer();
            Console.WriteLine($"Starting transformation of {inputFilePath}");
            var transformations = transformer.Transform(content, policy);
            if (!ValidateTransformations(transformations)) return;
            Console.WriteLine($"Transformation completed. Generated {transformations.CodeTransformations.Count} code versions.");

            //persist transformed code
            if (HasArgument(args, "-save"))
            {
                transformations.SaveTransformations(inputFilePath);
                Console.WriteLine($"Saved the transformed code versions to {Path.GetDirectoryName(inputFilePath)}");
            }

            //construct scheduler and schedule the code for execution
            Console.WriteLine($"Starting scheduler.");
            var scheduler = factory.CreateScheduler();
            scheduler.Schedule(transformations.CodeTransformations, args);
            Console.WriteLine($"Scheduler completed.");

            //determine verdict after execution
            var verdict = scheduler.GetVerdict(transformations.OutputChannels);
            ShowVerdict(verdict, policy, content);

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
                var inputArg = args.FirstOrDefault(arg => arg.ToLowerInvariant().StartsWith(prefix));
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

        /// <summary>
        /// Determine if the specified argument exists ni the list of arguments
        /// </summary>
        /// <param name="args"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        private static bool HasArgument(string[] args, string argument)
        {
            return args.Any(arg => arg.ToLowerInvariant().StartsWith(argument));
        }

        /// <summary>
        /// Get input code from the specified path
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        private static string GetInput(string fullPath)
        {
            var content = string.Empty;
            if (File.Exists(fullPath))
            {
                return File.ReadAllText(fullPath);
            }
            else
            {
                Console.WriteLine($"Input file {fullPath} does not exist.");
            }
            return string.Empty;
        }

        /// <summary>
        /// Read a policy file from the specified path
        /// </summary>
        /// <param name="policyPath"></param>
        /// <returns></returns>
        private static IPolicy GetPolicy(string policyPath)
        {
            
            if (File.Exists(policyPath))
            {
                return PolicyReader.ReadXml<Policy>(policyPath);
            }
            else
            {
                Console.WriteLine($"Policy file {policyPath} does not exist.");
            }
            return null;
        }

        /// <summary>
        /// Perform some checks to determine if the transformation is valid
        /// </summary>
        /// <param name="transformations"></param>
        /// <returns></returns>
        private static bool ValidateTransformations(TransformationResult transformations)
        {
            if (transformations.Errors.Count > 0)
            {
                Console.WriteLine("Error(s) in transformation: ");
                foreach (var error in transformations.Errors)
                {
                    Console.WriteLine(error);
                }
                return false;
            }

            if (transformations.InputChannels.Count == 0)
            {
                Console.WriteLine("No input channels found that match the active policy.");
                return false;
            }

            if (transformations.OutputChannels.Count == 0)
            {
                Console.WriteLine("No output channels found that match the active policy.");
                return false;
            }

            if (transformations.CodeTransformations.Count == 0)
            {
                Console.WriteLine("No code transformations available.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Show verdict about the input code
        /// </summary>
        /// <param name="verdict"></param>
        private static void ShowVerdict(Verdict verdict, IPolicy policy, string document)
        {
            Console.WriteLine("===== VERDICT =====");
            if (verdict.InterferenceDetected())
            {
                //code is interferent
                Console.WriteLine($"Observed {verdict.InterferentChannels.Count} channel(s) with different output values between the SME exection and the original execution.\nThe code is thus interferent.");
                foreach (var chan in verdict.InterferentChannels)
                {
                    var levelName = policy.Levels.Where(l => l.Level == chan.Label.Level).Select(l => l.Name).FirstOrDefault();
                    Console.Write($"\n=> Channel ID {chan.Id} (level {levelName})\n");
                    var code = chan.Location.GetText(document);
                    Console.Write($"Code: {code}");
                    Console.Write($"\nPosition in original code:  {chan.Location.GetLocation(document)}\n");
                    Console.Write($"Captured differences:\n{verdict.Messages[chan.Id]}");
                }

            }
            else
            {
                //code shows non-interference for the provided input values
                Console.WriteLine("No interferent channels detected for the provided input values.");
            }
        }
    }
}
