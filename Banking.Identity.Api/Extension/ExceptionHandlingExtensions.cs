namespace Banking.Identity.Api.Extension
{
    public static class ExceptionHandlingExtensions
    {
        public static IServiceCollection AddGlobalExceptionHandling(this IServiceCollection services)
        {
            services.AddProblemDetails();
            services.AddExceptionHandler<HandlerException>();

            return services;
        }
    }
}
