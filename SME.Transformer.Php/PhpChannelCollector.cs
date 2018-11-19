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
        public List<Channel> InputChannels = new List<Channel>();
        public List<Channel> OutputChannels = new List<Channel>();
        public List<Channel> SanitizeChannels = new List<Channel>();

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
                InputChannels.Add(channel);
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
                OutputChannels.Add(channel);
                _uniqueId++;
            }

            var sanitizeLabel = FindSanitizeLabel(node);
            if (sanitizeLabel != null)
            {
                var channel = new Channel() { Id = _uniqueId, Label = sanitizeLabel, Location = new PhpSourceLocation(node.Span) };
                SanitizeChannels.Add(channel);
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
                return _policy.Input.FirstOrDefault(channel => channel.Name.Equals(varUse.VarName.Value));
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
            return _policy.Output.FirstOrDefault(channel => channel.Name.Equals(functionName));
        }

        /// <summary>
        /// Check if the AST node is a sanitize channel according to the active policy.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private ChannelLabel FindSanitizeLabel(DirectFcnCall node)
        {
            var functionName = node.FullName.Name.QualifiedName.Name.Value;
            return _policy.Sanitize.FirstOrDefault(channel => channel.Name.Equals(functionName));
        }


        public List<SecurityLevel> GetDistinctSecurityLevels()
        {
            var distinctLevels = new List<int>();
            foreach (var inputChannel in InputChannels)
            {
                if (!distinctLevels.Contains(inputChannel.Label.Level))
                {
                    distinctLevels.Add(inputChannel.Label.Level);
                }
            }
            foreach (var outputChannel in OutputChannels)
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
