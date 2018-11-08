using System;
using System.Collections.Generic;
using System.Linq;
using Devsense.PHP.Syntax;
using Devsense.PHP.Syntax.Ast;
using SME.Shared;

namespace SME.Transformer.Php
{
    public class PhpChannelCollector : TokenVisitor
    {
        public List<Channel> _inputChannels = new List<Channel>();
        public List<Channel> _outputChannels = new List<Channel>();

        private int _uniqueId { get; set; }

        private readonly IPolicy _policy;
        public PhpChannelCollector(IPolicy policy, TreeContext treeContext, ITokenComposer tokenComposer, ISourceTokenProvider sourceTokenProvider)
            : base(treeContext, tokenComposer, sourceTokenProvider)
        {
            _policy = policy;

        }

        /// <summary>
        /// Visits elements like $_GET["xyz"]. These elements can be input channels if they meet the policy requirements.
        /// </summary>
        /// <param name="node"></param>
        public override void VisitItemUse(ItemUse node)
        {
            var inputLabel = FindInputLabel(node);
            if (inputLabel != null)
            {
                var channel = new Channel() { Id = _uniqueId, Label = inputLabel, Location = new PhpSourceLocation(node.Span) };
                _inputChannels.Add(channel);
                _uniqueId++;
            }
            base.VisitItemUse(node);
        }

        /// <summary>
        /// Visits direct function calls. These elements can be output channels.
        /// </summary>
        /// <param name="node"></param>
        public override void VisitDirectFcnCall(DirectFcnCall node)
        {
            
            var outputLabel = FindOutputLabel(node);
            if (outputLabel != null)
            {
                var channel = new Channel() { Id = _uniqueId, Label = outputLabel, Location = new PhpSourceLocation(node.Span) };
                _outputChannels.Add(channel);
                _uniqueId++;
            }

            base.VisitDirectFcnCall(node);
        }

        /// <summary>
        /// Check if the AST node is an input channel according to the active policy.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private ChannelLabel FindInputLabel(ItemUse node)
        {
            var varUse = node.Array as DirectVarUse;
            var arrayKey = node.Index as StringLiteral;

            if (varUse != null && arrayKey != null)
            {
                return _policy.InputLabels.FirstOrDefault(channel => channel.Name.Equals(varUse.VarName.Value));
            }

            return null;
        }


        /// <summary>
        /// Check if the AST node is an output channel according to the active policy.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private ChannelLabel FindOutputLabel(DirectFcnCall node)
        {
            var functionName = node.FullName.Name.QualifiedName.Name.Value;
            return _policy.OutputLabels.FirstOrDefault(channel => channel.Name.Equals(functionName));

        }

        public List<Channel> GetInputChannels()
        {
            return _inputChannels;
        }

        public List<Channel> GetOutputChannels()
        {
            return _outputChannels;
        }

        public List<SecurityLevel> GetDistinctSecurityLevels()
        {
            var distinctLevels = new List<int>();
            foreach (var inputChannel in _inputChannels)
            {
                if (!distinctLevels.Contains(inputChannel.Label.Level))
                {
                    distinctLevels.Add(inputChannel.Label.Level);
                }
            }
            foreach (var outputChannel in _outputChannels)
            {
                if (!distinctLevels.Contains(outputChannel.Label.Level))
                {
                    distinctLevels.Add(outputChannel.Label.Level);
                }
            }

            return _policy.Levels.Where(l => distinctLevels.Contains(l.Level)).ToList();

        }

        

    }
}
