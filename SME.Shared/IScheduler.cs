using System;
using System.Collections.Generic;
using System.Text;

namespace SME.Shared
{
    public interface IScheduler
    {
        void Run(string code, string[] args);
      
    }
}
