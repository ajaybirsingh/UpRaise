using UpRaise.Entities;
using UpRaise.Helpers;
using AutoMapper;
using Azure.Search;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure;

namespace UpRaise.Services
{
    public class SearchService
    {
        public readonly static string _campaignIndexName = "campaign";

        private readonly ILogger<SearchService> _logger = null;
        private readonly IConfiguration _configuration;
        private readonly Azure.Search.Documents.Indexes.SearchIndexClient _searchIndexClient = null;
        private readonly Azure.Search.Documents.SearchClient _searchClientCampaign = null;
        private readonly IMapper _mapper = null;

        public SearchService(ILogger<SearchService> logger, IConfiguration configuration, IMapper mapper)
        {
            _logger = logger;
            _configuration = configuration;
            _mapper = mapper;

            var adminApiKey = "dsdfdsfeew$@#@#!#@";// _configuration["Search:PrimaryAdminKey"];
            _searchIndexClient = new Azure.Search.Documents.Indexes.SearchIndexClient(new Uri("https://upraise.search.windows.net"), new AzureKeyCredential(adminApiKey));

            _searchClientCampaign = new SearchClient(new Uri("https://upraise.search.windows.net"), _campaignIndexName, new AzureKeyCredential(adminApiKey));
        }

        public async Task<bool> RecreateIndexAsync(string indexName, Type model)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    var searchIndex = new SearchIndex(indexName);
                    var fieldBuilder = new Azure.Search.Documents.Indexes.FieldBuilder();
                    searchIndex.Fields = fieldBuilder.Build(model);
                    try
                    {
                        await _searchIndexClient.DeleteIndexAsync(searchIndex);
                    }
                    catch (Exception e)
                    {
                        wrappedLogger.LogError(e);
                    }


                    await _searchIndexClient.CreateOrUpdateIndexAsync(searchIndex);

                    return true;
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return false;
            }
        }

        public async Task<bool> UpsertCampaignAsync(Campaign campaign)
        {
            //await RecreateIndexes(); //only for demo to create index

            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    var searchCampaign = _mapper.Map<UpRaise.Models.Search.Campaign>(campaign);

                    var batch = IndexDocumentsBatch.Create(
                        IndexDocumentsAction.Upload(searchCampaign));

                    var options = new IndexDocumentsOptions { ThrowOnAnyError = true };
                    
                    var documentResultIndex = await Policy
                           .Handle<RequestFailedException>()
                           .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, span) =>
                           {
                               //batch = ((RequestFailedException)ex).FindFailedActionsToRetry(batch, x => x.Id);
                               //wrappedLogger.LogWarning($"Failed to index some of the documents: {String.Join(", ", e.IndexingResults.Where(r => !r.Succeeded).Select(r => r.Key))}");
                           })
                           .ExecuteAsync(async () => await _searchClientCampaign.IndexDocumentsAsync(batch, options));
                    
                    var errors = documentResultIndex.Value.Results.Any(i => !i.Succeeded);

                    return !errors;
                }
                catch (Exception ex)
                {
                    // Sometimes when your Search service is under load, indexing will fail for some of the documents in
                    // the batch. Depending on your application, you can take compensating actions like delaying and
                    // retrying. For this simple demo, we just log the failed document keys and continue.
                    wrappedLogger.LogError(ex);
                }

                return false;
            }

        }

    }

}

