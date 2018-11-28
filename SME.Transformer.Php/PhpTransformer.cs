using Devsense.PHP.Syntax;
using Devsense.PHP.Syntax.Ast;
using SME.Shared;
using System.Collections.Generic;
using System.Linq;

namespace SME.Transformer.Php
{

    public class PhpTransformer : ITransformer
    {
        private const string filename = "file.php";
        public PhpTransformer()
        {

        }

        public TransformationResult Transform(string content, IPolicy policy)
        {
            var result = new TransformationResult();

            var sourceUnit = new CodeSourceUnit(content, filename, System.Text.Encoding.UTF8, Lexer.LexicalStates.INITIAL, LanguageFeatures.Php71Set);
            var nodesFactory = new BasicNodesFactory(sourceUnit);
            var errors = new PhpErrorSink();
            sourceUnit.Parse(nodesFactory, errors);
            GlobalCode ast = sourceUnit.Ast;
            if (errors.Count != 0)
            {
                ReportErrors(errors, result, content);
                return result; // AST is null or invalid
            }

            //collect channel information from source code
            var provider = SourceTokenProviderFactory.CreateEmptyProvider();
            var collectorComposer = new PhpTokenComposer(provider);
            var collector = new PhpChannelCollector(policy, new TreeContext(ast), collectorComposer, provider);
            collector.VisitElement(ast);
            result.InputChannels.AddRange(collector.InputChannels);
            result.OutputChannels.AddRange(collector.OutputChannels);
            result.SanitizeChannels.AddRange(collector.SanitizeChannels);

            //if there are no output or input channels found in the code, it makes no sense to transform it
            if (result.OutputChannels.Count == 0 || result.InputChannels.Count == 0)
            {
                return result;
            }

            var levels = collector.GetDistinctSecurityLevels().OrderByDescending(sl => sl.Level);

            //append a sanitize transformation if there are sanitize channels
            var lowestInputLevel = result.InputChannels.Min(sc => sc.Label.Level);
            if (result.SanitizeChannels.Any())
            {
                var sanSourceUnit = new CodeSourceUnit(content, filename, System.Text.Encoding.UTF8, Lexer.LexicalStates.INITIAL, LanguageFeatures.Php71Set);
                var sanNodesFactory = new BasicNodesFactory(sourceUnit);
                var sanErrors = new PhpErrorSink();
                sanSourceUnit.Parse(sanNodesFactory, sanErrors);
                GlobalCode sanAst = sanSourceUnit.Ast;
                if (sanErrors.Count != 0)
                {
                    return result; // AST is null or invalid
                }

                var pSanitize = new CodeTransformation();
                pSanitize.Kind = TransformationKind.Sanitize;
                pSanitize.SecurityLevel = new SecurityLevel() { Level = lowestInputLevel - 1, Name = "PS" };
                var composer = new PhpTokenComposer(provider);
                var rewriter = new PhpChannelRewriter(new TreeContext(sanAst), composer, provider, nodesFactory, policy, collector.InputChannels, collector.OutputChannels, collector.SanitizeChannels, pSanitize.SecurityLevel);
                rewriter.VisitElement(sanAst);
                pSanitize.Code = composer.Code.ToString();

                result.CodeTransformations.Add(pSanitize);
            }

            //create code version for each security level
            foreach (var level in levels)
            {
                var levelSourceUnit = new CodeSourceUnit(content, filename, System.Text.Encoding.UTF8, Lexer.LexicalStates.INITIAL, LanguageFeatures.Php71Set);
                var levelNodesFactory = new BasicNodesFactory(levelSourceUnit);
                var levelerrors = new PhpErrorSink();
                levelSourceUnit.Parse(nodesFactory, levelerrors);
                GlobalCode levelAst = levelSourceUnit.Ast;
                if (levelerrors.Count != 0)
                {
                    return result; // AST is null or invalid
                }


                var version = new CodeTransformation();
                version.Kind = TransformationKind.Default;
                version.SecurityLevel = level;
                var composer = new PhpTokenComposer(provider);
                var rewriter = new PhpChannelRewriter(new TreeContext(levelAst), composer, provider, levelNodesFactory, policy, collector.InputChannels, collector.OutputChannels, collector.SanitizeChannels, level);
                rewriter.VisitElement(levelAst);
                version.Code = composer.Code.ToString();


                result.CodeTransformations.Add(version);
            }

            //create PO version
            var poSourceUnit = new CodeSourceUnit(content, filename, System.Text.Encoding.UTF8, Lexer.LexicalStates.INITIAL, LanguageFeatures.Php71Set);
            var poNodesFactory = new BasicNodesFactory(poSourceUnit);
            var poErrors = new PhpErrorSink();
            poSourceUnit.Parse(poNodesFactory, poErrors);
            GlobalCode poAst = poSourceUnit.Ast;

            var po = new CodeTransformation();
            po.Kind = TransformationKind.Original;
            var poComposer = new PhpTokenComposer(provider);
            po.SecurityLevel = new SecurityLevel() { Level = lowestInputLevel, Name = "PO'" };
            var poRewriter = new PhpChannelRewriter(new TreeContext(poAst), poComposer, provider, poNodesFactory, policy, collector.InputChannels, collector.OutputChannels, collector.SanitizeChannels, po.SecurityLevel, isOriginalProgram: true);
            poRewriter.VisitElement(poAst);
            po.Code = poComposer.Code.ToString();
            result.CodeTransformations.Add(po);

            return result;

        }

        /// <summary>
        /// Copy errorsink values to error collection in output
        /// </summary>
        /// <param name="errorSink"></param>
        /// <param name="result"></param>
        private void ReportErrors(PhpErrorSink errorSink, TransformationResult result, string code)
        {
            foreach(var error in errorSink.Errors)
            {
                var sourceLocation = new PhpSourceLocation(error.Span);
                result.Errors.Add(error.ToString() + " Location: "+ sourceLocation.GetLocation(code));
            }
        }

        //private GlobalCode CreateAst(string content)
        //{
        //    var sourceUnit = new CodeSourceUnit(content, filename, System.Text.Encoding.UTF8, Lexer.LexicalStates.INITIAL, LanguageFeatures.Php71Set);
        //    var levelSourceUnit = new CodeSourceUnit(content, filename, System.Text.Encoding.UTF8, Lexer.LexicalStates.INITIAL, LanguageFeatures.Php71Set);
        //    var nodesFactory = new BasicNodesFactory(sourceUnit);
        //    var errors = new PhpErrorSink();
        //    sourceUnit.Parse(nodesFactory, errors);
            
        //    if (errors.Count != 0)
        //    {
        //        throw new System.Exception("Problem while creating AST from code. ");
        //    }
        //    return sourceUnit.Ast;
        //}


    }

}
