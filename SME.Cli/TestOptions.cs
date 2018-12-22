using SME.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace SME.Cli
{
    /// <summary>
    /// Datastructure that holds the properties for a test set
    /// </summary>
    public class TestOptions
    {
        public string InputPath { get; set; }
        public string PolicyPath { get; set; }
        public string OutputPath { get; set; }
        public string ParametersPath { get; set; }
        public bool SaveTransformations { get; set; }
        public bool ShowVerdict { get; set; }
    }
}
