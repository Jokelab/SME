using Devsense.PHP.Syntax;
using Devsense.PHP.Syntax.Ast;
using Devsense.PHP.Syntax.Visitor;
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

        public override void VisitEchoStmt(EchoStmt node)
        {

            var outputChannel = FindChannel(node, _outputChannels);
            if (outputChannel != null)
            {
                RewriteEchoStmt(outputChannel, node);
                _visitedChannels.Add(outputChannel.Id);
            }
            else
            {
                //no special treatment
                base.VisitEchoStmt(node);
            }
        }
        public override void VisitEvalEx(EvalEx node)
        {
            var outputChannel = FindChannel(node, _outputChannels);
            if (outputChannel != null)
            {
                RewriteEvalEx(outputChannel, node);
                _visitedChannels.Add(outputChannel.Id);
            }
            else
            {
                //no special treatment
                base.VisitEvalEx(node);
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
                var function = doInput ? FunctionNames.StoreInput : FunctionNames.GetInput;


                //construct a new call to the store/get input function
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
            if (_isOriginalProgram || outputChannel.Label.Level == _securityLevel.Level || _securityLevel.Level < _minInputLevel)
            {
                var functionName = _securityLevel.Level < _minInputLevel ? FunctionNames.CaptureOutput : FunctionNames.StoreOutput;
                //construct a new call to the capture output function
                var name = new TranslatedQualifiedName(new QualifiedName(new Name(functionName)), new Span());
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
            if (_isOriginalProgram || outputChannel.Label.Level == _securityLevel.Level)
            {
                //add a semicolon between the new call and the original call
                base.VisitEmptyStmt((EmptyStmt)_factory.EmptyStmt(new Span(0, 1)));


                if (!_isOriginalProgram)
                {
                    //construct a new call to the get output function
                    var name = new TranslatedQualifiedName(new QualifiedName(new Name(FunctionNames.GetOutput)), new Span());
                    var parameters = new List<ActualParam>
                    {
                        new ActualParam(new Span(), new LongIntLiteral(new Span(), outputChannel.Id))
                    };

                    var signature = new CallSignature(parameters, new Span());
                    //let factory create a new DirectFcnCall AST node.
                    var readOutputCall = (DirectFcnCall)_factory.Call(new Span(), name, signature, node.IsMemberOf);

                    //replace parameter with a read_output call
                    node.CallSignature.Parameters[0] = new ActualParam(new Span(), readOutputCall);


                    //visit the original call
                    base.VisitDirectFcnCall(node);
                }
            }
        }


        private void RewriteEchoStmt(Channel outputChannel, EchoStmt node)
        {
            //the original program (PO') captures all output values
            if (_isOriginalProgram || outputChannel.Label.Level == _securityLevel.Level || _securityLevel.Level < _minInputLevel)
            {
                var functionName = _securityLevel.Level < _minInputLevel ? FunctionNames.CaptureOutput : FunctionNames.StoreOutput;
                //construct a new call to the capture output function
                var name = new TranslatedQualifiedName(new QualifiedName(new Name(functionName)), new Span());
                var parameters = new List<ActualParam>();
                parameters.Add(new ActualParam(new Span(), new LongIntLiteral(new Span(), outputChannel.Id)));
                if (node.Parameters.Length > 0)
                {
                    parameters.Add(new ActualParam(new Span(), node.Parameters[0]));
                }

                var signature = new CallSignature(parameters, new Span());
                //let factory create a new DirectFcnCall AST node.
                var storeOutputCall = (DirectFcnCall)_factory.Call(new Span(), name, signature, null);

                //visit the new call
                base.VisitDirectFcnCall(storeOutputCall);
            }

            //add a semicolon between the new call and the original call
            base.VisitEmptyStmt((EmptyStmt)_factory.EmptyStmt(new Span(0, 1)));

            //performing an output to an output channel is only allowed if the current execution has the same security level
            if (_isOriginalProgram || outputChannel.Label.Level == _securityLevel.Level)
            {

                if (!_isOriginalProgram)
                {
                    //construct a new call to the get output function
                    var name = new TranslatedQualifiedName(new QualifiedName(new Name(FunctionNames.GetOutput)), new Span());
                    var parameters = new List<ActualParam>
                    {
                        new ActualParam(new Span(), new LongIntLiteral(new Span(), outputChannel.Id))
                    };

                    var signature = new CallSignature(parameters, new Span());
                    //let factory create a new Echo AST node and let it echo the read_output call
                    var readOutputCall = (DirectFcnCall)_factory.Call(new Span(), name, signature, null);
                    var echo = (EchoStmt)_factory.Echo(new Span(), new List<LangElement> { readOutputCall });

                    //visit the original call
                    base.VisitEchoStmt(echo);
                }
            }
        }

        private void RewriteEvalEx(Channel outputChannel, EvalEx node)
        {
            //the original program (PO') captures all output values
            if (_isOriginalProgram || outputChannel.Label.Level == _securityLevel.Level || _securityLevel.Level < _minInputLevel)
            {
                var functionName = _securityLevel.Level < _minInputLevel ? FunctionNames.CaptureOutput : FunctionNames.StoreOutput;
                //construct a new call to the capture output function
                var name = new TranslatedQualifiedName(new QualifiedName(new Name(functionName)), new Span());
                var parameters = new List<ActualParam>();
                parameters.Add(new ActualParam(new Span(), new LongIntLiteral(new Span(), outputChannel.Id)));
                if (node.Code != null)
                {
                    parameters.Add(new ActualParam(new Span(), node.Code));
                }

                var signature = new CallSignature(parameters, new Span());
                //let factory create a new DirectFcnCall AST node.
                var storeOutputCall = (DirectFcnCall)_factory.Call(new Span(), name, signature, null);

                //visit the new call
                base.VisitDirectFcnCall(storeOutputCall);
            }

            //add a semicolon between the new call and the original call
            base.VisitEmptyStmt((EmptyStmt)_factory.EmptyStmt(new Span(0, 1)));

            //performing an output to an output channel is only allowed if the current execution has the same security level
            if (_isOriginalProgram || outputChannel.Label.Level == _securityLevel.Level)
            {

                if (!_isOriginalProgram)
                {
                    //construct a new call to the get output function
                    var name = new TranslatedQualifiedName(new QualifiedName(new Name(FunctionNames.GetOutput)), new Span());
                    var parameters = new List<ActualParam>
                    {
                        new ActualParam(new Span(), new LongIntLiteral(new Span(), outputChannel.Id))
                    };

                    var signature = new CallSignature(parameters, new Span());
                    //let factory create a new Echo AST node and let it echo the read_output call
                    var readOutputCall = (DirectFcnCall)_factory.Call(new Span(), name, signature, null);
                    var evalExpression = (EvalEx)_factory.Eval(new Span(), readOutputCall);

                    //visit the original call
                    base.VisitEvalEx(evalExpression);
                }
            }
        }

        private void RewriteSanitizeChannel(Channel sanitizeChannel, DirectFcnCall node)
        {
            //Store condition: From(sc) >= l
            var storeSanitizedValue = sanitizeChannel.Label.Level >= _securityLevel.Level;

            //Get sanitized value condition: To(sc) >= l ^ l >= min(C)
            var getSanitizedValue = sanitizeChannel.Label.TargetLevel >= _securityLevel.Level && _securityLevel.Level >= _minInputLevel;

            if (storeSanitizedValue || getSanitizedValue)
            {
                //determine if it should read or store a sanitized value
                var function = getSanitizedValue && !_isOriginalProgram ? FunctionNames.GetSanitize : FunctionNames.StoreSanitize;

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
