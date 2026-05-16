BEGIN TRANSACTION;

IF OBJECT_ID(N'[GeoFenceZones]', N'U') IS NULL
BEGIN
    CREATE TABLE [GeoFenceZones] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(150) NOT NULL,
        [CenterLat] float NOT NULL,
        [CenterLng] float NOT NULL,
        [RadiusMeters] int NOT NULL,
        [EncryptionKeyId] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_GeoFenceZones] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GeoFenceZones_EncryptionKeys_EncryptionKeyId]
            FOREIGN KEY ([EncryptionKeyId]) REFERENCES [EncryptionKeys] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_GeoFenceZones_EncryptionKeyId')
    CREATE INDEX [IX_GeoFenceZones_EncryptionKeyId] ON [GeoFenceZones] ([EncryptionKeyId]);

IF OBJECT_ID(N'[Order]', N'U') IS NULL
BEGIN
    CREATE TABLE [Order] (
        [Id] int NOT NULL IDENTITY,
        [OrderNumber] nvarchar(32) NOT NULL,
        [LoginId] int NOT NULL,
        [UserType] int NOT NULL,
        [ClientId] int NULL,
        [LegalEntityId] int NULL,
        [ShipmentId] int NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [Currency] nvarchar(3) NOT NULL,
        [PaymentStatus] int NOT NULL,
        [ExternalOrderNumber] nvarchar(100) NULL,
        [Source] nvarchar(50) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Order] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Order_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]),
        CONSTRAINT [FK_Order_LegalEntities_LegalEntityId] FOREIGN KEY ([LegalEntityId]) REFERENCES [LegalEntities] ([Id]),
        CONSTRAINT [FK_Order_Login_LoginId] FOREIGN KEY ([LoginId]) REFERENCES [Login] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Order_ClientId')
    CREATE INDEX [IX_Order_ClientId] ON [Order] ([ClientId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Order_LegalEntityId')
    CREATE INDEX [IX_Order_LegalEntityId] ON [Order] ([LegalEntityId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Order_LoginId')
    CREATE INDEX [IX_Order_LoginId] ON [Order] ([LoginId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Order_ShipmentId')
    CREATE INDEX [IX_Order_ShipmentId] ON [Order] ([ShipmentId]);

IF OBJECT_ID(N'[FK_Order_Shipment_ShipmentId]', N'F') IS NULL
BEGIN
    ALTER TABLE [Order]
    ADD CONSTRAINT [FK_Order_Shipment_ShipmentId]
    FOREIGN KEY ([ShipmentId]) REFERENCES [Shipment] ([Id]);
END;

IF OBJECT_ID(N'[PaymentRefunds]', N'U') IS NULL
BEGIN
    CREATE TABLE [PaymentRefunds] (
        [Id] int NOT NULL IDENTITY,
        [PaymentId] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Currency] nvarchar(3) NOT NULL,
        [StripeRefundId] nvarchar(100) NULL,
        [Status] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PaymentRefunds] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PaymentRefunds_Payments_PaymentId]
            FOREIGN KEY ([PaymentId]) REFERENCES [Payments] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PaymentRefunds_PaymentId')
    CREATE INDEX [IX_PaymentRefunds_PaymentId] ON [PaymentRefunds] ([PaymentId]);

IF COL_LENGTH('Invoices', 'OrderId') IS NOT NULL
   AND OBJECT_ID(N'[FK_Invoices_Order_OrderId]', N'F') IS NULL
BEGIN
    ALTER TABLE [Invoices]
    ADD CONSTRAINT [FK_Invoices_Order_OrderId]
    FOREIGN KEY ([OrderId]) REFERENCES [Order] ([Id]);
END;

IF COL_LENGTH('Shipment', 'OrderId') IS NOT NULL
   AND OBJECT_ID(N'[FK_Shipment_Order_OrderId]', N'F') IS NULL
BEGIN
    ALTER TABLE [Shipment]
    ADD CONSTRAINT [FK_Shipment_Order_OrderId]
    FOREIGN KEY ([OrderId]) REFERENCES [Order] ([Id]);
END;

IF OBJECT_ID(N'[FK_Parcel_ShipmentClient_ShipmentClientShipmentId_ShipmentClientClientId]', N'F') IS NOT NULL
BEGIN
    ALTER TABLE [Parcel]
    DROP CONSTRAINT [FK_Parcel_ShipmentClient_ShipmentClientShipmentId_ShipmentClientClientId];
END;

IF OBJECT_ID(N'[FK_Packages_ShipmentClient_ShipmentClientShipmentId_ShipmentClientClientId]', N'F') IS NOT NULL
BEGIN
    ALTER TABLE [Packages]
    DROP CONSTRAINT [FK_Packages_ShipmentClient_ShipmentClientShipmentId_ShipmentClientClientId];
END;

IF OBJECT_ID(N'[ShipmentClient]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [ShipmentClient];
END;

IF OBJECT_ID(N'[__EFMigrationsHistory]', N'U') IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260507170552_InitialCleanSchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260507170552_InitialCleanSchema', N'9.0.10');
END;

COMMIT;
GO