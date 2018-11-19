using Pchp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SME.Scheduler.Php
{
    public class PhpArgumentParser
    {

        public static PhpArray Parse(string kind, string[] args)
        {
            var array = new PhpArray();
            if (args.Length > 0)
            {
                var prefix = $"-{kind}?";
                var queryString = args.FirstOrDefault(arg => arg.StartsWith(prefix));
                if (!string.IsNullOrEmpty(queryString))
                {
                    queryString = queryString.Substring(prefix.Length);
                    var collection = System.Web.HttpUtility.ParseQueryString(queryString);
                    foreach (var key in collection.Keys)
                    {
                        array[key] = PhpValue.Create(collection[key.ToString()]);
                    }
                }

            }
            return array;
        }
    }
}
