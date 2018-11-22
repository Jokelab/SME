using Devsense.PHP.Syntax;
using Devsense.PHP.Syntax.Ast;
using Devsense.PHP.Text;
using SME.Shared;
using SME.Shared.Constants;
using System.Collections.Generic;
using System.Linq;

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
        private readonly int _minInputLevel;
        private readonly bool _isOriginalProgram;

        private List<int> _visitedChannels = new List<int>();
        public PhpChannelRewriter(TreeContext treeContext, ITokenComposer tokenComposer, ISourceTokenProvider sourceTokenProvider, BasicNodesFactory fac, IPolicy policy, List<Channel> inputChannels, List<Channel> outputChannels, List<Channel> sanitizeChannels, SecurityLevel level, bool isOriginalProgram = false)
            : base(treeContext, tokenComposer, sourceTokenProvider)
        {
            _factory = fac;
            _policy = policy;
            _inputChannels = inputChannels;
            _minInputLevel = _inputChannels.Min(ic => ic.Label.Level); //determine lowest ordinal value for all input channels
            _outputChannels = outputChannels;
            _sanitizeChannels = sanitizeChannels;
            _securityLevel = level;
            _isOriginalProgram = isOriginalProgram;
        }

        public override void VisitItemUse(ItemUse node)
        {
            var inputChannel = FindChannel(node, _inputChannels);
            if (inputChannel != null)
            {
                //keep track of visited channels to prevent infinite recursion
                if (!_visitedChannels.Contains(inputChannel.Id))
                {
                    _visitedChannels.Add(inputChannel.Id);
                    RewriteInputChannel(inputChannel, node);
                }
                else
                {
                    base.VisitItemUse(node);
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
                _visitedChannels.Add(outputChannel.Id);
            }
            else if (sanitizeChannel != null)
            {
                //keep track of visited channels to prevent infinite recursion
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

        #region "Rewrite methods"
        private void RewriteInputChannel(Channel inputChannel, ItemUse node)
        {
            //only keep the input channel if it's security label >= current level
            if (inputChannel.Label.Level >= _securityLevel.Level)
            {

                bool isSanitizeTransformation = _securityLevel.Level < _minInputLevel; //it is the sanitize transformation
                bool sanitizeChannelsAvailable = _sanitizeChannels.Any();

                bool doInput = _isOriginalProgram || isSanitizeTransformation || (inputChannel.Label.Level == _securityLevel.Level && !sanitizeChannelsAvailable);
                var function = doInput ? FunctionNames.StoreInput : FunctionNames.ReadInput;


                //construct a new call to the capture_output function
                var name = new TranslatedQualifiedName(new QualifiedName(new Name(function)), new Span());
                var parameters = new List<ActualParam>();
                parameters.Add(new ActualParam(new Span(), new LongIntLiteral(new Span(), inputChannel.Id)));
                parameters.Add(new ActualParam(new Span(), node));

                var signature = new CallSignature(parameters, new Span());
                //let factory create a new DirectFcnCall AST node.
                var storeInput = (DirectFcnCall)_factory.Call(new Span(), name, signature, node.IsMemberOf);

                //visit the new call
                base.VisitDirectFcnCall(storeInput);

            }
            else
            {
                //insert default value
                var defaultValue = CreateDefaultValue();
                base.VisitElement(defaultValue);
            }
        }

        private void RewriteOutputChannel(Channel outputChannel, DirectFcnCall node)
        {
            //the original program (PO') captures all output values
            if (_isOriginalProgram || outputChannel.Label.Level == _securityLevel.Level)
            {
                //construct a new call to the capture output function
                var name = new TranslatedQualifiedName(new QualifiedName(new Name(FunctionNames.StoreOutput)), new Span());
                var parameters = new List<ActualParam>();
                parameters.Add(new ActualParam(new Span(), new LongIntLiteral(new Span(), outputChannel.Id)));
                if (node.CallSignature.Parameters.Length > 0)
                {
                    parameters.Add(node.CallSignature.Parameters[0]);
                }

                var signature = new CallSignature(parameters, new Span());
                //let factory create a new DirectFcnCall AST node.
                var storeOutputCall = (DirectFcnCall)_factory.Call(new Span(), name, signature, node.IsMemberOf);

                //visit the new call
                base.VisitDirectFcnCall(storeOutputCall);
            }

            //performing an output to an output channel is only allowed if the current execution has the same security level
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
            //Store condition: From(sc) >= l
            var storeSanitizedValue = sanitizeChannel.Label.Level >= _securityLevel.Level;

            //Read sanitized value condition: To(sc) >= l ^ l >= min(C)
            var readSanitizedValue = sanitizeChannel.Label.TargetLevel >= _securityLevel.Level && _securityLevel.Level >= _minInputLevel;

            if (storeSanitizedValue || readSanitizedValue)
            {
                //determine if it should read or store a sanitized value
                var function = readSanitizedValue && !_isOriginalProgram ? FunctionNames.ReadSanitize : FunctionNames.StoreSanitize;

                //construct a new call
                var name = new TranslatedQualifiedName(new QualifiedName(new Name(function)), new Span());
                var parameters = new List<ActualParam>();
                parameters.Add(new ActualParam(new Span(), new LongIntLiteral(new Span(), sanitizeChannel.Id)));
                parameters.Add(new ActualParam(new Span(), node));

                var signature = new CallSignature(parameters, new Span());
                //let factory create a new DirectFcnCall AST node.
                var captureSanitizeCall = (DirectFcnCall)_factory.Call(new Span(), name, signature, node.IsMemberOf);

                //visit the new call
                base.VisitDirectFcnCall(captureSanitizeCall);
            }
            else
            {
                //insert default value
                var defaultValue = CreateDefaultValue();
                base.VisitElement(defaultValue);
            }
        }

        #endregion

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

        private LangElement CreateDefaultValue()
        {
            //create empty string
            return _factory.Literal(new Span(0, 2), "''", "''");
        }



    }
}
