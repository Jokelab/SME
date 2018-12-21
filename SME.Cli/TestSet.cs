using SME.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SME.Cli
{
    public class TestSet
    {
        private readonly string _policyPath;
        private readonly IPolicy _policy;
        private readonly string _outputPath;
        private readonly string _parametersPath;
        private TextWriter _writer;
        private List<string> _files = new List<string>();
        private readonly bool _saveTransformations;

        public TestSet(string policyPath, string outputPath, string parametersPath, bool saveTransformations, TextWriter writer)
        {
            _policyPath = policyPath;
            _policy = TestCase.GetPolicy(policyPath);
            _outputPath = outputPath;
            _parametersPath = parametersPath;
            _writer = writer;
            _saveTransformations = saveTransformations;
        }

        public void AddFile(string filePath)
        {
            _files.Add(filePath);
        }

        public int Count
        {
            get
            {
                return _files.Count;
            }
        }

        public List<TestOutput> RunTests(string[] args)
        {
            var result = new List<TestOutput>();

            var allTasks = new List<Task>();

            var parameters = ParametersFileReader.Read(_parametersPath);
            if (parameters.Count == 0) parameters.Add(args);

            foreach (var file in _files)
            {

                try
                {
                    _writer.WriteLine("Starting tests for " + file);

                    foreach (var paramValues in parameters)
                    {
                        var test = new TestCase(file, _policy, _saveTransformations, paramValues, _writer);
                        var verdict = test.RunTest();
                        if (verdict.InterferenceDetected())
                        {
                            WriteOutput($"{Path.GetFileName(file)} is interferent for {string.Join(",", paramValues)}.");
                            result.Add(new TestOutput() { TestCase = test, Verdict = verdict });

                            //stop trying other input values if interference is detected
                            break;
                        }
                        else
                        {
                            WriteOutput($"{Path.GetFileName(file)} is ok.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _writer.Write("Error while processing: " + file + ex.ToString());
                }
                finally
                {

                    _writer.WriteLine("Done with " + file);
                }

            }

            return result;
        }

        private void WriteOutput(string message)
        {
            if (!string.IsNullOrEmpty(_outputPath))
            {
                File.AppendAllLines(_outputPath, new string[] { $"{DateTime.Now.ToLongTimeString()}: {message}" });
            }
        }

    }
}
