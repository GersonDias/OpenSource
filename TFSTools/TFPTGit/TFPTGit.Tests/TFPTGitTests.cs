using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Linq;
using TFPT.Engine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace TFPT.Tests
{
    /// <summary>
    /// Testes integrados com o TFS. Exigem dois projetos em GIT, um de origem e outro de destino.
    /// </summary>
    [TestClass]
    public class TFPTGitTests
    {
        private string _collectionURI;
        private Git _tfpt;

        private const string originCollectionUrl = "http://localhost:8080/tfs/DefaultCollection";
        private const string destinyCollectionUrl = "http://localhost:8080/tfs/TesteCollection";
        private const string originProject = "GitProject";
        private const string destinyProject = "NewGitProject";

        private NetworkCredential DefaultCredentials
        {
            get
            {
                return new NetworkCredential("gerson.dias", "lambda123", "lambda3");
            }
        }

        [TestInitialize]
        public void Setup()
        {
            _collectionURI = originCollectionUrl;
            var credentials = DefaultCredentials;

            _tfpt = new Git(_collectionURI, credentials);
        }

        [TestMethod]
        public void Consigo_Montar_a_URL_Das_APIs_Git()
        {
            Assert.AreEqual("http://localhost:8080/tfs/DefaultCollection/_apis/git/repositories?api-version=1.0-preview.1", _tfpt.RepositoryApi.Uri.ToString());
        }

        [TestMethod]
        public void Consigo_Fazer_O_Get_Dos_Repositorios_De_Um_Projeto()
        {
            var response = _tfpt.RepositoryApi.GetAllRepositoriesOfProject(originProject);

            Assert.IsNotNull(response);

            Assert.IsTrue(response.Result.Count > 0);
            Assert.AreEqual(originProject, response.Result.Repositories.First().ProjectName);
        }

        [TestMethod]
        public void Consigo_Fazer_O_Get_Do_Repositorio_De_Um_Projeto_Que_Nao_Tem_Nenhum_Repositorio()
        {
            var response = new Git(destinyCollectionUrl, DefaultCredentials)
                .RepositoryApi.GetAllRepositoriesOfProject("NewGitProject");

            Assert.IsNotNull(response.Result);

            var projectId = response.Result.Repositories.Select(x => x.ProjectID);
            Assert.IsNotNull(projectId);
            //Assert.IsFalse(string.IsNullOrEmpty(response.Result));
        }

        [TestMethod]
        public void Consigo_Encontrar_Um_Repositorio_Conhecido_Do_Projeto()
        {
            var response = _tfpt.RepositoryApi.GetAllRepositoriesOfProject("GitProjectBTG");

            var repo = response.Result.Repositories.FirstOrDefault(x => x.RepositoryName == ("NovoRepo2"));

            Assert.IsNotNull(repo);
            Assert.AreEqual("NovoRepo2", repo.RepositoryName);
        }

        [TestMethod]
        public void Consigo_Criar_Um_Novo_Repositorio()
        {
            var repoName = "NovoRepo5";

            var responseCreate = _tfpt.RepositoryApi.CreateRepositoryInProject(originProject, repoName);

            Assert.IsNotNull(responseCreate);

            var responseCheck = _tfpt.RepositoryApi.GetAllRepositoriesOfProject(originProject);
            var repo = responseCheck.Result.Repositories.FirstOrDefault(x => x.RepositoryName == repoName);

            Assert.IsNotNull(repo);
        }

        [TestMethod]
        public void Dado_Um_Projeto_Consigo_Recriar_Todo_A_Estrutura_De_Branchs_Em_Outro()
        {
            var credentialsOrigem = DefaultCredentials;
            var projetoOrigem = new Git(originCollectionUrl, credentialsOrigem);

            var credentialsDestino = credentialsOrigem;
            var projetoDestino = new Git(destinyCollectionUrl, credentialsDestino);

            var repositoriosOrigem = projetoOrigem.RepositoryApi.GetAllRepositoriesOfProject(originProject);

            var newRepositories = new List<string>();

            Parallel.ForEach(repositoriosOrigem.Result.Repositories, repo =>
            {
                var newRepoUrl = projetoDestino.RepositoryApi.CreateRepositoryInProject(destinyProject, repo.RepositoryName);
                Console.WriteLine(newRepoUrl.Result);
                newRepositories.Add(newRepoUrl.Result);
            });

            Assert.AreEqual(repositoriosOrigem.Result.Count, newRepositories.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void A_Migracao_Nao_Acontece_Se_Informar_Um_Diretorio_Inexistente()
        {
            var origin = new Git(originCollectionUrl, DefaultCredentials);
            var destiny = new Git(destinyCollectionUrl, DefaultCredentials);

            origin.RepositoryApi.MigrateTo(originProject, destiny, destinyProject, @"c:\NonExistingDirectory");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void A_Migracao_Podera_Ocorrer_Somente_Em_Uma_Pasta_Sem_Subpastas()
        {
            //arrange
            var origin = new Git(originCollectionUrl, DefaultCredentials);
            var destiny = new Git(destinyCollectionUrl, DefaultCredentials);

            origin.RepositoryApi.MigrateTo(originProject, destiny, destinyProject, @"c:\windows");
        }

        [TestMethod]
        public void A_Migracao_Ocorre_Com_Sucesso()
        {
            var path = @"c:\migracaoGit123";

            if (System.IO.Directory.Exists(path))
            {
                System.IO.Directory.Delete(path, true);
            }

            System.IO.Directory.CreateDirectory(path);

            var origin = new Git(originCollectionUrl, DefaultCredentials);
            var destiny = new Git(destinyCollectionUrl, DefaultCredentials);

            origin.RepositoryApi.MigrateTo(originProject, destiny, destinyProject, path);
        }

        [TestMethod]
        public void WorkingDirectory_Pode_Conter_Espacos()
        {
            var str = @"C:\migracaogitbase\testes testse";

            var str2 = @"C:\migracaogitbase\testes testse\a.docx";

            var str3 = Path.GetDirectoryName(str2);

            Directory.SetCurrentDirectory(str);
        }
    }
}
