BEGIN TRANSACTION;

-- =========================
-- Invoice table
-- =========================
IF OBJECT_ID(N'[Invoice]', N'U') IS NULL
BEGIN
    CREATE TABLE [Invoice] (
        [Id] int NOT NULL IDENTITY,
        [Number] nvarchar(50) NOT NULL,
        [UserId] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [PaidAmount] decimal(18,2) NOT NULL DEFAULT 0,
        [Currency] nvarchar(10) NOT NULL DEFAULT N'EUR',
        [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [DueDate] datetime2 NOT NULL,
        [Description] nvarchar(500) NULL,
        [Status] int NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Invoice] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Invoice_Login_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [Login] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = N'IX_Invoice_UserId' 
    AND object_id = OBJECT_ID(N'[Invoice]')
)
BEGIN
    CREATE INDEX [IX_Invoice_UserId] ON [Invoice] ([UserId]);
END;

-- =========================
-- Payments additions
-- =========================
IF COL_LENGTH('Payments', 'InvoiceId') IS NULL
    ALTER TABLE [Payments] ADD [InvoiceId] int NULL;

IF COL_LENGTH('Payments', 'ProcessedByEmployeeId') IS NULL
    ALTER TABLE [Payments] ADD [ProcessedByEmployeeId] int NULL;

IF COL_LENGTH('Payments', 'Currency') IS NULL
    ALTER TABLE [Payments] ADD [Currency] nvarchar(3) NOT NULL DEFAULT N'EUR';

IF COL_LENGTH('Payments', 'Provider') IS NULL
    ALTER TABLE [Payments] ADD [Provider] nvarchar(50) NOT NULL DEFAULT N'Stripe';

IF COL_LENGTH('Payments', 'ExternalPaymentId') IS NULL
    ALTER TABLE [Payments] ADD [ExternalPaymentId] nvarchar(100) NULL;

IF COL_LENGTH('Payments', 'CreatedAt') IS NULL
    ALTER TABLE [Payments] ADD [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME();

IF COL_LENGTH('Payments', 'CompletedAt') IS NULL
    ALTER TABLE [Payments] ADD [CompletedAt] datetime2 NULL;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = N'IX_Payments_InvoiceId' 
    AND object_id = OBJECT_ID(N'[Payments]')
)
BEGIN
    CREATE INDEX [IX_Payments_InvoiceId] ON [Payments] ([InvoiceId]);
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys 
    WHERE name = N'FK_Payments_Invoice_InvoiceId'
)
BEGIN
    ALTER TABLE [Payments]
    ADD CONSTRAINT [FK_Payments_Invoice_InvoiceId]
    FOREIGN KEY ([InvoiceId]) REFERENCES [Invoice] ([Id]) ON DELETE NO ACTION;
END;

-- =========================
-- CardTransactions additions
-- =========================
IF COL_LENGTH('CardTransactions', 'CardBrand') IS NULL
    ALTER TABLE [CardTransactions] ADD [CardBrand] nvarchar(20) NOT NULL DEFAULT N'';

IF COL_LENGTH('CardTransactions', 'Provider') IS NULL
    ALTER TABLE [CardTransactions] ADD [Provider] nvarchar(100) NOT NULL DEFAULT N'Stripe';

IF COL_LENGTH('CardTransactions', 'ProviderTransactionId') IS NULL
    ALTER TABLE [CardTransactions] ADD [ProviderTransactionId] nvarchar(100) NOT NULL DEFAULT N'';

IF COL_LENGTH('CardTransactions', 'ProviderMessage') IS NULL
    ALTER TABLE [CardTransactions] ADD [ProviderMessage] nvarchar(200) NULL;

IF COL_LENGTH('CardTransactions', 'CreatedAt') IS NULL
    ALTER TABLE [CardTransactions] ADD [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME();

IF COL_LENGTH('CardTransactions', 'CompletedAt') IS NULL
    ALTER TABLE [CardTransactions] ADD [CompletedAt] datetime2 NULL;

-- =========================
-- EF migration history
-- =========================
IF NOT EXISTS (
    SELECT 1 FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502164541_UpdateBillingRelations'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260502164541_UpdateBillingRelations', N'9.0.10');
END;

COMMIT;
GO