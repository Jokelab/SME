using Devsense.PHP.Syntax;
using Devsense.PHP.Syntax.Ast;
using Devsense.PHP.Text;
using SME.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SME.Transformer.Php
{
    public class PhpChannelRewriter : TokenVisitor
    {
        private readonly IPolicy _policy;
        private readonly BasicNodesFactory _factory;
        private readonly List<Channel> _outputChannels;
        private readonly List<Channel> _inputChannels;
        private readonly SecurityLevel _securityLevel;
        public PhpChannelRewriter(TreeContext treeContext, ITokenComposer tokenComposer, ISourceTokenProvider sourceTokenProvider, BasicNodesFactory fac, IPolicy policy, List<Channel> inputChannels, List<Channel> outputChannels, SecurityLevel level)
            : base(treeContext, tokenComposer, sourceTokenProvider) { 
            _factory = fac;
            _policy = policy;
            _inputChannels = inputChannels;
            _outputChannels = outputChannels;
            _securityLevel = level;
        }

        public override void VisitItemUse(ItemUse node)
        {
            var inputChannel = FindChannel(node, _inputChannels);
            if (inputChannel != null)
            {
                //only keep the input channel if it's security label >= current level
                if (inputChannel.Label.Level >= _securityLevel.Level)
                {
                    base.VisitItemUse(node);
                }
                else
                {
                    //insert a default value
                    var emptyString = _factory.Literal(new Span(0,2), "''", "''");
                    base.VisitElement(emptyString);
                }

            }
            else
            {
                //not found as input channel
                base.VisitItemUse(node);
            }

        }

        public override void VisitDirectFcnCall(DirectFcnCall node)
        {
            var outputChannel = FindChannel(node, _outputChannels);
            if (outputChannel != null)
            {
                
                //construct a new call to the capture_output function
                var name = new TranslatedQualifiedName(new QualifiedName(new Name("capture_output")), new Span());
                var parameters = new List<ActualParam>();
                parameters.Add(new ActualParam(new Span(), new DoubleLiteral(new Span(), outputChannel.Id)));
                if (node.CallSignature.Parameters.Length > 0)
                {
                    parameters.Add(node.CallSignature.Parameters[0]);
                }

                var signature = new CallSignature(parameters, new Span());
                //let factory create a new DirectFcnCall AST node.
                var captureOutputCall = (DirectFcnCall)_factory.Call(new Span(), name, signature, node.IsMemberOf);

                //visit the new call
                base.VisitDirectFcnCall(captureOutputCall);

                //an output channel is only allowed at the corresponding security level
                if (outputChannel.Label.Level == _securityLevel.Level)
                {
                    //add a semicolon between the new call and the original call
                    base.VisitEmptyStmt((EmptyStmt)_factory.EmptyStmt(new Span(0, 1)));

                    //visit the original call
                    base.VisitDirectFcnCall(node);
                }
            }

            else
            {
                //no special treatment for this function call
                base.VisitDirectFcnCall(node);
            }
        }

        /// <summary>
        /// Check if the AST node is an output channel according to the active policy.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private Channel FindChannel(LangElement node, List<Channel> channels)
        {
            foreach (var channel in channels)
            {
                //spans implement IEquality, so check if this node matches the channel span
                if (channel.Location is PhpSourceLocation channelSource && channelSource.Location == node.Span)
                {
                    return channel;
                }
            }
            return null; //not found
        }


    }
}
