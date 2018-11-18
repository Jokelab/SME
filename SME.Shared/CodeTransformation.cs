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
        public SecurityLevel SecurityLevel { get; set; }

        /// <summary>
        /// Indicates if it is the adjusted version of the original code (PO') or not.
        /// </summary>
        public bool IsOriginal { get; set; }


        public override string ToString()
        {
            return  $"{SecurityLevel.Name}:\n {Code}";
        }
    }
}
