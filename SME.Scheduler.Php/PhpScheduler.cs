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

        public PhpScheduler()
        {
            _evaluator = new PhpScriptEvaluator();
        }

        private void Run(CodeTransformation version, string[] args)
        {
            PhpArray get = new PhpArray();
            get["id"] = PhpValue.Create(args[0]);
            PhpArray post = new PhpArray();
            PhpArray cookies = new PhpArray();
            
            _evaluator.Evaluate(version.Code, get, post, cookies); ;
        }

        public virtual IEnumerable<CodeTransformation> SortTransformations(IEnumerable<CodeTransformation> codeTransformations)
        {
            return codeTransformations.OrderByDescending(ct => ct.Level);
        }

        public void Schedule(IEnumerable<CodeTransformation> codeTransformations, string[] args)
        {
            foreach (var version in codeTransformations)
            {
                Console.WriteLine("Now executing level " + version.Level.Name + ":");
                Run(version, args);
                Console.Write("\n\n");
            }
        }
    }
}
