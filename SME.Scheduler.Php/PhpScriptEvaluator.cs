using Pchp.Core;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System;
using SME.Shared;

namespace SME.Scheduler.Php
{
    public class PhpScriptEvaluator
    {
        public static void Evaluate(string code, PhpArray getVariables, PhpArray postVariables, PhpArray cookieVariables)
        {

            //use IScriptingProvider singleton 
            var provider = Context.GlobalServices.GetService<Context.IScriptingProvider>();
            using (var ctx = Context.CreateConsole(string.Empty, new string[] { }))
            {
                //declare a methos that captures the output
                ctx.DeclareFunction("capture_output", new Action<int, string>((id, val) => ctx.Echo($"Captured value for channel {id}: {val}\n")));
                ctx.Get = getVariables;
                ctx.Post = postVariables;
                ctx.Cookie = cookieVariables;

                var script = provider.CreateScript(new Context.ScriptOptions()
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
