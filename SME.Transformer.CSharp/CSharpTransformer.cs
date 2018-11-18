using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SME.Shared;

namespace SME.Transformer.CSharp
{
    public class CSharpTransformer: ITransformer
    {
        
        public CSharpTransformer()
        {
        }

        public CSharpSyntaxTree CreateSyntaxTree(string sourceCode)
        {
            if (string.IsNullOrEmpty(sourceCode))
            {
                throw new ArgumentNullException(nameof(sourceCode));
            }
            return (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(sourceCode);


        }

        public  TransformationResult Transform(string sourceCode, IPolicy policy)
        {
            var tree = CreateSyntaxTree(sourceCode);

            var result = new TransformationResult();
            var channelCollector = new CSharpChannelCollector(policy);
            channelCollector.Visit(tree.GetRoot());

            var compilation = CSharpCompilation.Create("PO")
            .AddReferences(
                 MetadataReference.CreateFromFile(
                     typeof(object).Assembly.Location))
            .AddSyntaxTrees(tree);



            var rewriter = new CSharpOutputChannelRewriter(compilation.GetSemanticModel(tree, true), policy);

            var node = rewriter.Visit(tree.GetRoot());


            result.CodeTransformations.Add( new CodeTransformation() { SecurityLevel = new SecurityLevel() { Name = "PH", Level = 1 }, Code = result.ToString() });
            

            return result ;

        }

   
    }
}
