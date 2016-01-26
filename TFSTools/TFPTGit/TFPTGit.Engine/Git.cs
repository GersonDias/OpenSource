using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TFPT.Engine
{
    public class Git
    {
        private Uri _collectionURI;
        private ICredentials _credentials;

        public RepositoryActions RepositoryApi
        {
            get
            {
                return new RepositoryActions(_collectionURI, _credentials);
            }
        }

        public Git(string collectionURI, ICredentials credentials)
        {
            _collectionURI = new Uri(collectionURI);
            _credentials = credentials;
        }
    }

    public abstract class GitRestApi
    {
        private const string API_VERSION = "?api-version=1.0-preview.1";
        private const string relativePath = "/_apis/git/(action)";
        private const string relativePathOfProjectAPI = "/_apis/git/(project)/(action)";
        private readonly Uri _collectionUri;

        protected ICredentials Credentials { get; private set; }

        protected abstract string Action { get; }

        public virtual Uri Uri { get { return new Uri((_collectionUri + string.Concat(relativePath, API_VERSION)).Replace("(action)", Action)); } }
        public virtual Uri ProjectUri(string projectName)
        {
            return new Uri((_collectionUri + string.Concat(relativePathOfProjectAPI, API_VERSION)).Replace("(action)", Action).Replace("(project)", projectName));
        }

        protected GitRestApi(Uri collectionUri, ICredentials credentials)
        {
            _collectionUri = collectionUri;
            Credentials = credentials;
        }
    }

    public class RepositoryActions : GitRestApi
    {
        private List<string> _errors = new List<string>();
        public IEnumerable<string> Errors { get { return _errors; } }

        protected override string Action
        {
            get
            {
                return "repositories";
            }
        }

        private HttpWebRequest CreateRequest(string httpVerb, string projectName = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(string.IsNullOrEmpty(projectName) ? Uri : ProjectUri(projectName));
            request.Credentials = Credentials;
            request.Method = httpVerb;
            
            return request;
        }

        public async Task<string> CreateRepositoryInProject(string projectName, string repoName)
        {
            var repositories = GetAllRepositoriesOfProject(projectName).Result.Repositories;
            var projectId = repositories.Select(x => x.ProjectID.ToString()).FirstOrDefault();

            if (projectId != null)
            {
                try
                {
                    var request = CreateRequest("POST");

                    string json = JsonConvert.SerializeObject(new { name = repoName, project = new { id = projectId.ToString() } });
                    var jsonByte = System.Text.Encoding.ASCII.GetBytes(json);
                    request.ContentType = "application/json";
                    request.ContentLength = jsonByte.Length;
                    var postStream = request.GetRequestStream();
                    postStream.Write(jsonByte, 0, jsonByte.Length);
                    postStream.Flush();
                    postStream.Close();

                    var response = await request.GetResponseAsync();

                    string responseText;
                    using (var responseReader = new StreamReader(response.GetResponseStream()))
                    {
                        responseText = await responseReader.ReadToEndAsync();
                    }

                    return JObject.Parse(responseText).Value<string>("remoteUrl");
                }
                catch (WebException e)
                {
                    if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Conflict)
                    {
                        return repositories.FirstOrDefault(x => x.RepositoryName == repoName).RemoteUrl;
                    }       
                }
            }

            return null;
        }

        public RepositoryActions(Uri collectionUri, ICredentials credentials) : base(collectionUri, credentials)
        {
        }

        public async Task<RepositoriesResponse> GetAllRepositoriesOfProject(string projectName)
        {
            var request = CreateRequest("GET", projectName);

            var response = await request.GetResponseAsync();

            string responseText;
            using (var responseReader = new StreamReader(response.GetResponseStream()))
            {
                responseText = await responseReader.ReadToEndAsync();
            }

            return new RepositoriesResponse(responseText);
        }



        public void MigrateTo(string originProject, Git destinyGit, string destinyProject, string cloneBasePath)
        {
            if (!Directory.Exists(cloneBasePath))
            {
                throw new InvalidOperationException("Base Path doesn't exists");
            }

            //if (Directory.GetDirectories(cloneBasePath).Any())
            //{
            //    throw new InvalidOperationException("Inform a base class that doesn't contain any subdirectories");
            //}

            var repositoriosOrigem = this.GetAllRepositoriesOfProject(originProject).Result;

            Parallel.ForEach(repositoriosOrigem.Repositories, new ParallelOptions() { MaxDegreeOfParallelism = 1 }, repo =>// foreach (var repo in repositoriosOrigem.Repositories)
            {
                Console.WriteLine("Criando repositorio " + repo.RepositoryName);
                var newRemoteUri = destinyGit.RepositoryApi.CreateRepositoryInProject(destinyProject, repo.RepositoryName).Result;
                newRemoteUri = newRemoteUri.Replace(" ", "%20");

                var subpath = string.Format("{0}", Path.Combine(cloneBasePath, repo.RepositoryName).Replace(" ", "%20"));

                if (Directory.Exists(subpath))
                {
                    Console.WriteLine("Efetuando o pull no repositorio " + repo.RemoteUrl);
                    ProcessGitCommand("pull ", subpath);
                }
                else
                {
                    Console.WriteLine("Clonando o repositorio " + repo.RemoteUrl);
                    ProcessGitCommand("clone " + repo.RemoteUrl.Replace(" ", "%20"), cloneBasePath);
                }

                Console.WriteLine("Adicionando Remote " + newRemoteUri);
                ProcessGitCommand("remote add newRemote " + newRemoteUri, subpath);

                Console.WriteLine("Efetuando push ");
                ProcessGitCommand("push newRemote", subpath);
            });
        }

        public void CloneAllRepositories(string originProject, string cloneBasePath)
        {
            if (!Directory.Exists(cloneBasePath))
            {
                throw new InvalidOperationException("Base Path doesn't exists");
            }

            var repositoriosOrigem = this.GetAllRepositoriesOfProject(originProject).Result;

            Parallel.ForEach(repositoriosOrigem.Repositories, new ParallelOptions() { MaxDegreeOfParallelism = 1 }, repo =>
            {
                var subpath = string.Format("{0}", Path.Combine(cloneBasePath, repo.RepositoryName).Replace(" ", "%20"));

                if (Directory.Exists(subpath))
                {
                    Console.WriteLine("Efetuando o pull no repositorio " + repo.RemoteUrl);
                    ProcessGitCommand("pull ", subpath);
                }
                else
                {
                    Console.WriteLine("Clonando o repositorio " + repo.RemoteUrl);
                    ProcessGitCommand("clone " + repo.RemoteUrl.Replace(" ", "%20"), cloneBasePath);
                }
            });
        }


        private void ProcessGitCommand(string command, string workingDirectory)
        {
            var gitInfo = new ProcessStartInfo();
            gitInfo.CreateNoWindow = false;
            gitInfo.RedirectStandardError = true;
            gitInfo.RedirectStandardOutput = true;
            gitInfo.FileName = @"C:\Program Files (x86)\Git" + @"\bin\git.exe";
            gitInfo.Arguments = command;
            //gitInfo.WorkingDirectory = workingDirectory.Replace("'", "\"");

            Directory.SetCurrentDirectory(@workingDirectory);


            gitInfo.UseShellExecute = false;

            Process gitProcess = new Process();

            gitProcess.StartInfo = gitInfo;
            gitProcess.OutputDataReceived += gitProcess_OutputDataReceived;
            gitProcess.ErrorDataReceived += gitProcess_ErrorDataReceived;

            try
            {
                gitProcess.Start();
                gitProcess.BeginOutputReadLine();
                gitProcess.BeginErrorReadLine();

                gitProcess.WaitForExit();

                if (gitProcess.ExitCode != 0)
                {
                    //string erro = "";
                    //using (gitProcess.StandardError)
                    //{
                        //var erro = gitProcess.StandardError.ReadToEndAsync();
                        _errors.Add(string.Format("Erro durante a execução do comando em {0}: {1} {2}.", workingDirectory, gitInfo.FileName, gitInfo.Arguments));
                    //}

                    //throw new ApplicationException(string.Format("Erro durante a execução do comando em {0}: {1} {2}. \n\n{3}", workingDirectory, gitInfo.FileName, gitInfo.Arguments, erro));
                }
            }
            finally
            {
                gitProcess.Close();
            }
        }

        void gitProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(e.Data);
            Console.ForegroundColor = color;
        }

        private void gitProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Data);
            Console.ForegroundColor = color;            
        }

        public class RepositoriesResponse
        {
            public int Count { get; private set; }
            public IEnumerable<RepositoryDetails> Repositories { get; private set; }

            public RepositoriesResponse(string jsonResponse)
            {
                var jObject = JObject.Parse(jsonResponse);
                Count = int.Parse(jObject["count"].ToString());

                var repositories = new List<RepositoryDetails>(Count);

                foreach (var detail in jObject.SelectToken("value"))
                {
                    Guid repositoryId = Guid.Parse(detail["id"].Value<string>());
                    string repositoryName = detail["name"].Value<string>();
                    Guid projectId = Guid.Parse(detail["project"]["id"].Value<string>());
                    string projectName = detail["project"]["name"].Value<string>();
                    string remoteUrl = detail["remoteUrl"].Value<string>();

                    repositories.Add(new RepositoryDetails(repositoryId, repositoryName, projectId, projectName, remoteUrl));
                }

                Repositories = repositories;
            }
        }
    }

    public class RepositoryDetails
    {
        public Guid RepositoryID { get; private set; }
        public string RepositoryName { get; private set; }
        public Guid ProjectID { get; private set; }
        public string ProjectName { get; private set; }
        public string RemoteUrl { get; private set; }


        public RepositoryDetails(Guid repositoryId, string repositoryName, Guid projectId, string projectName, string remoteUrl)
        {
            RepositoryID = repositoryId;
            RepositoryName = repositoryName;
            RepositoryID = repositoryId;
            ProjectID = projectId;
            ProjectName = projectName;
            RemoteUrl = remoteUrl;
        }
    }
}

