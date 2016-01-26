using Fclp;
using System;
using System.Linq;
using System.Net;
using System.Text;
using TFPT.Engine;

namespace TFPT.UI
{
    public class ConsoleArguments
    {
        public string RepositoryName { get; set; }
        public enum ActionEnum
        {
            MigrateAsIs,
            CloneAllRepositories,
            CreateRepositoryInProject
        }

        public ActionEnum Action { get; set; }

        public string OriginCollection { get; set; }
        public string DestinationCollection { get; set; }

        public string OriginProject { get; set; }
        public string DestinationProject { get; set; }

        public string OriginCredentials { get; set; }
        public string DestinationCredentials { get; set; }

        public string BasePath { get; set; }
    }

    public abstract class Commands
    {
        protected abstract FluentCommandLineBuilder<ConsoleArguments> Args { get; }

        public abstract void Process();

        string[] _args;
        public Commands(string[] args)
        {
            _args = args;
        }



        protected ConsoleArguments ProcessCommandArgs()
        {
            var args = Args;
            var result = args.Parse(_args);

            if (result.HasErrors)
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("Usage:\n\n");

                foreach (var r in result.UnMatchedOptions)
                {
                    sb.AppendLine(r.Description);
                }

                throw new ArgumentException(sb.ToString());
            }

            return args.Object;
        }

        protected static NetworkCredential GetCredentials(string credentials)
        {
            try
            {
                var split = credentials.Split(';');
                var domain = split[0].Split('\\')[0];
                var user = split[0].Split('\\')[1];
                var password = split[1];

                return new NetworkCredential(user, password, domain);
            }
            catch
            {
                throw new ApplicationException(@"Parameter credential must be in format <domain>\<userName>;password");
            }
        }

    }

    public class MigrateAsIsCommand : Commands
    {
        public MigrateAsIsCommand(string[] args) : base(args) { }

        public override void Process()
        {
            var consoleArgs = base.ProcessCommandArgs();

            var origin = new Git(consoleArgs.OriginCollection, GetCredentials(consoleArgs.OriginCredentials));
            var destination = new Git(consoleArgs.DestinationCollection, GetCredentials(consoleArgs.DestinationCredentials ?? consoleArgs.OriginCredentials));

            origin.RepositoryApi.MigrateTo(consoleArgs.OriginProject, destination, consoleArgs.DestinationProject, consoleArgs.BasePath);            
        }      
  
        protected override FluentCommandLineBuilder<ConsoleArguments> Args
        {
            get
            {
                var fluentCommandLine = new FluentCommandLineBuilder<ConsoleArguments>();

                fluentCommandLine.Setup(a => a.OriginCollection)
                    .As("c1")
                    .WithDescription("/c1: Collection URI of base project. IE: http://localhost:8080/tfs/DefaultCollection")
                    .Required();

                fluentCommandLine.Setup(a => a.DestinationCollection)
                    .As("c2")
                    .WithDescription("/c2: Collection URI of destination project. IE: http://localhost:8080/tfs/anotherCollection")
                    .Required();

                fluentCommandLine.Setup(a => a.OriginProject)
                    .As("p1")
                    .WithDescription("/p1: Project Name of origin project. IE: GitProject")
                    .Required();

                fluentCommandLine.Setup(a => a.DestinationProject)
                    .As("p2")
                    .WithDescription("/p2: Project Name of destination project. IE: GitProject")
                    .Required();

                fluentCommandLine.Setup(a => a.OriginCredentials)
                    .As("u1")
                    .WithDescription(@"/u1: Credentials used in origin collection. Format: <domain>\<userName>;password")
                    .Required();

                fluentCommandLine.Setup(a => a.DestinationCredentials)
                    .As("u2")
                    .WithDescription(@"/u2: Credentials used in origin collection. Format: <domain>\<userName>;password. If not informed, the credentials provided in 'u1' param will be used");

                fluentCommandLine.Setup(a => a.BasePath)
                    .As("path")
                    .WithDescription(@"/path: Base path where 'git clone' will occur (must exists in machine)")
                    .Required();

                return fluentCommandLine;
            }
        }
    }

    public class CloneAllRepositoriesCommand : Commands
    {
        public CloneAllRepositoriesCommand(string[] args): base(args) { }

        public override void Process()
        {
            var consoleArgs = base.ProcessCommandArgs();

            var origin = new Git(consoleArgs.OriginCollection, GetCredentials(consoleArgs.OriginCredentials));
            origin.RepositoryApi.CloneAllRepositories(consoleArgs.OriginProject, consoleArgs.BasePath);
        }      

        
        protected override FluentCommandLineBuilder<ConsoleArguments> Args
        {
            get
            {
                var fluentCommandLine = new FluentCommandLineBuilder<ConsoleArguments>();

                fluentCommandLine.Setup(a => a.OriginCollection)
                    .As("c1")
                    .WithDescription("/c1: Collection URI of base project. IE: http://localhost:8080/tfs/DefaultCollection")
                    .Required();

                fluentCommandLine.Setup(a => a.OriginProject)
                    .As("p1")
                    .WithDescription("/p1: Project Name of origin project. IE: GitProject")
                    .Required();

                fluentCommandLine.Setup(a => a.OriginCredentials)
                    .As("u1")
                    .WithDescription(@"/u1: Credentials used in origin collection. Format: <domain>\<userName>;password")
                    .Required();

                fluentCommandLine.Setup(a => a.BasePath)
                    .As("path")
                    .WithDescription(@"/path: Base path where 'git clone' will occur (must exists in machine)")
                    .Required();

                return fluentCommandLine;
            }
        }
    }

    public class CreateRepositoryInProjectCommand : Commands
    {
        public CreateRepositoryInProjectCommand(string[] args) : base(args) { }

        public string RemoteUrl { get; private set; }

        public override void Process()
        {
            var consoleArgs = base.ProcessCommandArgs();

            var origin = new Git(consoleArgs.OriginCollection, GetCredentials(consoleArgs.OriginCredentials));
            RemoteUrl = origin.RepositoryApi.CreateRepositoryInProject(consoleArgs.OriginProject, consoleArgs.RepositoryName).Result;
        }      

        protected override FluentCommandLineBuilder<ConsoleArguments> Args
        {
            get
            {
                var fluentCommandLine = new FluentCommandLineBuilder<ConsoleArguments>();

                fluentCommandLine.Setup(a => a.OriginCollection)
                    .As('c')
                    .WithDescription("/c: Collection URI of project. IE: http://localhost:8080/tfs/DefaultCollection")
                    .Required();

                fluentCommandLine.Setup(a => a.OriginCredentials)
                    .As('u')
                    .WithDescription(@"/u: Credentials used in collection. Format: <domain>\<userName>;password")
                    .Required();

                fluentCommandLine.Setup(a => a.RepositoryName)
                    .As('p')
                    .WithDescription(@"/p: Project Name")
                    .Required();


                fluentCommandLine.Setup(a => a.RepositoryName)
                    .As('r')
                    .WithDescription(@"/r: Name of new repository")
                    .Required();

                return fluentCommandLine;
            }
        }
    }
}
