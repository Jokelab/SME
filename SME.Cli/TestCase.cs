using SME.Factory;
using SME.Shared;
using System;
using System.IO;

namespace SME.Cli
{
    public class TestCase
    {
        private string _inputFilePath;
        private bool _saveTransformations;
        private TextWriter _writer;
        private string[] _arguments;

        public TestCase(string inputFile, IPolicy policy, bool saveTransformations, string[] arguments, TextWriter writer)
        {

            _inputFilePath = inputFile;
            InputFileContent = GetInput(inputFile);
            Policy = policy;
            _saveTransformations = saveTransformations;
            _arguments = arguments;
            _writer = writer;
        }


        public Verdict RunTest()
        {
            //create factory based on the file extension
            var factory = FactoryProducer.GetFactory(Path.GetExtension(_inputFilePath));

            //construct transformer and apply transformations
            var transformer = factory.CreateTransformer();
            WriteLine($"Starting transformation of {_inputFilePath}");
            var transformations = transformer.Transform(InputFileContent, Policy);

            if (!ValidateTransformations(transformations))
            {
                var errorVerdict = new Verdict();
                errorVerdict.TransformationResult = transformations;
                return errorVerdict;
            }
            WriteLine($"Transformation completed. Generated {transformations.CodeTransformations.Count} code versions.");

            //persist transformed code
            if (_saveTransformations)
            {
                transformations.SaveTransformations(_inputFilePath);
                WriteLine($"Saved the transformed code versions to {Path.GetDirectoryName(_inputFilePath)}");
            }

            //construct scheduler and schedule the code for execution
            WriteLine($"Starting scheduler.");
            var scheduler = factory.CreateScheduler();
            scheduler.Schedule(transformations.CodeTransformations, _arguments);
            WriteLine($"Scheduler completed.");

            //determine verdict after execution
            return scheduler.GetVerdict(transformations.OutputChannels);
        }

        private void WriteLine(string text)
        {
            if (_writer != null)
            {
                _writer.WriteLine(text);
            }
        }

        public string InputFileContent { get; }

        public IPolicy Policy { get; set; }

        /// <summary>
        /// Perform some checks to determine if the transformation is valid
        /// </summary>
        /// <param name="transformations"></param>
        /// <returns></returns>
        private bool ValidateTransformations(TransformationResult transformations)
        {
            if (transformations.Errors.Count > 0)
            {
                WriteLine("Error(s) in transformation: ");
                foreach (var error in transformations.Errors)
                {
                    Console.WriteLine(error);
                }
                return false;
            }

            if (transformations.InputChannels.Count == 0)
            {
                WriteLine("No input channels found that match the active policy.");
                return false;
            }

            if (transformations.OutputChannels.Count == 0)
            {
                WriteLine("No output channels found that match the active policy.");
                return false;
            }

            if (transformations.CodeTransformations.Count == 0)
            {
                WriteLine("No code transformations available.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Get input code from the specified path
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public static string GetInput(string fullPath)
        {
            if (File.Exists(fullPath))
            {
                return File.ReadAllText(fullPath);
            }
            return string.Empty;
        }

        /// <summary>
        /// Read a policy file from the specified path
        /// </summary>
        /// <param name="policyPath"></param>
        /// <returns></returns>
        public static IPolicy GetPolicy(string policyPath)
        {
            if (File.Exists(policyPath))
            {
                return PolicyReader.ReadXml<Policy>(policyPath);
            }
            return null;
        }


    }
}
