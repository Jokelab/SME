using Pchp.Core;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System;
using SME.Shared;

namespace SME.Scheduler.Php
{
    public class PhpScriptEvaluator
    {
        private Context.IScriptingProvider _provider = Context.GlobalServices.GetService<Context.IScriptingProvider>();
        public PhpScriptEvaluator()
        {
        }

        public void Evaluate(string code, PhpArray getVariables, PhpArray postVariables, PhpArray cookieVariables)
        {

            //Context.CreateConsole() is a Peachpie runtime object, representing a PHP runtime thread. 
            using (var ctx = Context.CreateConsole(string.Empty, new string[] { }))
            {
                //declare a method that captures the output
                ctx.DeclareFunction("capture_output", new Action<int, string>((id, val) => ctx.Echo($"Captured value for channel {id}: {val}\n")));
                ctx.DeclareFunction("capture_sanitize", new Func<int, string, object>((id, val) => { ctx.Echo($"Captured sanitized value for channel {id}: {(val != null ? val.ToString() : string.Empty)}\n"); return val; }));
                ctx.Get = getVariables;
                ctx.Post = postVariables;
                ctx.Cookie = cookieVariables;

                var script = _provider.CreateScript(new Context.ScriptOptions()
                {
                    Context = ctx,
                    Location = new Location(string.Empty, 0, 0),
                    EmitDebugInformation = true,
                    IsSubmission = false,

                    AdditionalReferences = new string[] {
                        typeof(Peachpie.Library.Graphics.PhpImage).Assembly.Location,
                        typeof(Peachpie.Library.Network.CURLFunctions).Assembly.Location,
                        typeof(Peachpie.Library.MySql.MySql).Assembly.Location
                    },
                }, code);

                //evaluate the php code
                script.Evaluate(ctx, ctx.Globals, null);
            }
        }
    }
}
