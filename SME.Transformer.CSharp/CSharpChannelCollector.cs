using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SME.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace SME.Transformer.CSharp
{
    public class CSharpChannelCollector: CSharpSyntaxWalker
    {
        private readonly IPolicy _policy;
        private List<SecurityLevel> _distinctLevels;
        public readonly List<InvocationExpressionSyntax> OutputChannels = new List<InvocationExpressionSyntax>();
        public readonly List<InvocationExpressionSyntax> InputChannels = new List<InvocationExpressionSyntax>();

        public CSharpChannelCollector(IPolicy policy)
        {
            _policy = policy;
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {

            if (MatchesInputChannel(node))
            {
                InputChannels.Add(node);

            }
   
            if (MatchesOutputChannel(node))
            {
                OutputChannels.Add(node);
            }
            
        }

        public override void Visit(SyntaxNode node)
        {
            _distinctLevels = new List<SecurityLevel>();
            base.Visit(node);
        }

        private bool MatchesInputChannel(InvocationExpressionSyntax node)
        {
           // _policy.InputChannels.Find(channel => channel.Name == node.Expression.)
            return true;
        }

        private bool MatchesOutputChannel(InvocationExpressionSyntax node)
        {
            return true;
        }

        public SecurityLevel[] DistinctLevels
        {
            get
            {
                if (_distinctLevels == null)
                {
                    throw new Exception($"Run the {nameof(Visit)} method first to determine the available security levels");
                }
                return _distinctLevels.ToArray();
            }
        }
    }
}
