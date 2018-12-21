using Pchp.Core;
using SME.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SME.Scheduler.Php
{
    public class PhpScheduler : IScheduler
    {
        private PhpScriptEvaluator _evaluator;
        private readonly MemoryStore _smeMemoryStore = new MemoryStore();
        private readonly MemoryStore _originalMemoryStore = new MemoryStore();

        public PhpScheduler()
        {
            _evaluator = new PhpScriptEvaluator();
        }

        private void Run(CodeTransformation version, string[] args, MemoryStore memoryStore)
        {
            PhpArray get = PhpArgumentParser.Parse("get", args);
            PhpArray post = PhpArgumentParser.Parse("post", args);
            PhpArray cookies = PhpArgumentParser.Parse("cookies", args);
            PhpArray session = PhpArgumentParser.Parse("session", args);
            _evaluator.Evaluate(version, get, post, cookies, session, memoryStore);
        }

        public virtual IEnumerable<CodeTransformation> SortTransformations(IEnumerable<CodeTransformation> codeTransformations)
        {
            //assume they are added in the correct order, so there is no need to sort
            return codeTransformations;
        }

        public void Schedule(IEnumerable<CodeTransformation> codeTransformations, string[] args)
        {

            var sorted = SortTransformations(codeTransformations);
            foreach (var version in sorted)
            {
                //Console.WriteLine($"Executing level {version.SecurityLevel.Name} ({version.SecurityLevel.Level}):");

                var memStore = version.Kind == TransformationKind.Original ? _originalMemoryStore : _smeMemoryStore;
                Run(version, args, memStore);
                //Console.Write("\n\n");
            }
        }

        public Verdict GetVerdict(List<Channel> outputChannels)
        {
            return OutputComparer.Compare(outputChannels, _smeMemoryStore, _originalMemoryStore);
        }
    }
}
