using Devsense.PHP.Syntax;
using Devsense.PHP.Syntax.Ast;
using Devsense.PHP.Text;
using SME.Shared;
using System.Collections.Generic;

namespace SME.Transformer.Php
{
    public class PhpChannelRewriter : TokenVisitor
    {
        private readonly IPolicy _policy;
        private readonly BasicNodesFactory _factory;
        private readonly List<Channel> _outputChannels;
        private readonly List<Channel> _inputChannels;
        private readonly List<Channel> _sanitizeChannels;
        private readonly SecurityLevel _securityLevel;

        private List<int> _visitedChannels = new List<int>();
        public PhpChannelRewriter(TreeContext treeContext, ITokenComposer tokenComposer, ISourceTokenProvider sourceTokenProvider, BasicNodesFactory fac, IPolicy policy, List<Channel> inputChannels, List<Channel> outputChannels, List<Channel> sanitizeChannels, SecurityLevel level)
            : base(treeContext, tokenComposer, sourceTokenProvider)
        {
            _factory = fac;
            _policy = policy;
            _inputChannels = inputChannels;
            _outputChannels = outputChannels;
            _sanitizeChannels = sanitizeChannels;
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
                    var emptyString = _factory.Literal(new Span(0, 2), "''", "''");
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
            var sanitizeChannel = FindChannel(node, _sanitizeChannels);
            if (outputChannel != null)
            {
                RewriteOutputChannel(outputChannel, node);

            }
            else if (sanitizeChannel != null)
            {
                //keep track of visited channels
                if (!_visitedChannels.Contains(sanitizeChannel.Id))
                {
                    _visitedChannels.Add(sanitizeChannel.Id);
                    //rewrite sanitize channel
                    RewriteSanitizeChannel(sanitizeChannel, node);
                }
                else
                {
                    base.VisitDirectFcnCall(node);
                }
            }
            else
            {
                //no special treatment for this function call
                base.VisitDirectFcnCall(node);
            }
        }

        private void RewriteOutputChannel(Channel outputChannel, DirectFcnCall node)
        {
            //construct a new call to the capture_output function
            var name = new TranslatedQualifiedName(new QualifiedName(new Name("capture_output")), new Span());
            var parameters = new List<ActualParam>();
            parameters.Add(new ActualParam(new Span(), new LongIntLiteral(new Span(), outputChannel.Id)));
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

        private void RewriteSanitizeChannel(Channel sanitizeChannel, DirectFcnCall node)
        {
            //construct a new call to the capture_output function
            var name = new TranslatedQualifiedName(new QualifiedName(new Name("capture_sanitize")), new Span());
            var parameters = new List<ActualParam>();
            parameters.Add(new ActualParam(new Span(), new LongIntLiteral(new Span(), sanitizeChannel.Id)));
            parameters.Add(new ActualParam(new Span(), node));


            var signature = new CallSignature(parameters, new Span());
            //let factory create a new DirectFcnCall AST node.
            var captureSanitizeCall = (DirectFcnCall)_factory.Call(new Span(), name, signature, node.IsMemberOf);

            //visit the new call
            base.VisitDirectFcnCall(captureSanitizeCall);

            //an output channel is only allowed at the corresponding security level
            //if (sanitizeChannel.Label.Level == _securityLevel.Level)
            //{
            //    //add a semicolon between the new call and the original call
            //    base.VisitEmptyStmt((EmptyStmt)_factory.EmptyStmt(new Span(0, 1)));

            //    //visit the original call
            //    base.VisitDirectFcnCall(node);
            //}
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
