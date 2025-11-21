namespace ProductManagementAPI.Common.Logging;

public static class LoggingExtensions
{
    public static void LogProductCreationMetrics(
        this ILogger logger,
        LoggingModels metrics)
    {
        logger.LogInformation(
            "OperationId={OperationId} | Product={ProductName} | SKU={SKU} | Category={Category} | " +
            "ValidationMs={ValidationMs} | DatabaseMs={DatabaseMs} | TotalMs={TotalMs} | Success={Success} | Error={ErrorReason}",
            metrics.OperationId,
            metrics.ProductName,
            metrics.Sku,
            metrics.Category,
            metrics.ValidationDuration.TotalMilliseconds,
            metrics.DatabaseSaveDuration.TotalMilliseconds,
            metrics.TotalDuration.TotalMilliseconds,
            metrics.Success,
            metrics.ErrorReason ?? "-"
        );
    }
}