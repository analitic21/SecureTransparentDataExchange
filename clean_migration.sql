IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [AppUsers] (
    [Id] int NOT NULL IDENTITY,
    [UserName] nvarchar(max) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [FullName] nvarchar(max) NULL,
    CONSTRAINT [PK_AppUsers] PRIMARY KEY ([Id])
);

CREATE TABLE [CargoTypes] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_CargoTypes] PRIMARY KEY ([Id])
);

CREATE TABLE [Country] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Country] PRIMARY KEY ([Id])
);

CREATE TABLE [EncryptionKeys] (
    [Id] int NOT NULL IDENTITY,
    [PublicKey] nvarchar(max) NOT NULL,
    [PrivateKey] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ExpiresAt] datetime2 NULL,
    CONSTRAINT [PK_EncryptionKeys] PRIMARY KEY ([Id])
);

CREATE TABLE [LoginAgreements] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(255) NOT NULL,
    [Version] nvarchar(10) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [IsLatest] bit NOT NULL,
    CONSTRAINT [PK_LoginAgreements] PRIMARY KEY ([Id])
);

CREATE TABLE [Role] (
    [Id] int NOT NULL IDENTITY,
    [Description] nvarchar(max) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_Role] PRIMARY KEY ([Id])
);

CREATE TABLE [City] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [CountryId] int NOT NULL,
    CONSTRAINT [PK_City] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_City_Country_CountryId] FOREIGN KEY ([CountryId]) REFERENCES [Country] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [GeoFenceZones] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(150) NOT NULL,
    [CenterLat] float NOT NULL,
    [CenterLng] float NOT NULL,
    [RadiusMeters] int NOT NULL,
    [EncryptionKeyId] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_GeoFenceZones] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GeoFenceZones_EncryptionKeys_EncryptionKeyId] FOREIGN KEY ([EncryptionKeyId]) REFERENCES [EncryptionKeys] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [JwtSettings] (
    [Id] int NOT NULL IDENTITY,
    [EncryptionKeyId] int NOT NULL,
    [Issuer] nvarchar(256) NOT NULL,
    [Audience] nvarchar(256) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [EncryptedSecretKey] nvarchar(2048) NOT NULL,
    CONSTRAINT [PK_JwtSettings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_JwtSettings_EncryptionKeys_EncryptionKeyId] FOREIGN KEY ([EncryptionKeyId]) REFERENCES [EncryptionKeys] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [LoginAgreementArticles] (
    [Id] int NOT NULL IDENTITY,
    [AgreementId] int NOT NULL,
    [Title] nvarchar(255) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_LoginAgreementArticles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LoginAgreementArticles_LoginAgreements_AgreementId] FOREIGN KEY ([AgreementId]) REFERENCES [LoginAgreements] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] int NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_Role_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Role] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Permissions] (
    [Id] int NOT NULL IDENTITY,
    [PermissionName] nvarchar(100) NOT NULL,
    [RoleId] int NOT NULL,
    CONSTRAINT [PK_Permissions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Permissions_Role_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Role] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Feedbacks] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [Message] nvarchar(1000) NOT NULL,
    [SubmittedAt] datetime2 NOT NULL,
    [CityId] int NOT NULL,
    CONSTRAINT [PK_Feedbacks] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Feedbacks_City_CityId] FOREIGN KEY ([CityId]) REFERENCES [City] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [PostalCode] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(20) NOT NULL,
    [CityId] int NOT NULL,
    CONSTRAINT [PK_PostalCode] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PostalCode_City_CityId] FOREIGN KEY ([CityId]) REFERENCES [City] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Address] (
    [Id] int NOT NULL IDENTITY,
    [Street] nvarchar(250) NOT NULL,
    [PostalCodeId] int NOT NULL,
    [Latitude] float NULL,
    [Longitude] float NULL,
    CONSTRAINT [PK_Address] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Address_PostalCode_PostalCodeId] FOREIGN KEY ([PostalCodeId]) REFERENCES [PostalCode] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [DeliveryRoute] (
    [Id] int NOT NULL IDENTITY,
    [OriginCountryId] int NOT NULL,
    [DestinationLatitude] float NULL,
    [DestinationLongitude] float NULL,
    [OriginCityId] int NOT NULL,
    [OriginPostalCodeId] int NOT NULL,
    [DestinationCountryId] int NOT NULL,
    [DestinationCityId] int NOT NULL,
    [DestinationPostalCodeId] int NOT NULL,
    CONSTRAINT [PK_DeliveryRoute] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DeliveryRoute_City_DestinationCityId] FOREIGN KEY ([DestinationCityId]) REFERENCES [City] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_DeliveryRoute_City_OriginCityId] FOREIGN KEY ([OriginCityId]) REFERENCES [City] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_DeliveryRoute_Country_DestinationCountryId] FOREIGN KEY ([DestinationCountryId]) REFERENCES [Country] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_DeliveryRoute_Country_OriginCountryId] FOREIGN KEY ([OriginCountryId]) REFERENCES [Country] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_DeliveryRoute_PostalCode_DestinationPostalCodeId] FOREIGN KEY ([DestinationPostalCodeId]) REFERENCES [PostalCode] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_DeliveryRoute_PostalCode_OriginPostalCodeId] FOREIGN KEY ([OriginPostalCodeId]) REFERENCES [PostalCode] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Login] (
    [Id] int NOT NULL IDENTITY,
    [Email] nvarchar(256) NOT NULL,
    [UserName] nvarchar(256) NOT NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [Name] nvarchar(max) NULL,
    [LastName] nvarchar(max) NULL,
    [DateOfBirth] datetime2 NULL,
    [AddressId] int NULL,
    [ManualCity] nvarchar(max) NULL,
    [ManualCountry] nvarchar(max) NULL,
    [ManualPostalCode] nvarchar(max) NULL,
    [ManualAddress] nvarchar(max) NULL,
    [RegistrationNumber] nvarchar(max) NULL,
    [TaxId] nvarchar(max) NULL,
    [CompanyName] nvarchar(max) NULL,
    [ContactPerson] nvarchar(max) NULL,
    [ContactPosition] nvarchar(max) NULL,
    [CompanyPhone] nvarchar(max) NULL,
    [CompanyAddress] nvarchar(max) NULL,
    [AgreeToTerms] bit NOT NULL,
    [AgreementVersion] nvarchar(max) NULL,
    [IsConfirmed] bit NOT NULL,
    [IsEmailConfirmed] bit NOT NULL,
    [RoleId] int NULL,
    [UserType] int NULL,
    [TwoFactorSecretKey] nvarchar(max) NULL,
    [IsTwoFactorEnabled] bit NOT NULL,
    [RecoveryCodes] nvarchar(max) NULL,
    [RecoveryCode] nvarchar(max) NULL,
    [RecoveryCodeExpiry] datetime2 NULL,
    [UpdatedAt] datetime2 NULL,
    [LastLogin] datetime2 NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_Login] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Login_Address_AddressId] FOREIGN KEY ([AddressId]) REFERENCES [Address] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Login_Role_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Role] ([Id])
);

CREATE TABLE [Admins] (
    [AdminId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [RoleId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Admins] PRIMARY KEY ([AdminId]),
    CONSTRAINT [FK_Admins_Login_UserId] FOREIGN KEY ([UserId]) REFERENCES [Login] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Admins_Role_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Role] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_Login_UserId] FOREIGN KEY ([UserId]) REFERENCES [Login] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] int NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_Login_UserId] FOREIGN KEY ([UserId]) REFERENCES [Login] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] int NOT NULL,
    [RoleId] int NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_Login_UserId] FOREIGN KEY ([UserId]) REFERENCES [Login] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_Role_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Role] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
    [UserId] int NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_Login_UserId] FOREIGN KEY ([UserId]) REFERENCES [Login] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AuditLogs] (
    [Id] int NOT NULL IDENTITY,
    [Action] nvarchar(max) NOT NULL,
    [User] nvarchar(max) NOT NULL,
    [Timestamp] datetime2 NOT NULL,
    [Type] int NULL,
    [AppUserId] int NULL,
    [LoginId] int NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AuditLogs_AppUsers_AppUserId] FOREIGN KEY ([AppUserId]) REFERENCES [AppUsers] ([Id]),
    CONSTRAINT [FK_AuditLogs_Login_LoginId] FOREIGN KEY ([LoginId]) REFERENCES [Login] ([Id])
);

CREATE TABLE [Clients] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [LastName] nvarchar(max) NOT NULL,
    [DateOfBirth] datetime2 NULL,
    [AddressId] int NULL,
    [LoginId] int NOT NULL,
    CONSTRAINT [PK_Clients] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Clients_Address_AddressId] FOREIGN KEY ([AddressId]) REFERENCES [Address] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Clients_Login_LoginId] FOREIGN KEY ([LoginId]) REFERENCES [Login] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [LegalEntities] (
    [Id] int NOT NULL IDENTITY,
    [LoginId] int NOT NULL,
    [CompanyName] nvarchar(200) NOT NULL,
    [RegistrationNumber] nvarchar(100) NULL,
    [TaxId] nvarchar(50) NULL,
    [ContactPerson] nvarchar(100) NULL,
    [ContactPosition] nvarchar(100) NULL,
    [CompanyPhone] nvarchar(50) NULL,
    [ManualCity] nvarchar(100) NULL,
    [ManualCountry] nvarchar(100) NULL,
    [ManualPostalCode] nvarchar(20) NULL,
    [ManualAddress] nvarchar(200) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_LegalEntities] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LegalEntities_Login_LoginId] FOREIGN KEY ([LoginId]) REFERENCES [Login] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [LoginAgreementConsents] (
    [Id] int NOT NULL IDENTITY,
    [LoginId] int NOT NULL,
    [AgreementId] int NOT NULL,
    [ConsentGiven] bit NOT NULL,
    [AcceptedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_LoginAgreementConsents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LoginAgreementConsents_LoginAgreements_AgreementId] FOREIGN KEY ([AgreementId]) REFERENCES [LoginAgreements] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_LoginAgreementConsents_Login_LoginId] FOREIGN KEY ([LoginId]) REFERENCES [Login] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [RefreshTokens] (
    [Id] int NOT NULL IDENTITY,
    [Token] nvarchar(max) NOT NULL,
    [ExpiryDate] datetime2 NOT NULL,
    [UserId] int NOT NULL,
    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RefreshTokens_Login_UserId] FOREIGN KEY ([UserId]) REFERENCES [Login] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [TwoFactorAuthModels] (
    [Id] int NOT NULL IDENTITY,
    [SecretKey] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UserId] int NOT NULL,
    [LoginId] int NULL,
    CONSTRAINT [PK_TwoFactorAuthModels] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TwoFactorAuthModels_Login_LoginId] FOREIGN KEY ([LoginId]) REFERENCES [Login] ([Id])
);

CREATE TABLE [DeliveryHistories] (
    [Id] int NOT NULL IDENTITY,
    [DeliveryRouteId] int NOT NULL,
    [ActualCost] decimal(18,2) NOT NULL,
    [ActualDistanceInKm] float NOT NULL,
    [OrderDate] datetime2 NOT NULL,
    [DeliveredAt] datetime2 NULL,
    [Remarks] nvarchar(max) NULL,
    [ClientId] int NOT NULL,
    CONSTRAINT [PK_DeliveryHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DeliveryHistories_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_DeliveryHistories_DeliveryRoute_DeliveryRouteId] FOREIGN KEY ([DeliveryRouteId]) REFERENCES [DeliveryRoute] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Companies] (
    [Id] int NOT NULL IDENTITY,
    [CompanyName] nvarchar(max) NOT NULL,
    [ContactNumber] nvarchar(max) NOT NULL,
    [CompanyType] int NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [LegalEntityId] int NOT NULL,
    [AddressId] int NULL,
    [ContactInfo] nvarchar(max) NULL,
    [RegistrationNumber] nvarchar(max) NOT NULL,
    [ContactPerson] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Companies] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Companies_Address_AddressId] FOREIGN KEY ([AddressId]) REFERENCES [Address] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Companies_LegalEntities_LegalEntityId] FOREIGN KEY ([LegalEntityId]) REFERENCES [LegalEntities] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [QRCodeModels] (
    [Id] int NOT NULL IDENTITY,
    [Email] nvarchar(max) NOT NULL,
    [SecretKey] nvarchar(max) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [QrCodeBase64] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [TwoFactorAuthModelId] int NULL,
    CONSTRAINT [PK_QRCodeModels] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_QRCodeModels_TwoFactorAuthModels_TwoFactorAuthModelId] FOREIGN KEY ([TwoFactorAuthModelId]) REFERENCES [TwoFactorAuthModels] ([Id])
);

CREATE TABLE [Employees] (
    [Id] int NOT NULL IDENTITY,
    [LoginId] int NOT NULL,
    [CompanyId] int NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [LastName] nvarchar(100) NOT NULL,
    [Email] nvarchar(256) NOT NULL,
    [Phone] nvarchar(50) NOT NULL,
    [ContractEndDate] datetime2 NULL,
    [Status] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [TemporaryPassword] nvarchar(max) NULL,
    CONSTRAINT [PK_Employees] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Employees_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Employees_Login_LoginId] FOREIGN KEY ([LoginId]) REFERENCES [Login] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [BlockchainLogs] (
    [Id] int NOT NULL IDENTITY,
    [ShipmentId] int NOT NULL,
    [BlockHash] nvarchar(255) NOT NULL,
    [PreviousBlockHash] nvarchar(255) NOT NULL,
    [Data] nvarchar(max) NOT NULL,
    [Timestamp] datetime2 NOT NULL,
    [LoginId] int NOT NULL,
    [Description] nvarchar(500) NULL,
    CONSTRAINT [PK_BlockchainLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BlockchainLogs_Login_LoginId] FOREIGN KEY ([LoginId]) REFERENCES [Login] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [CardTransactions] (
    [Id] int NOT NULL IDENTITY,
    [ShipmentId] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [CardNumberMasked] nvarchar(25) NOT NULL,
    [CardBrand] nvarchar(20) NOT NULL,
    [Status] int NOT NULL,
    [Provider] nvarchar(100) NOT NULL,
    [ProviderTransactionId] nvarchar(100) NOT NULL,
    [ProviderMessage] nvarchar(200) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CompletedAt] datetime2 NULL,
    [CardTransactionId] int NULL,
    [PaymentId] int NULL,
    CONSTRAINT [PK_CardTransactions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CardTransactions_CardTransactions_CardTransactionId] FOREIGN KEY ([CardTransactionId]) REFERENCES [CardTransactions] ([Id])
);

CREATE TABLE [Invoices] (
    [Id] int NOT NULL IDENTITY,
    [Number] nvarchar(50) NOT NULL,
    [UserId] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [PaidAmount] decimal(18,2) NOT NULL,
    [OrderId] int NULL,
    [Currency] nvarchar(10) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [DueDate] datetime2 NOT NULL,
    [Description] nvarchar(500) NULL,
    [Status] int NOT NULL,
    CONSTRAINT [PK_Invoices] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Invoices_Login_UserId] FOREIGN KEY ([UserId]) REFERENCES [Login] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [IoTDevices] (
    [Id] int NOT NULL IDENTITY,
    [TrackingNumber] nvarchar(max) NOT NULL,
    [Latitude] float NOT NULL,
    [Longitude] float NOT NULL,
    [LastUpdated] datetime2 NOT NULL,
    [ParcelId] int NULL,
    CONSTRAINT [PK_IoTDevices] PRIMARY KEY ([Id])
);

CREATE TABLE [LoginTransaction] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [ActionType] int NOT NULL,
    [ActionDate] datetime2 NOT NULL,
    [Details] nvarchar(1000) NOT NULL,
    [PaymentAmount] decimal(18,2) NULL,
    [PaymentStatus] int NOT NULL,
    [ShipmentStatus] int NOT NULL,
    [ShipmentId] int NULL,
    [ProcessedByEmployeeId] int NULL,
    CONSTRAINT [PK_LoginTransaction] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LoginTransaction_Employees_ProcessedByEmployeeId] FOREIGN KEY ([ProcessedByEmployeeId]) REFERENCES [Employees] ([Id]),
    CONSTRAINT [FK_LoginTransaction_Login_UserId] FOREIGN KEY ([UserId]) REFERENCES [Login] ([Id]) ON DELETE CASCADE
);

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

CREATE TABLE [Shipment] (
    [Id] int NOT NULL IDENTITY,
    [TrackingNumber] nvarchar(max) NOT NULL,
    [Status] int NOT NULL,
    [PaymentStatus] int NOT NULL,
    [ClientId] int NULL,
    [OrderId] int NULL,
    [DeliveryRouteId] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Shipment] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Shipment_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]),
    CONSTRAINT [FK_Shipment_DeliveryRoute_DeliveryRouteId] FOREIGN KEY ([DeliveryRouteId]) REFERENCES [DeliveryRoute] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Shipment_Order_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Order] ([Id])
);

CREATE TABLE [Packages] (
    [Id] int NOT NULL IDENTITY,
    [TrackingNumber] nvarchar(max) NOT NULL,
    [Sender] nvarchar(max) NOT NULL,
    [Receiver] nvarchar(max) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CargoTypeId] int NULL,
    [IoTDeviceId] int NULL,
    [ShipmentId] int NULL,
    [CreatedByUserId] int NULL,
    [UpdatedByUserId] int NULL,
    CONSTRAINT [PK_Packages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Packages_CargoTypes_CargoTypeId] FOREIGN KEY ([CargoTypeId]) REFERENCES [CargoTypes] ([Id]),
    CONSTRAINT [FK_Packages_IoTDevices_IoTDeviceId] FOREIGN KEY ([IoTDeviceId]) REFERENCES [IoTDevices] ([Id]),
    CONSTRAINT [FK_Packages_Login_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Login] ([Id]),
    CONSTRAINT [FK_Packages_Login_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [Login] ([Id]),
    CONSTRAINT [FK_Packages_Shipment_ShipmentId] FOREIGN KEY ([ShipmentId]) REFERENCES [Shipment] ([Id])
);

CREATE TABLE [Parcel] (
    [Id] int NOT NULL IDENTITY,
    [TrackingNumber] nvarchar(max) NOT NULL,
    [Sender] nvarchar(max) NOT NULL,
    [Receiver] nvarchar(max) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [WeightKg] decimal(18,2) NOT NULL,
    [CargoValue] decimal(18,2) NOT NULL,
    [PackageCost] decimal(18,2) NOT NULL,
    [CargoTypeId] int NULL,
    [ShipmentId] int NULL,
    [CreatedByUserId] int NULL,
    [UpdatedByUserId] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Parcel] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Parcel_CargoTypes_CargoTypeId] FOREIGN KEY ([CargoTypeId]) REFERENCES [CargoTypes] ([Id]),
    CONSTRAINT [FK_Parcel_Login_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Login] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Parcel_Login_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [Login] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Parcel_Shipment_ShipmentId] FOREIGN KEY ([ShipmentId]) REFERENCES [Shipment] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Payments] (
    [Id] int NOT NULL IDENTITY,
    [ShipmentId] int NOT NULL,
    [OrderId] int NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Currency] nvarchar(3) NOT NULL,
    [InvoiceId] int NULL,
    [ProcessedByEmployeeId] int NULL,
    [Provider] nvarchar(50) NOT NULL,
    [PaymentMethod] nvarchar(50) NOT NULL,
    [Status] int NOT NULL,
    [ExternalPaymentId] nvarchar(100) NULL,
    [TransactionId] nvarchar(100) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CompletedAt] datetime2 NULL,
    CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Payments_Employees_ProcessedByEmployeeId] FOREIGN KEY ([ProcessedByEmployeeId]) REFERENCES [Employees] ([Id]),
    CONSTRAINT [FK_Payments_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Payments_Order_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Order] ([Id]),
    CONSTRAINT [FK_Payments_Shipment_ShipmentId] FOREIGN KEY ([ShipmentId]) REFERENCES [Shipment] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [SystemLogs] (
    [Id] int NOT NULL IDENTITY,
    [EventType] nvarchar(50) NOT NULL,
    [Message] nvarchar(500) NOT NULL,
    [Timestamp] datetime2 NOT NULL,
    [UserId] int NULL,
    [ShipmentId] int NULL,
    [BlockchainLogId] int NULL,
    CONSTRAINT [PK_SystemLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SystemLogs_BlockchainLogs_BlockchainLogId] FOREIGN KEY ([BlockchainLogId]) REFERENCES [BlockchainLogs] ([Id]),
    CONSTRAINT [FK_SystemLogs_Login_UserId] FOREIGN KEY ([UserId]) REFERENCES [Login] ([Id]),
    CONSTRAINT [FK_SystemLogs_Shipment_ShipmentId] FOREIGN KEY ([ShipmentId]) REFERENCES [Shipment] ([Id])
);

CREATE TABLE [TrackingNumbers] (
    [Id] int NOT NULL IDENTITY,
    [TrackingNumber] nvarchar(max) NOT NULL,
    [ShipmentStatus] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ShipmentId] int NOT NULL,
    [UserId] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_TrackingNumbers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TrackingNumbers_Shipment_ShipmentId] FOREIGN KEY ([ShipmentId]) REFERENCES [Shipment] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [PaymentRefunds] (
    [Id] int NOT NULL IDENTITY,
    [PaymentId] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Currency] nvarchar(3) NOT NULL,
    [StripeRefundId] nvarchar(100) NULL,
    [Status] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_PaymentRefunds] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PaymentRefunds_Payments_PaymentId] FOREIGN KEY ([PaymentId]) REFERENCES [Payments] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_Address_PostalCodeId] ON [Address] ([PostalCodeId]);

CREATE INDEX [IX_Admins_RoleId] ON [Admins] ([RoleId]);

CREATE INDEX [IX_Admins_UserId] ON [Admins] ([UserId]);

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [IX_AuditLogs_AppUserId] ON [AuditLogs] ([AppUserId]);

CREATE INDEX [IX_AuditLogs_LoginId] ON [AuditLogs] ([LoginId]);

CREATE INDEX [IX_BlockchainLogs_LoginId] ON [BlockchainLogs] ([LoginId]);

CREATE INDEX [IX_BlockchainLogs_ShipmentId] ON [BlockchainLogs] ([ShipmentId]);

CREATE INDEX [IX_CardTransactions_CardTransactionId] ON [CardTransactions] ([CardTransactionId]);

CREATE INDEX [IX_CardTransactions_PaymentId] ON [CardTransactions] ([PaymentId]);

CREATE INDEX [IX_CardTransactions_ShipmentId] ON [CardTransactions] ([ShipmentId]);

CREATE INDEX [IX_City_CountryId] ON [City] ([CountryId]);

CREATE INDEX [IX_Clients_AddressId] ON [Clients] ([AddressId]);

CREATE INDEX [IX_Clients_LoginId] ON [Clients] ([LoginId]);

CREATE INDEX [IX_Companies_AddressId] ON [Companies] ([AddressId]);

CREATE UNIQUE INDEX [IX_Companies_LegalEntityId] ON [Companies] ([LegalEntityId]);

CREATE INDEX [IX_DeliveryHistories_ClientId] ON [DeliveryHistories] ([ClientId]);

CREATE INDEX [IX_DeliveryHistories_DeliveryRouteId] ON [DeliveryHistories] ([DeliveryRouteId]);

CREATE INDEX [IX_DeliveryRoute_DestinationCityId] ON [DeliveryRoute] ([DestinationCityId]);

CREATE INDEX [IX_DeliveryRoute_DestinationCountryId] ON [DeliveryRoute] ([DestinationCountryId]);

CREATE INDEX [IX_DeliveryRoute_DestinationPostalCodeId] ON [DeliveryRoute] ([DestinationPostalCodeId]);

CREATE INDEX [IX_DeliveryRoute_OriginCityId] ON [DeliveryRoute] ([OriginCityId]);

CREATE INDEX [IX_DeliveryRoute_OriginCountryId] ON [DeliveryRoute] ([OriginCountryId]);

CREATE INDEX [IX_DeliveryRoute_OriginPostalCodeId] ON [DeliveryRoute] ([OriginPostalCodeId]);

CREATE INDEX [IX_Employees_CompanyId] ON [Employees] ([CompanyId]);

CREATE INDEX [IX_Employees_LoginId] ON [Employees] ([LoginId]);

CREATE INDEX [IX_Feedbacks_CityId] ON [Feedbacks] ([CityId]);

CREATE INDEX [IX_GeoFenceZones_EncryptionKeyId] ON [GeoFenceZones] ([EncryptionKeyId]);

CREATE UNIQUE INDEX [IX_Invoices_OrderId] ON [Invoices] ([OrderId]) WHERE [OrderId] IS NOT NULL;

CREATE INDEX [IX_Invoices_UserId] ON [Invoices] ([UserId]);

CREATE UNIQUE INDEX [IX_IoTDevices_ParcelId] ON [IoTDevices] ([ParcelId]) WHERE [ParcelId] IS NOT NULL;

CREATE INDEX [IX_JwtSettings_EncryptionKeyId] ON [JwtSettings] ([EncryptionKeyId]);

CREATE INDEX [IX_LegalEntities_LoginId] ON [LegalEntities] ([LoginId]);

CREATE INDEX [EmailIndex] ON [Login] ([NormalizedEmail]);

CREATE INDEX [IX_Login_AddressId] ON [Login] ([AddressId]);

CREATE INDEX [IX_Login_RoleId] ON [Login] ([RoleId]);

CREATE UNIQUE INDEX [UserNameIndex] ON [Login] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE INDEX [IX_LoginAgreementArticles_AgreementId] ON [LoginAgreementArticles] ([AgreementId]);

CREATE INDEX [IX_LoginAgreementConsents_AgreementId] ON [LoginAgreementConsents] ([AgreementId]);

CREATE INDEX [IX_LoginAgreementConsents_LoginId] ON [LoginAgreementConsents] ([LoginId]);

CREATE INDEX [IX_LoginTransaction_ProcessedByEmployeeId] ON [LoginTransaction] ([ProcessedByEmployeeId]);

CREATE INDEX [IX_LoginTransaction_ShipmentId] ON [LoginTransaction] ([ShipmentId]);

CREATE INDEX [IX_LoginTransaction_UserId] ON [LoginTransaction] ([UserId]);

CREATE INDEX [IX_Order_ClientId] ON [Order] ([ClientId]);

CREATE INDEX [IX_Order_LegalEntityId] ON [Order] ([LegalEntityId]);

CREATE INDEX [IX_Order_LoginId] ON [Order] ([LoginId]);

CREATE INDEX [IX_Order_ShipmentId] ON [Order] ([ShipmentId]);

CREATE INDEX [IX_Packages_CargoTypeId] ON [Packages] ([CargoTypeId]);

CREATE INDEX [IX_Packages_CreatedByUserId] ON [Packages] ([CreatedByUserId]);

CREATE INDEX [IX_Packages_IoTDeviceId] ON [Packages] ([IoTDeviceId]);

CREATE INDEX [IX_Packages_ShipmentId] ON [Packages] ([ShipmentId]);

CREATE INDEX [IX_Packages_UpdatedByUserId] ON [Packages] ([UpdatedByUserId]);

CREATE INDEX [IX_Parcel_CargoTypeId] ON [Parcel] ([CargoTypeId]);

CREATE INDEX [IX_Parcel_CreatedByUserId] ON [Parcel] ([CreatedByUserId]);

CREATE INDEX [IX_Parcel_ShipmentId] ON [Parcel] ([ShipmentId]);

CREATE INDEX [IX_Parcel_UpdatedByUserId] ON [Parcel] ([UpdatedByUserId]);

CREATE INDEX [IX_PaymentRefunds_PaymentId] ON [PaymentRefunds] ([PaymentId]);

CREATE INDEX [IX_Payments_InvoiceId] ON [Payments] ([InvoiceId]);

CREATE INDEX [IX_Payments_OrderId] ON [Payments] ([OrderId]);

CREATE INDEX [IX_Payments_ProcessedByEmployeeId] ON [Payments] ([ProcessedByEmployeeId]);

CREATE INDEX [IX_Payments_ShipmentId] ON [Payments] ([ShipmentId]);

CREATE UNIQUE INDEX [IX_Payments_TransactionId] ON [Payments] ([TransactionId]) WHERE [TransactionId] IS NOT NULL;

CREATE INDEX [IX_Permissions_RoleId] ON [Permissions] ([RoleId]);

CREATE INDEX [IX_PostalCode_CityId] ON [PostalCode] ([CityId]);

CREATE INDEX [IX_QRCodeModels_TwoFactorAuthModelId] ON [QRCodeModels] ([TwoFactorAuthModelId]);

CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [Role] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_Shipment_ClientId] ON [Shipment] ([ClientId]);

CREATE INDEX [IX_Shipment_DeliveryRouteId] ON [Shipment] ([DeliveryRouteId]);

CREATE INDEX [IX_Shipment_OrderId] ON [Shipment] ([OrderId]);

CREATE INDEX [IX_SystemLogs_BlockchainLogId] ON [SystemLogs] ([BlockchainLogId]);

CREATE INDEX [IX_SystemLogs_ShipmentId] ON [SystemLogs] ([ShipmentId]);

CREATE INDEX [IX_SystemLogs_UserId] ON [SystemLogs] ([UserId]);

CREATE INDEX [IX_TrackingNumbers_ShipmentId] ON [TrackingNumbers] ([ShipmentId]);

CREATE INDEX [IX_TwoFactorAuthModels_LoginId] ON [TwoFactorAuthModels] ([LoginId]);

ALTER TABLE [BlockchainLogs] ADD CONSTRAINT [FK_BlockchainLogs_Shipment_ShipmentId] FOREIGN KEY ([ShipmentId]) REFERENCES [Shipment] ([Id]) ON DELETE CASCADE;

ALTER TABLE [CardTransactions] ADD CONSTRAINT [FK_CardTransactions_Payments_PaymentId] FOREIGN KEY ([PaymentId]) REFERENCES [Payments] ([Id]);

ALTER TABLE [CardTransactions] ADD CONSTRAINT [FK_CardTransactions_Shipment_ShipmentId] FOREIGN KEY ([ShipmentId]) REFERENCES [Shipment] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Invoices] ADD CONSTRAINT [FK_Invoices_Order_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Order] ([Id]);

ALTER TABLE [IoTDevices] ADD CONSTRAINT [FK_IoTDevices_Parcel_ParcelId] FOREIGN KEY ([ParcelId]) REFERENCES [Parcel] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [LoginTransaction] ADD CONSTRAINT [FK_LoginTransaction_Shipment_ShipmentId] FOREIGN KEY ([ShipmentId]) REFERENCES [Shipment] ([Id]);

ALTER TABLE [Order] ADD CONSTRAINT [FK_Order_Shipment_ShipmentId] FOREIGN KEY ([ShipmentId]) REFERENCES [Shipment] ([Id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260507170552_InitialCleanSchema', N'9.0.10');

COMMIT;
GO

