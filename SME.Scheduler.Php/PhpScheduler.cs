using Pchp.Core;
using SME.Shared;
using System;
using System.Collections.Generic;
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

        public void Run(string code, string[] args)
        {
            PhpArray get = new PhpArray();
            get["id"] = PhpValue.Create(args[0]);
            PhpArray post = new PhpArray();
            PhpArray cookies = new PhpArray();
            
            _evaluator.Evaluate(code, get, post, cookies); ;
        }
    }
}
