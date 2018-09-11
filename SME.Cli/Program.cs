using SME.Factory;
using SME.Shared;
using System.IO;

namespace SME.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            var policy = CreatePolicy();

            var currentPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var fullPath = Path.Combine(currentPath, "Sample.php");
            var content = File.ReadAllText(fullPath);

            var factory = FactoryProducer.GetFactory(Path.GetExtension(fullPath)); 
            var transformer = factory.CreateTransformer();
            var result = transformer.Transform(content, policy);


            var scheduler = factory.CreateScheduler();
            foreach (var version in result.CodeTransformations)
            {
                System.Console.WriteLine("Now executing level " + version.Level.Name + ":");
                scheduler.Run(version.Code, args);
                System.Console.Write("\n\n");
            }
    

            System.Console.ReadKey();
        }

        public static IPolicy CreatePolicy()
        {
            var policy = new Policy();

            policy.Levels.Add(new SecurityLevel() { Name = "L", Level = 1 });
            policy.Levels.Add(new SecurityLevel() { Name = "H", Level = 2 });

            //input channels
            policy.InputLabels.Add(new ChannelLabel() { Name = "_GET", Level = 1 });
            policy.InputLabels.Add(new ChannelLabel() { Name = "_POST", Level = 1 });

            //output channels
            policy.OutputLabels.Add(new ChannelLabel() { Name = "mysql_query", Level = 2 });

            return policy;
        }
    }
}
