using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Timesheets
{
    public class ProjectClient : IProjectClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<ProjectClient> _logger;
        private readonly IDictionary<long, ProjectInfo> _projectCache = new Dictionary<long, ProjectInfo>();
        private readonly Func<Task<string>> _accessTokenFn;
        public ProjectClient(HttpClient client,ILogger<ProjectClient> logger, Func<Task<string>> accessTokenFn)
        {
            _client = client;
            _logger = logger;
            _accessTokenFn = accessTokenFn;
        }

        public async Task<ProjectInfo> Get(long projectId)  => await new GetProjectCommand(DoGet,DoGetFromCache,projectId).ExecuteAsync();
        

         public async Task<ProjectInfo> DoGet(long projectId)
         {

              var token = await _accessTokenFn();

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
             _client.DefaultRequestHeaders.Accept.Clear();
            var streamTask = _client.GetStreamAsync($"project?projectId={projectId}");
            _logger.LogInformation($"Attempting to fetch projectId: {projectId}");
            var serializer = new DataContractJsonSerializer(typeof(ProjectInfo));
            var project = serializer.ReadObject(await streamTask) as ProjectInfo;
            _projectCache.Add(projectId, project);
              _logger.LogInformation($"Caching projectId: {projectId}");
              return project;

         }

         public  Task<ProjectInfo> DoGetFromCache(long projectId)
         {
             _logger.LogInformation($"Retriving from Caching projectId: {projectId}");
             return Task.FromResult(_projectCache[projectId]);
         }
    }
}