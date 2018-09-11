using Pchp.Core;
using SME.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace SME.Scheduler.Php
{
    public class PhpScheduler : IScheduler
    {
        public void Run(string code, string[] args)
        {
            PhpArray get = new PhpArray();
            get["id"] = PhpValue.Create(999);
            PhpArray post = new PhpArray();
            PhpArray cookies = new PhpArray();
            PhpScriptEvaluator.Evaluate(code, get, post, cookies); ;
        }
    }
}
