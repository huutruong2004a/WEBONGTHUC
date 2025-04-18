using WEB_CONG_THUC.Repositories.Implementations.CategoryRepository;
using WEB_CONG_THUC.Repositories.Interfaces.ICategoryRepository;

namespace WEB_CONG_THUC.Extensions.CollectionExtensions
{
    public static class CategoryCollectionExtensions
    {
        public static IServiceCollection CategoryServices(this IServiceCollection services)
        {
            services.AddScoped<ICategoryGetAllAsync, CategoryGetAllAsync>();
            services.AddScoped<ICategoryAddAsync, CategoryAddAsync>();
            services.AddScoped<ICategoryGetBySlugAsync, CategoryGetBySlugAsync>();
            services.AddScoped<ICategoryDeleteAsync, CategoryDeleteAsync>(); 
            services.AddScoped<ICategoryUpdateAsync, CategoryUpdateAsync>();

            return services;
        }
    }
}
