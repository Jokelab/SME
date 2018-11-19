﻿using Pchp.Core;
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
        private readonly MemoryStore _defaultMemoryStore = new MemoryStore();
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

            _evaluator.Evaluate(version, get, post, cookies, memoryStore); ;
        }



        public virtual IEnumerable<CodeTransformation> SortTransformations(IEnumerable<CodeTransformation> codeTransformations)
        {
            return codeTransformations.OrderByDescending(ct => ct.SecurityLevel.Level);
        }

        public void Schedule(IEnumerable<CodeTransformation> codeTransformations, string[] args)
        {

            var sorted = SortTransformations(codeTransformations);
            foreach (var version in sorted)
            {
                Console.WriteLine("Now executing level " + version.SecurityLevel.Name + ":");

                var memStore = version.IsOriginal ? _originalMemoryStore : _defaultMemoryStore;
                Run(version, args, memStore);
                Console.Write("\n\n");
            }
        }
    }
}
