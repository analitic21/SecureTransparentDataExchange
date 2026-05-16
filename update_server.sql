BEGIN TRANSACTION;
DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Shipment]') AND [c].[name] = N'ClientId');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Shipment] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [Shipment] ALTER COLUMN [ClientId] int NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260427142858_FixShipmentClientNullable', N'9.0.10');

ALTER TABLE [Parcel] ADD [CargoValue] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [Parcel] ADD [PackageCost] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [Parcel] ADD [WeightKg] decimal(18,2) NOT NULL DEFAULT 0.0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260428161242_AddParcelWeightAndCost', N'9.0.10');

COMMIT;
GO

