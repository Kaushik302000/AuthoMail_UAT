using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HealthChecks.SqlServer;
using HealthChecks.System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace AM.HealthAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddControllers();
            ConfigureHealthChecks(services);
        }

        private void MapHealthCheckEndPoints(IEndpointRouteBuilder endpoints)
        {
            GetEnvironments()
                .ForEach(env =>
                {
                    endpoints.MapHealthChecks($"/{env}", new HealthCheckOptions()
                    {
                        Predicate = (check) => check.Tags.Contains(env)
                    });
                });
        }

        private List<string> GetEnvironments()
        {
            var environments = new List<string>();
            Configuration.GetSection("environments").Bind(environments);
            return environments;
        }

        private void ConfigureHealthChecks(IServiceCollection services)
        {
            GetEnvironments()
                .ForEach(env =>
                {
                    var config = new Config();
                    Configuration.GetSection(env).Bind(config);

                    services.AddHealthChecks()
                                    //.AddSqlServer(config.ConnectionString, name: $"{env}_SqlServer", tags: new[] { env })
                                    .AddDiskStorageHealthCheck(s => s.AddDrive(config.APIDownloadPath, config.MinFreeGB * 1024), name: $"{env}_DiskStorage", tags: new[] { env })
                                    .AddTypeActivatedCheck<AMRootAccessCheck>($"{env}_AMRootAccessCheck", HealthStatus.Unhealthy, args: new object[] { config.AMRootFagosFTP }, tags: new[] { env })
                                    .AddTypeActivatedCheck<AMTokenEndpointCheck>($"{env}_AMTokenEndpointCheck", HealthStatus.Unhealthy, args: new object[] { config.Url }, tags: new[] { env });
                });
        }
        //
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                MapHealthCheckEndPoints(endpoints);
            });
        }
    }

    public class AMRootAccessCheck : IHealthCheck
    {
        public string AMRootFagosFTP { get; set; }
        public AMRootAccessCheck(string rootPath)
        {
            AMRootFagosFTP = rootPath;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                string textWritten = "Jai Swaminarayan";

                string tempFilePath = Path.Combine(AMRootFagosFTP, Path.GetRandomFileName());
                //Write Access
                await File.WriteAllTextAsync(tempFilePath, textWritten);

                //Read Access
                var text = await File.ReadAllTextAsync(tempFilePath);

                if (File.Exists(tempFilePath)) File.Delete(tempFilePath);

            }
            catch (Exception ex)
            {
                return new HealthCheckResult(status: context.Registration.FailureStatus, exception: ex);
            }
            return HealthCheckResult.Healthy();
        }
    }

    public class AMTokenEndpointCheck : IHealthCheck
    {
        public string Url { get; set; }

        public AMTokenEndpointCheck(string url)
        {
            Url = url;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                //Token Access 
                using (HttpClient client = new HttpClient())
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    //setup login data
                    var formContent = new System.Net.Http.FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("grant_type", "password"),
                        new KeyValuePair<string, string>("username", "swami000"),
                        new KeyValuePair<string, string>("password", "swami000")
                    });

                    //send request
                    HttpResponseMessage responseMessage = await client.PostAsync(Url, formContent);
                    var responseJson = await responseMessage.Content.ReadAsStringAsync();
                    var jObject = JObject.Parse(responseJson);

                    if (responseMessage.StatusCode == HttpStatusCode.BadRequest && jObject.GetValue("error_description").ToString() == "The user name or password is incorrect.")
                        return HealthCheckResult.Healthy();
                    else
                        return HealthCheckResult.Unhealthy();
                }
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(status: context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
