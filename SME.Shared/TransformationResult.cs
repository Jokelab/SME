using System.Collections.Generic;
using System.IO;

namespace SME.Shared
{
    public class TransformationResult
    {
        public List<CodeTransformation> CodeTransformations { get; private set; }
        public List<Channel> InputChannels { get; private set; }
        public List<Channel> OutputChannels { get; private set; }
        public List<Channel> SanitizeChannels { get; private set; }

        public TransformationResult()
        {
            CodeTransformations = new List<CodeTransformation>();
            InputChannels = new List<Channel>();
            OutputChannels = new List<Channel>();
            SanitizeChannels = new List<Channel>();
        }

        /// <summary>
        /// Persist transformed code to files.
        /// </summary>
        /// <param name="originalPath">Path to the original source file</param>
        public void SaveTransformations(string originalPath)
        {
            foreach (var version in CodeTransformations)
            {
                string filename = Path.GetFileNameWithoutExtension(originalPath);
                string outputPath = originalPath.Replace(filename, $"{filename}.{version.SecurityLevel.Name}");

                //write code transformation to output
                File.WriteAllText(outputPath, version.Code);
            }
        }
    }
}
