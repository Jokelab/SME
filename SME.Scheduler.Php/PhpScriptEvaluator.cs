using Pchp.Core;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System;
using SME.Shared;
using SME.Shared.Constants;

namespace SME.Scheduler.Php
{
    public class PhpScriptEvaluator
    {
        private Context.IScriptingProvider _provider = Context.GlobalServices.GetService<Context.IScriptingProvider>();

        public PhpScriptEvaluator()
        {

        }

        public void Evaluate(CodeTransformation codeTransformation, PhpArray getVariables, PhpArray postVariables, PhpArray cookieVariables, PhpArray sessionVariables, MemoryStore memoryStore)
        {

            var fullpath = Path.Combine(Directory.GetCurrentDirectory(), "main.php");

            //Context.CreateConsole() is a Peachpie runtime object, representing a PHP runtime thread. 
            using (var ctx = Context.CreateConsole(string.Empty, new string[] { }))
            {
                //declare methods to store/read channel values
                ctx.DeclareFunction(FunctionNames.StoreInput, new Func<int, string, string>((id, val) => { ctx.Echo($"Stored input value for channel {id}: {val}\n"); memoryStore.Store(id, val); return val; }));
                ctx.DeclareFunction(FunctionNames.GetInput, new Func<int, string, string>((id, val) => { var readValue = memoryStore.Get(id, codeTransformation.SecurityLevel.Level); ctx.Echo($"Get input value for channel {id}: {readValue}\n"); return readValue; }));
                ctx.DeclareFunction(FunctionNames.StoreSanitize, new Func<int, string, string>((id, val) => { ctx.Echo($"Stored sanitized value for channel {id}: {val}\n"); memoryStore.Store(id, val); return val; }));
                ctx.DeclareFunction(FunctionNames.GetSanitize, new Func<int, string, string>((id, val) => { var readValue = memoryStore.Get(id, codeTransformation.SecurityLevel.Level); ctx.Echo($"Get sanitized value for channel {id}: {readValue}\n"); return readValue; }));

                ctx.DeclareFunction(FunctionNames.CaptureOutput, new Action<int, string>((id, val) => { ctx.Echo($"Captured output value for channel {id}: {val}\n"); }));
                ctx.DeclareFunction(FunctionNames.StoreOutput, new Action<int, string>((id, val) => { ctx.Echo($"Stored output value for channel {id}: {val}\n"); memoryStore.Store(id, val); }));
                ctx.DeclareFunction(FunctionNames.GetOutput, new Func<int, string>((id) => { var readValue = memoryStore.Get(id, codeTransformation.SecurityLevel.Level); ctx.Echo($"Get output value for channel {id}: {readValue}\n"); return readValue; }));

                ctx.Get = getVariables;
                ctx.Post = postVariables;
                ctx.Cookie = cookieVariables;
                ctx.Session = sessionVariables;

                var script = _provider.CreateScript(new Context.ScriptOptions()
                {
                    Context = ctx,
                    Location = new Location(fullpath, 0, 0),
                    EmitDebugInformation = true,
                    IsSubmission = false,
                    AdditionalReferences = new string[] {
                        typeof(Peachpie.Library.Graphics.PhpImage).Assembly.Location,
                        typeof(Peachpie.Library.Network.CURLFunctions).Assembly.Location,
                        typeof(Peachpie.Library.MySql.MySql).Assembly.Location
                    },
                }, codeTransformation.Code);


                //evaluate the php code
                script.Evaluate(ctx, ctx.Globals, null);


            }
        }
    }
}
