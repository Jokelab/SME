using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SME.Shared;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SME.Transformer.CSharp
{
    public class CSharpOutputChannelRewriter : CSharpSyntaxRewriter
    {
        private readonly SemanticModel _semanticModel;
        private readonly IPolicy _policy;

        public CSharpOutputChannelRewriter(SemanticModel semanticModel, IPolicy policy)
        {
            _semanticModel = semanticModel;
            _policy = policy;
        }



        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            //var test = SemanticModel.GetTypeInfo(node);

            var symbolInfo = _semanticModel.GetSymbolInfo(node);

            var nameSyntaxExpression = node.Expression as IdentifierNameSyntax;
            if (nameSyntaxExpression != null)
            {
                var oldName = nameSyntaxExpression.Identifier.Text;

                var name = IdentifierName(oldName + "_replaced");
                
                return node.ReplaceNode(node.Expression, name);
            }

            return base.VisitInvocationExpression(node);
        }

        public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            return base.VisitMemberAccessExpression(node);
        }



    }
}
