using Fclp;
using System;
using System.Linq;
using System.Net;
using TFPT.Engine;

namespace TFPT.UI
{
    public class ConsoleUI
    {
        private const string USAGE_ERROR = "Usage: tfpt <action>\n\nAction:\nMigrateAsIs: Migrate all repositories of one git project to another one.\nCloneAllRepositories: Clone all repositories of one git project.\nCreateRepositoryInProject: Create a new repository in one git project";
        static void Main(string[] args)
        {
            try
            {
                if (args.Count() < 1)
                {
                    WriteErrors("Usage: tfpt <action>\n\nAction:\nMigrateAsIs: Migrate all repositories of one git project to another one.\nCloneAllRepositories: Clone all repositories of one git project.\nCreateRepositoryInProject: Create a new repository in one git project");
                    return;
                }


                switch ((ConsoleArguments.ActionEnum)Enum.Parse(typeof(ConsoleArguments.ActionEnum), args[0], true))
                {
                    case ConsoleArguments.ActionEnum.MigrateAsIs:
                        new MigrateAsIsCommand(args).Process();
                        break;
                    case ConsoleArguments.ActionEnum.CloneAllRepositories:
                        new CloneAllRepositoriesCommand(args).Process();
                        break;
                    case ConsoleArguments.ActionEnum.CreateRepositoryInProject:
                        new CreateRepositoryInProjectCommand(args.Skip(1).ToArray()).Process();
                        break;
                    default:
                        WriteErrors(USAGE_ERROR);
                        break;
                }
            }
            catch (Exception err)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(err.Message);
                Console.ForegroundColor = oldColor;
            }
        }

        private static void WriteErrors(Git repository)
        {
            foreach (var erro in repository.RepositoryApi.Errors)
            {
                WriteErrors(erro);
            }
        }

        private static void WriteErrors(string message)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Erros durante o processo:\n");
            Console.WriteLine(message);
            Console.ForegroundColor = color;
        }

    }
}
