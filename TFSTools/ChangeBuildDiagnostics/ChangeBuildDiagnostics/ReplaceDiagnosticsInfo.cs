using Microsoft.TeamFoundation.Build.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.FileContainer;
using Microsoft.VisualStudio.Services.FileContainer.Client;
using System;
using System.Activities;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChangeBuildDiagnostics.Activity
{
    [BuildActivity(HostEnvironmentOption.Agent)]
    public class ReplaceDiagnosticsInfo : CodeActivity
    {
        /// <summary>
        /// e.g. {"P2ssw0rd", "*****"}
        /// </summary>
        [RequiredArgument]
        public InArgument<Dictionary<string, string>> FindAndReplaceStrings { get; set; }

        /// <summary>
        /// Get it from BuildDetails
        /// </summary>
        [RequiredArgument]
        public InArgument<Uri> TeamProjectUri { get; set; }

        /// <summary>
        /// Get it from BuildDetails
        /// </summary>
        [RequiredArgument]
        public InArgument<Uri> BuildUri { get; set; }

        private FileContainerHttpClient _fcClient;
        private Uri _teamProjectUri;
        private Uri _buildUri;

        /// <summary>
        /// You need to put this activity in a different agent that write the diagnostics log that you want to change.
        /// </summary>
        /// <param name="context"></param>
        protected override void Execute(CodeActivityContext context)
        {
            Thread.Sleep(30000);

            var findAndReplace = context.GetValue(FindAndReplaceStrings);
            _teamProjectUri = context.GetValue(TeamProjectUri);
            _buildUri = context.GetValue(BuildUri);

            var vssCredential = new VssCredentials(true);
            _fcClient = new FileContainerHttpClient(_teamProjectUri, vssCredential);
            var containers = _fcClient.QueryContainersAsync(new List<Uri>() { _buildUri }).Result;

            if (!containers.Any())
                return;

            var agentLogs = GetAgentLogs(containers);

            if (agentLogs == null)
                return;

            using (var handler = new HttpClientHandler() { UseDefaultCredentials = true })
            {
                var reader = DownloadAgentLog(agentLogs, handler);

                using (var ms = new MemoryStream())
                {
                    ReplaceStrings(findAndReplace, reader, ms);
                    var response = UploadDocument(containers, agentLogs, ms);
                }
            }
        }


        private HttpResponseMessage UploadDocument(IEnumerable<FileContainer> containers, FileContainerItem agentLogs, MemoryStream ms)
        {
            var upl = _fcClient.UploadFileAsync(containers.ElementAt(0).Id, agentLogs.Path, ms, new System.Threading.CancellationToken());
            var uplResponse = upl.Result;

            return uplResponse;
        }

        private static void ReplaceStrings(Dictionary<string, string> findAndReplace, StreamReader reader, MemoryStream ms)
        {
            var writer = new StreamWriter(ms, Encoding.UTF8);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();

                foreach (var str in findAndReplace)
                {
                    line = line.Replace(str.Key, str.Value);
                }

                writer.WriteLine(line);
            }

            writer.Flush();

            ms.Seek(0, SeekOrigin.Begin);
        }

        private static StreamReader DownloadAgentLog(FileContainerItem agentLogs, HttpClientHandler handler)
        {
            var httpClient = new HttpClient(handler);
            var reader = new StreamReader(new GZipStream(httpClient.GetStreamAsync(agentLogs.ContentLocation).Result, CompressionMode.Decompress), Encoding.UTF8);
            return reader;
        }

        private FileContainerItem GetAgentLogs(IEnumerable<FileContainer> containers)
        {
            var contentLocation = containers.ElementAt(0).ContentLocation;
            var files = _fcClient.QueryContainerItemsAsync(containers.ElementAt(0).Id).Result;
            var agentLogs = files.FirstOrDefault(x => x.ContentLocation.Contains("AgentScope"));

            return agentLogs;
        }
    }
}

