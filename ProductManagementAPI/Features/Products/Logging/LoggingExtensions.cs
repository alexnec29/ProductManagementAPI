using Microsoft.Extensions.Logging;

namespace ProductManagementAPI.Features.Products.Logging;

public static class LoggingExtensions
{
    public static void LogProductCreationMetrics(
        this ILogger logger,
        ProductCreationMetrics metrics)
    {
        logger.LogInformation(
            "OperationId={OperationId} | Product={ProductName} | SKU={SKU} | Category={Category} | " +
            "ValidationMs={ValidationMs} | DatabaseMs={DatabaseMs} | TotalMs={TotalMs} | Success={Success} | Error={ErrorReason}",
            metrics.OperationId,
            metrics.ProductName,
            metrics.SKU,
            metrics.Category,
            metrics.ValidationDuration.TotalMilliseconds,
            metrics.DatabaseSaveDuration.TotalMilliseconds,
            metrics.TotalDuration.TotalMilliseconds,
            metrics.Success,
            metrics.ErrorReason ?? "-"
        );
    }
}