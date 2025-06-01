using WEB_CONG_THUC.Repositories;

namespace WEB_CONG_THUC.Extensions.CollectionExtensions
{
    public static class VideoCollectionExtensions
    {
        public static IServiceCollection VideoServices(this IServiceCollection services)
        {
            services.AddScoped<IVideoRepository, VideoRepository>();

            return services;
        }
    }
}
