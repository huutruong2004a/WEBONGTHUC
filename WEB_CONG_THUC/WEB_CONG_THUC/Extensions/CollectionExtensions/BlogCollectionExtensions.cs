using WEB_CONG_THUC.Repositories;

namespace WEB_CONG_THUC.Extensions.CollectionExtensions
{
    public static class BlogCollectionExtensions
    {
        public static IServiceCollection BlogServices(this IServiceCollection services)
        {
            services.AddScoped<IBlogRepository, BlogRepository>();

            return services;
        }
    }
}
