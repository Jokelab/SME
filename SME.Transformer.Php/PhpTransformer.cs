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

            var levels = collector.GetDistinctSecurityLevels().OrderByDescending(sl => sl.Level);

            //append a sanitize transformation if there are sanitize channels
            var lowestInputLevel = result.InputChannels.Min(sc => sc.Label.Level);
            if (result.SanitizeChannels.Any())
            {
                var pSanitize = new CodeTransformation();
                pSanitize.Kind = TransformationKind.Sanitize;
                pSanitize.SecurityLevel = new SecurityLevel() { Level = lowestInputLevel - 1, Name = "PS" };
                var composer = new PhpTokenComposer(provider);
                var rewriter = new PhpChannelRewriter(new TreeContext(ast), composer, provider, nodesFactory, policy, collector.InputChannels, collector.OutputChannels, collector.SanitizeChannels, pSanitize.SecurityLevel);
                rewriter.VisitElement(ast);
                pSanitize.Code = composer.Code.ToString();

                result.CodeTransformations.Add(pSanitize);
            }

            //create code version for each security level
            foreach (var level in levels)
            {
                var version = new CodeTransformation();
                version.Kind = TransformationKind.Default;
                version.SecurityLevel = level;
                var composer = new PhpTokenComposer(provider);
                var rewriter = new PhpChannelRewriter(new TreeContext(ast), composer, provider, nodesFactory, policy, collector.InputChannels, collector.OutputChannels, collector.SanitizeChannels, level);
                rewriter.VisitElement(ast);
                version.Code = composer.Code.ToString();


                result.CodeTransformations.Add(version);
            }

            //create PO version
            var po = new CodeTransformation();
            po.Kind = TransformationKind.Original;
            var poComposer = new PhpTokenComposer(provider);
            po.SecurityLevel = new SecurityLevel() { Level = lowestInputLevel, Name = "PO'" };
            var poRewriter = new PhpChannelRewriter(new TreeContext(ast), poComposer, provider, nodesFactory, policy, collector.InputChannels, collector.OutputChannels, collector.SanitizeChannels, po.SecurityLevel, captureAllOutput: true);
            poRewriter.VisitElement(ast);
            po.Code = poComposer.Code.ToString();
            result.CodeTransformations.Add(po);

            return result;

        }


    }

}
