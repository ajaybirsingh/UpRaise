using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UpRaise.Services.EF;
using EasyCaching.Core;
using UpRaise.DTOs.Redis;

namespace UpRaise.Helpers.Rules
{

    public class VersionMiddleware
    {
        readonly RequestDelegate _next;
        static readonly Assembly _entryAssembly = System.Reflection.Assembly.GetEntryAssembly();
        static readonly string _version = FileVersionInfo.GetVersionInfo(_entryAssembly.Location).FileVersion;

        public VersionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync(_version);

            //we're all done, so don't invoke next middleware
        }
    }


    public class CampaignDetailsRewriteRule : Microsoft.AspNetCore.Rewrite.IRule
    {
        private readonly IEasyCachingProvider _cacheProvider;

        public CampaignDetailsRewriteRule(
            IEasyCachingProvider cacheProvider)
        {
            _cacheProvider = cacheProvider;
        }

        public void ApplyRule(RewriteContext context)
        {
            var request = context.HttpContext.Request;

            if (request.Path.Value.StartsWith("/campaign-", StringComparison.OrdinalIgnoreCase))
            {
                var SEOFriendlyURL = request.Path.Value.Substring(10).ToLower();

                if (string.IsNullOrWhiteSpace(SEOFriendlyURL))
                    request.Path = "/home";
                else
                {
                    var key = $"{Constants.Cache_SEO_Prefix}{SEOFriendlyURL}";
                    var seoCacheData = _cacheProvider.Get<SEOFriendlyURLDTO>(key);

                    if (!seoCacheData.IsNull)
                    {
                        var url = $"/home/detail/{seoCacheData.Value.TypeId}/{seoCacheData.Value.Id}";
                        context.Logger.LogInformation($"Cache Hit -- forwarding to '{url}'");

                        request.Path = url;
                    }
                    else
                    {
                        context.Logger.LogInformation($"Cache Miss");
                        request.Path = "/home";
                    }
                }

                context.Result = RuleResult.SkipRemainingRules;

            }

            context.Logger.LogInformation($"Accessing - {request.Path.Value}");

            /*
            var request = context.HttpContext.Request;
            var host = request.Host;
            if (host.Host.Contains("yourSampleHost",
             StringComparison.OrdinalIgnoreCase))
            {
                if (host.Port == 8080)
                {
                    context.Result = RuleResult.ContinueRules;
                    return;
                }
            }
            var response = context.HttpContext.Response;
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Result = RuleResult.EndResponse;
            */
        }

        /*
         * private readonly string _extension;
    private readonly PathString _newPath;

    public RedirectImageRequests(string extension, string newPath)
    {
        if (string.IsNullOrEmpty(extension))
        {
            throw new ArgumentException(nameof(extension));
        }

        if (!Regex.IsMatch(extension, @"^\.(png|jpg|gif)$"))
        {
            throw new ArgumentException("Invalid extension", nameof(extension));
        }

        if (!Regex.IsMatch(newPath, @"(/[A-Za-z0-9]+)+?"))
        {
            throw new ArgumentException("Invalid path", nameof(newPath));
        }

        _extension = extension;
        _newPath = new PathString(newPath);
    }

    public void ApplyRule(RewriteContext context)
    {
        var request = context.HttpContext.Request;

        // Because we're redirecting back to the same app, stop 
        // processing if the request has already been redirected
        if (request.Path.StartsWithSegments(new PathString(_newPath)))
        {
            return;
        }

        if (request.Path.Value.EndsWith(_extension, StringComparison.OrdinalIgnoreCase))
        {
            var response = context.HttpContext.Response;
            response.StatusCode = (int) HttpStatusCode.MovedPermanently;
            context.Result = RuleResult.EndResponse;
            response.Headers[HeaderNames.Location] = 
                _newPath + request.Path + request.QueryString;
        }
    }
        */
    }

}
