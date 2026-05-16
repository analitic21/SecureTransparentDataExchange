BEGIN TRANSACTION;

IF COL_LENGTH('dbo.Shipment', 'ClientId') IS NULL
BEGIN
    ALTER TABLE dbo.Shipment ADD ClientId INT NULL;
END;

IF COL_LENGTH('dbo.Shipment', 'OrderId') IS NULL
BEGIN
    ALTER TABLE dbo.Shipment ADD OrderId INT NULL;
END;

IF COL_LENGTH('dbo.Shipment', 'DeliveryRouteId') IS NULL
BEGIN
    ALTER TABLE dbo.Shipment ADD DeliveryRouteId INT NULL;
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Shipment_ClientId'
      AND object_id = OBJECT_ID('dbo.Shipment')
)
BEGIN
    CREATE INDEX IX_Shipment_ClientId ON dbo.Shipment(ClientId);
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Shipment_OrderId'
      AND object_id = OBJECT_ID('dbo.Shipment')
)
BEGIN
    CREATE INDEX IX_Shipment_OrderId ON dbo.Shipment(OrderId);
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Shipment_DeliveryRouteId'
      AND object_id = OBJECT_ID('dbo.Shipment')
)
BEGIN
    CREATE INDEX IX_Shipment_DeliveryRouteId ON dbo.Shipment(DeliveryRouteId);
END;

COMMIT;