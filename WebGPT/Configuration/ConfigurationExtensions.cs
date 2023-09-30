namespace WebGPT.Configuration
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection ConfigureNamedOptions<T>(
            this IServiceCollection services, IConfiguration configuration,
            string? defaultSection = null)
            where T : class
        {
            foreach (var option in configuration.GetChildren())
            {
                services.Configure<T>(option.Key, option);
            }

            if(defaultSection is not null)
            {
                services.Configure<T>(configuration.GetSection(defaultSection));
            }

            return services;
        }
    }
}
