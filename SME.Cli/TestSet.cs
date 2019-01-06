using SME.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SME.Cli
{
    public class TestSet
    {
        private readonly TestOptions _options;
        private readonly IPolicy _policy;
        private TextWriter _writer;
        private List<string> _files = new List<string>();

        public TestSet(TestOptions options, TextWriter writer)
        {
            _options = options;
            _policy = TestCase.GetPolicy(_options.PolicyPath);
            _writer = writer;
        }

        public void AddFile(string filePath)
        {
            _files.Add(filePath);
        }

        /// <summary>
        /// Number of files in the test set
        /// </summary>
        public int Count
        {
            get
            {
                return _files.Count;
            }
        }

        /// <summary>
        /// Execute all tests in the test set
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public List<TestOutput> RunTests(string[] args)
        {
            var result = new List<TestOutput>();
            int errorCount = 0;
            var allTasks = new List<Task>();
            var startTime = DateTime.Now;
            //fetch input values
            var parameters = ParametersFileReader.Read(_options.ParametersPath);
            if (parameters.Count == 0) parameters.Add(args);

            WriteOutput($"=== Test started ===");

            foreach (var file in _files)
            {

                try
                {
                    _writer.WriteLine($"Starting tests for {file}");
                    
                    foreach (var paramValues in parameters)
                    {
                        var test = new TestCase(file, _policy, _options.SaveTransformations, paramValues, _writer);
                        var verdict = test.RunTest();
                        if (verdict.InterferenceDetected())
                        {
                            WriteOutput($"{Path.GetFileName(file)} is interferent for input values: {string.Join(",", paramValues)}.");
                            result.Add(new TestOutput() { TestCase = test, Verdict = verdict });

                            if (_options.ShowVerdict)
                            {
                                var verdictText = verdict.Show(_policy, test.InputFileContent);
                                //write to console
                                WriteOutput(verdictText);
                            }
                            //stop trying other input values if interference is detected
                            break;
                        }
                        else if (verdict.TransformationResult != null && verdict.TransformationResult.Errors.Count > 0)
                        {
                            WriteOutput($"{Path.GetFileName(file)} failed transformation: {string.Join(",", verdict.TransformationResult.Errors)}");
                            errorCount++;
                            break;
                        }
                        else
                        {
                            WriteOutput($"{Path.GetFileName(file)} seems noninterferent with input values: {string.Join(",", paramValues)}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _writer.Write($"Error while processing { file}: {ex}");
                }
                finally
                {

                    _writer.WriteLine($"Done with {file}");
                }

            }

            WriteSummary(result, errorCount, startTime, DateTime.Now);

            return result;
        }

        private void WriteSummary(List<TestOutput> testResult, int errorCount, DateTime start, DateTime end)
        {
            var ratio = testResult.Count / (decimal)_files.Count;
            var errorRatio = errorCount / (decimal)_files.Count;
            if (_files.Count > 1)
            {
                WriteOutput($"\n\n=== Test summary ===");
                WriteOutput($"Result: detected {testResult.Count}/{_files.Count} programs ({ratio.ToString("0.0%")}) which show interference.");
                WriteOutput($"Result: {errorCount}/{_files.Count} programs ({errorRatio.ToString("0.0%")}) which were erroneous (no input/output channels or syntax errors).");
                WriteOutput($"Duration: {end-start}. Average time per program: {(end-start).TotalSeconds / _files.Count} seconds.");
            }
        }

        private void WriteOutput(string message)
        {
            _writer.WriteLine(message);
            if (!string.IsNullOrEmpty(_options.OutputPath))
            {
                File.AppendAllLines(_options.OutputPath, new string[] { $"{DateTime.Now.ToLongTimeString()}: {message}" });
            }
        }

    }
}
