namespace SME.Factory
{
    public static class FactoryProducer
    {
        public static SmeFactory GetFactory(string fileExtension)
        {
            switch (fileExtension.ToLower())
            {
                case ".php": return new PhpFactory();
                case ".cs": return new CSharpFactory();
            }

            throw new System.Exception($"Unsupported file extension: {fileExtension}");
            
        }
    }
}
