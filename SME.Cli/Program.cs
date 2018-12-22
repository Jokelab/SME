using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SME.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            //initialize options
            var options = new TestOptions
            {
                //read input file content
                InputPath = GetFileArgument(args, "-input:", @"samples\sqli.php"),

                //file to write test results to
                OutputPath = GetFileArgument(args, "-output:", string.Empty),

                //read policy
                PolicyPath = GetFileArgument(args, "-policy:", "policy.xml"),

                //optional parameters file
                ParametersPath = GetFileArgument(args, "-params:", string.Empty),

                //optional: save transformations or not
                SaveTransformations = HasArgument(args, "-save"),

                //option: write verdict to output file
                ShowVerdict = HasArgument(args, "-showverdict")
            };



            // get the file attributes for file or directory
            FileAttributes attr = File.GetAttributes(options.InputPath);

            //create a test set based on the input directory or for a single file
            var set = new TestSet(options, Console.Out);

            //detect whether its a directory or file
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                //if it is a directory, add all php files to the test set
                var files = Directory.GetFiles(options.InputPath, "*.php");
                foreach (var file in files)
                {
                    set.AddFile(file);
                }
            }
            else
            {
                set.AddFile(options.InputPath);
            }

            //start the tests
            set.RunTests(args);
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
            var currentPath = Directory.GetCurrentDirectory();// Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
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





    }
}
