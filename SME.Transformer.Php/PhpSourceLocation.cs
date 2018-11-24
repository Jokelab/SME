using Devsense.PHP.Text;
using SME.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace SME.Transformer.Php
{
    public class PhpSourceLocation : ISourceLocation
    {
        public Span Location { get; set; }

        public PhpSourceLocation(Span location)
        {
            Location = location;
        }

        /// <summary>
        /// Get string representation of the source location
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public string GetText(string document)
        {
            var code = string.Empty;
            try
            {
                code = Location.GetText(document);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex);
            }
            return code;
        }

        public string GetLocation()
        {
            return $"{Location.Start}..{Location.End}";
        }
    }
}
