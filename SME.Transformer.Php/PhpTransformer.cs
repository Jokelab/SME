using Devsense.PHP.Syntax;
using Devsense.PHP.Syntax.Ast;
using SME.Shared;
using System.Collections.Generic;

namespace SME.Transformer.Php
{

    public class PhpTransformer: ITransformer
    {

        public PhpTransformer()
        {
          
        }

        public TransformationResult Transform(string content, IPolicy policy)
        {
            var result = new TransformationResult();

            const string filename = "file.php";
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
            result.InputChannels.AddRange(collector.GetInputChannels());
            result.OutputChannels.AddRange(collector.GetOutputChannels());

            var levels = collector.GetDistinctSecurityLevels();

            //create code version for each security level
            foreach (var level in levels)
            {
                var version = new CodeTransformation();
                var composer = new PhpTokenComposer(provider);
                var rewriter = new PhpChannelRewriter(new TreeContext(ast), composer, provider, nodesFactory, policy, collector.GetInputChannels(), collector.GetOutputChannels(), level);
                rewriter.VisitElement(ast);

                version.Code = composer.Code.ToString();
                version.Level = level;
                result.CodeTransformations.Add(version);
            }
            var po = new CodeTransformation();
            var poComposer = new PhpTokenComposer(provider);
            var poRewriter = new PhpChannelRewriter(new TreeContext(ast), poComposer, provider, nodesFactory, policy, collector.GetInputChannels(), collector.GetOutputChannels(), new SecurityLevel() { Level = 0, Name = "Original" });
            poRewriter.VisitElement(ast);
            po.Code = poComposer.Code.ToString();
            po.Level = new SecurityLevel() { Level = 0, Name = "Original" };
            result.CodeTransformations.Add(po);

            return result;
     
        }

        
    }

}
