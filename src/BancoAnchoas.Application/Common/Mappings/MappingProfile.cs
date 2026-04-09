using BancoAnchoas.Application.Features.Categories.DTOs;
using BancoAnchoas.Application.Features.Notifications.DTOs;
using BancoAnchoas.Application.Features.Products.DTOs;
using BancoAnchoas.Application.Features.Requesters.DTOs;
using BancoAnchoas.Application.Features.Stock.DTOs;
using BancoAnchoas.Application.Features.Warehouses.DTOs;
using BancoAnchoas.Domain.Entities;
using Mapster;

namespace BancoAnchoas.Application.Common.Mappings;

public static class MapsterConfig
{
    public static TypeAdapterConfig Configure()
    {
        var config = TypeAdapterConfig.GlobalSettings;

        config.NewConfig<Product, ProductDto>()
            .Map(d => d.CategoryName, s => s.Category.Name);

        config.NewConfig<Product, ProductListDto>()
            .Map(d => d.CategoryName, s => s.Category.Name);

        config.NewConfig<StockMovement, StockMovementDto>()
            .Map(d => d.ProductName, s => s.Product.Name)
            .Map(d => d.SectorName, s => s.Sector.Name)
            .Map(d => d.FromSectorName, s => s.FromSector != null ? s.FromSector.Name : null)
            .Map(d => d.RequesterName, s => s.Requester != null ? s.Requester.Name : null);

        // Category, Warehouse, Notification → flat DTOs, convention-based
        config.NewConfig<Category, CategoryDto>();
        config.NewConfig<Warehouse, WarehouseDto>();
        config.NewConfig<Notification, NotificationDto>();
        config.NewConfig<Requester, RequesterDto>();

        return config;
    }
}
