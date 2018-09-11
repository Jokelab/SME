using System.Collections.Generic;

namespace SME.Shared
{
    public class TransformationResult
    {
        public List<CodeTransformation> CodeTransformations { get; private set; }
        public List<Channel> InputChannels { get; private set; }
        public List<Channel> OutputChannels { get; private set; }

        public TransformationResult()
        {
            CodeTransformations = new List<CodeTransformation>();
            InputChannels = new List<Channel>();
            OutputChannels = new List<Channel>();
        }
    }
}
