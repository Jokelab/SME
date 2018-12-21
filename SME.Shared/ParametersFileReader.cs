using System;
using System.Collections.Generic;
using System.IO;

namespace SME.Shared
{
    /// <summary>
    /// Read the contents of a paramter file
    /// </summary>
    public static class ParametersFileReader
    {
        private const string separator = "|";
        public static List<string[]> Read(string path)
        {
            var result = new List<string[]>();
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                var lines = File.ReadAllLines(path);
                foreach (var line in lines)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        var lineArguments = line.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                        result.Add(lineArguments);
                    }
                }

            }
            return result;
        }
    }
}
