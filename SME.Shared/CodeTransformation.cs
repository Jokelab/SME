namespace SME.Shared
{
    public class CodeTransformation
    {

        /// <summary>
        /// The modified code for the intended security level
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The security level that corresponds with the transformed node.
        /// </summary>
        public SecurityLevel Level { get; set; }

        public override string ToString()
        {
            return  $"{Level.Name}:\n {Code}";
        }
    }
}
