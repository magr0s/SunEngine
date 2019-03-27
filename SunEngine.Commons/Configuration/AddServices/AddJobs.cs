using Microsoft.Extensions.DependencyInjection;
using SunEngine.Commons.Scheduler;

namespace SunEngine.Commons.Configuration.AddServices
{
    public static class AddJobsExtensions
    {
        public static void AddJobs(this IServiceCollection services)
        {
            services.AddHostedService<CleanCacheJobsService>();
        }
    }
}