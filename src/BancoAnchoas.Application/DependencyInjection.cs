using BancoAnchoas.Application.Common.Behaviors;
using BancoAnchoas.Application.Common.Interfaces;
using BancoAnchoas.Application.Common.Mappings;
using BancoAnchoas.Application.Common.Services;
using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BancoAnchoas.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        var config = MapsterConfig.Configure();
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        services.AddValidatorsFromAssembly(assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        // Report generators (Strategy pattern)
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        services.AddSingleton<IReportGenerator, CsvReportGenerator>();
        services.AddSingleton<IReportGenerator, ExcelReportGenerator>();
        services.AddSingleton<IReportGenerator, PdfReportGenerator>();
        services.AddSingleton<IReportGeneratorFactory, ReportGeneratorFactory>();

        return services;
    }
}
