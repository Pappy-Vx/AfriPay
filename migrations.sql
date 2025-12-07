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
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251129215129_init'
)
BEGIN
    CREATE TABLE [Customers] (
        [CustomerId] uniqueidentifier NOT NULL,
        [CustomerReference] nvarchar(50) NOT NULL,
        [FirstName] nvarchar(100) NOT NULL,
        [LastName] nvarchar(100) NOT NULL,
        [Email] nvarchar(200) NOT NULL,
        [PhoneNumber] nvarchar(20) NOT NULL,
        [BVN] nvarchar(11) NOT NULL,
        [IsBvnVerified] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Customers] PRIMARY KEY ([CustomerId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251129215129_init'
)
BEGIN
    CREATE TABLE [Accounts] (
        [AccountId] uniqueidentifier NOT NULL,
        [AccountNumber] nvarchar(10) NOT NULL,
        [CustomerId] uniqueidentifier NOT NULL,
        [RowVersion] rowversion NOT NULL,
        [DateOpened] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        [ProviderReference] nvarchar(100) NULL,
        CONSTRAINT [PK_Accounts] PRIMARY KEY ([AccountId]),
        CONSTRAINT [FK_Accounts_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251129215129_init'
)
BEGIN
    CREATE TABLE [OnboardingRequests] (
        [OnboardingId] uniqueidentifier NOT NULL,
        [RequestReference] nvarchar(50) NOT NULL,
        [FirstName] nvarchar(100) NOT NULL,
        [LastName] nvarchar(100) NOT NULL,
        [Email] nvarchar(200) NOT NULL,
        [PhoneNumber] nvarchar(20) NOT NULL,
        [BVN] nvarchar(11) NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [CustomerId] uniqueidentifier NULL,
        [VirtualAccountId] uniqueidentifier NULL,
        [RequestedAt] datetime2 NOT NULL,
        [CompletedAt] datetime2 NULL,
        [FailureReason] nvarchar(500) NULL,
        [Id] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_OnboardingRequests] PRIMARY KEY ([OnboardingId]),
        CONSTRAINT [FK_OnboardingRequests_Accounts_VirtualAccountId] FOREIGN KEY ([VirtualAccountId]) REFERENCES [Accounts] ([AccountId]),
        CONSTRAINT [FK_OnboardingRequests_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251129215129_init'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Accounts_AccountNumber] ON [Accounts] ([AccountNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251129215129_init'
)
BEGIN
    CREATE INDEX [IX_Accounts_CustomerId] ON [Accounts] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251129215129_init'
)
BEGIN
    CREATE INDEX [IX_Accounts_ProviderReference] ON [Accounts] ([ProviderReference]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251129215129_init'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Customers_BVN] ON [Customers] ([BVN]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251129215129_init'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Customers_CustomerReference] ON [Customers] ([CustomerReference]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251129215129_init'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Customers_Email] ON [Customers] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251129215129_init'
)
BEGIN
    CREATE INDEX [IX_OnboardingRequests_BVN] ON [OnboardingRequests] ([BVN]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251129215129_init'
)
BEGIN
    CREATE INDEX [IX_OnboardingRequests_CustomerId] ON [OnboardingRequests] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251129215129_init'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OnboardingRequests_RequestReference] ON [OnboardingRequests] ([RequestReference]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251129215129_init'
)
BEGIN
    CREATE INDEX [IX_OnboardingRequests_Status] ON [OnboardingRequests] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251129215129_init'
)
BEGIN
    CREATE INDEX [IX_OnboardingRequests_VirtualAccountId] ON [OnboardingRequests] ([VirtualAccountId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251129215129_init'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251129215129_init', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130004814_init2'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OnboardingRequests]') AND [c].[name] = N'Id');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [OnboardingRequests] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [OnboardingRequests] DROP COLUMN [Id];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130004814_init2'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251130004814_init2', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130013933_init3'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251130013933_init3', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    ALTER TABLE [OnboardingRequests] DROP CONSTRAINT [FK_OnboardingRequests_Customers_CustomerId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    DROP INDEX [IX_OnboardingRequests_BVN] ON [OnboardingRequests];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OnboardingRequests]') AND [c].[name] = N'BVN');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [OnboardingRequests] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [OnboardingRequests] DROP COLUMN [BVN];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OnboardingRequests]') AND [c].[name] = N'Email');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [OnboardingRequests] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [OnboardingRequests] DROP COLUMN [Email];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OnboardingRequests]') AND [c].[name] = N'FirstName');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [OnboardingRequests] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [OnboardingRequests] DROP COLUMN [FirstName];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OnboardingRequests]') AND [c].[name] = N'LastName');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [OnboardingRequests] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [OnboardingRequests] DROP COLUMN [LastName];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OnboardingRequests]') AND [c].[name] = N'PhoneNumber');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [OnboardingRequests] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [OnboardingRequests] DROP COLUMN [PhoneNumber];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    EXEC sp_rename N'[OnboardingRequests].[RequestedAt]', N'CreatedAt', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    DROP INDEX [IX_OnboardingRequests_Status] ON [OnboardingRequests];
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OnboardingRequests]') AND [c].[name] = N'Status');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [OnboardingRequests] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [OnboardingRequests] ALTER COLUMN [Status] int NOT NULL;
    CREATE INDEX [IX_OnboardingRequests_Status] ON [OnboardingRequests] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    ALTER TABLE [OnboardingRequests] ADD [Country] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    ALTER TABLE [OnboardingRequests] ADD [CustomerId1] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    ALTER TABLE [OnboardingRequests] ADD [Id] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    ALTER TABLE [OnboardingRequests] ADD [IdentityCountry] nvarchar(3) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    ALTER TABLE [OnboardingRequests] ADD [IdentityNumber] nvarchar(50) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    ALTER TABLE [OnboardingRequests] ADD [SelfieUrl] nvarchar(500) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    ALTER TABLE [Customers] ADD [Status] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    ALTER TABLE [Accounts] ADD [AccountType] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    ALTER TABLE [Accounts] ADD [DailyTransferLimit] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    ALTER TABLE [Accounts] ADD [DailyTransferTotal] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    ALTER TABLE [Accounts] ADD [LastDailyResetDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    ALTER TABLE [Accounts] ADD [SingleTransferLimit] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    CREATE TABLE [AmlScreenings] (
        [Id] uniqueidentifier NOT NULL,
        [OnboardingId] uniqueidentifier NOT NULL,
        [Status] int NOT NULL,
        [RiskLevel] int NOT NULL,
        [RiskScore] decimal(5,2) NOT NULL,
        [ScreeningProvider] nvarchar(100) NULL,
        [ScreeningReference] nvarchar(100) NULL,
        [ScreeningDate] datetime2 NULL,
        [Flags] nvarchar(1000) NOT NULL,
        [Notes] nvarchar(1000) NULL,
        [RawResponse] nvarchar(4000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_AmlScreenings] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    CREATE TABLE [IdentityVerifications] (
        [Id] uniqueidentifier NOT NULL,
        [OnboardingId] uniqueidentifier NOT NULL,
        [IdentityNumber] nvarchar(50) NOT NULL,
        [IdentityCountry] nvarchar(3) NOT NULL,
        [Status] int NOT NULL,
        [VerificationProvider] nvarchar(100) NULL,
        [VerificationReference] nvarchar(100) NULL,
        [VerificationDate] datetime2 NULL,
        [MatchScore] decimal(5,2) NULL,
        [FailureReason] nvarchar(500) NULL,
        [RawResponse] nvarchar(4000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_IdentityVerifications] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    CREATE TABLE [ManualReviewCases] (
        [Id] uniqueidentifier NOT NULL,
        [OnboardingId] uniqueidentifier NOT NULL,
        [Status] int NOT NULL,
        [Priority] int NOT NULL,
        [AssignedTo] nvarchar(100) NULL,
        [AssignedAt] datetime2 NULL,
        [ResolvedAt] datetime2 NULL,
        [Resolution] nvarchar(100) NULL,
        [ResolutionNotes] nvarchar(1000) NULL,
        [Reason] nvarchar(500) NOT NULL,
        [Comments] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_ManualReviewCases] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    CREATE INDEX [IX_OnboardingRequests_CustomerId1] ON [OnboardingRequests] ([CustomerId1]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    CREATE INDEX [IX_AmlScreenings_OnboardingId] ON [AmlScreenings] ([OnboardingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    CREATE INDEX [IX_AmlScreenings_RiskLevel] ON [AmlScreenings] ([RiskLevel]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    CREATE INDEX [IX_AmlScreenings_ScreeningReference] ON [AmlScreenings] ([ScreeningReference]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    CREATE INDEX [IX_AmlScreenings_Status] ON [AmlScreenings] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    CREATE INDEX [IX_IdentityVerifications_OnboardingId] ON [IdentityVerifications] ([OnboardingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    CREATE INDEX [IX_IdentityVerifications_Status] ON [IdentityVerifications] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    CREATE INDEX [IX_IdentityVerifications_VerificationReference] ON [IdentityVerifications] ([VerificationReference]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    CREATE INDEX [IX_ManualReviewCases_AssignedTo] ON [ManualReviewCases] ([AssignedTo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    CREATE INDEX [IX_ManualReviewCases_OnboardingId] ON [ManualReviewCases] ([OnboardingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    CREATE INDEX [IX_ManualReviewCases_Priority] ON [ManualReviewCases] ([Priority]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    CREATE INDEX [IX_ManualReviewCases_Status] ON [ManualReviewCases] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    ALTER TABLE [OnboardingRequests] ADD CONSTRAINT [FK_OnboardingRequests_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    ALTER TABLE [OnboardingRequests] ADD CONSTRAINT [FK_OnboardingRequests_Customers_CustomerId1] FOREIGN KEY ([CustomerId1]) REFERENCES [Customers] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201162741_init4'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251201162741_init4', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201164519_init5'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251201164519_init5', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201202450_init6'
)
BEGIN
    DROP INDEX [IX_Customers_Email] ON [Customers];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201202450_init6'
)
BEGIN
    ALTER TABLE [OnboardingRequests] ADD [ContactInfo_AlternativePhoneNumber] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201202450_init6'
)
BEGIN
    ALTER TABLE [OnboardingRequests] ADD [Email] nvarchar(100) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201202450_init6'
)
BEGIN
    ALTER TABLE [OnboardingRequests] ADD [PhoneNumber] nvarchar(20) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201202450_init6'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'PhoneNumber');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [Customers] ALTER COLUMN [PhoneNumber] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201202450_init6'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Email');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var8 + '];');
    ALTER TABLE [Customers] ALTER COLUMN [Email] nvarchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201202450_init6'
)
BEGIN
    ALTER TABLE [Customers] ADD [AddressCity] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201202450_init6'
)
BEGIN
    ALTER TABLE [Customers] ADD [AddressCountry] nvarchar(3) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201202450_init6'
)
BEGIN
    ALTER TABLE [Customers] ADD [AddressPostalCode] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201202450_init6'
)
BEGIN
    ALTER TABLE [Customers] ADD [AddressState] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201202450_init6'
)
BEGIN
    ALTER TABLE [Customers] ADD [AddressStreet] nvarchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201202450_init6'
)
BEGIN
    ALTER TABLE [Customers] ADD [ContactInfo_AlternativePhoneNumber] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201202450_init6'
)
BEGIN
    ALTER TABLE [Customers] ADD [Customer_Email] nvarchar(450) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201202450_init6'
)
BEGIN
    ALTER TABLE [Customers] ADD [Customer_PhoneNumber] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201202450_init6'
)
BEGIN
    ALTER TABLE [Accounts] ADD [BalanceAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201202450_init6'
)
BEGIN
    ALTER TABLE [Accounts] ADD [BalanceCurrency] nvarchar(3) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201202450_init6'
)
BEGIN
    ALTER TABLE [Accounts] ADD [ReservedBalanceAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201202450_init6'
)
BEGIN
    ALTER TABLE [Accounts] ADD [ReservedBalanceCurrency] nvarchar(3) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201202450_init6'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Customers_Customer_Email] ON [Customers] ([Customer_Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201202450_init6'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251201202450_init6', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201211021_init7'
)
BEGIN
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OnboardingRequests]') AND [c].[name] = N'Id');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [OnboardingRequests] DROP CONSTRAINT [' + @var9 + '];');
    ALTER TABLE [OnboardingRequests] DROP COLUMN [Id];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251201211021_init7'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251201211021_init7', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202193847_init8'
)
BEGIN
    DROP INDEX [IX_Customers_Customer_Email] ON [Customers];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202193847_init8'
)
BEGIN
    DECLARE @var10 sysname;
    SELECT @var10 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Customer_Email');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var10 + '];');
    ALTER TABLE [Customers] DROP COLUMN [Customer_Email];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202193847_init8'
)
BEGIN
    DECLARE @var11 sysname;
    SELECT @var11 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'IsBvnVerified');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var11 + '];');
    ALTER TABLE [Customers] ADD DEFAULT CAST(0 AS bit) FOR [IsBvnVerified];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202193847_init8'
)
BEGIN
    DECLARE @var12 sysname;
    SELECT @var12 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'IsActive');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var12 + '];');
    ALTER TABLE [Customers] ADD DEFAULT CAST(1 AS bit) FOR [IsActive];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202193847_init8'
)
BEGIN
    ALTER TABLE [Customers] ADD [PasswordHash] nvarchar(500) NOT NULL DEFAULT N'1';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202193847_init8'
)
BEGIN
            UPDATE Customers 
            SET PasswordHash = 'AQAAAAEAACcQAAAAEFPK...' 
            WHERE PasswordHash IS NULL
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202193847_init8'
)
BEGIN
    DECLARE @var13 sysname;
    SELECT @var13 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'PasswordHash');
    IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var13 + '];');
    ALTER TABLE [Customers] ALTER COLUMN [PasswordHash] nvarchar(500) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202193847_init8'
)
BEGIN
    CREATE INDEX [IX_Customers_IsActive] ON [Customers] ([IsActive]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202193847_init8'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251202193847_init8', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203191724_init9'
)
BEGIN
    ALTER TABLE [Customers] ADD [NormalizedUserTag] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203191724_init9'
)
BEGIN
    ALTER TABLE [Customers] ADD [UserTag] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203191724_init9'
)
BEGIN
    ALTER TABLE [Customers] ADD [UserTagSetAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203191724_init9'
)
BEGIN
    CREATE TABLE [Transactions] (
        [Id] uniqueidentifier NOT NULL,
        [TransactionReference] nvarchar(50) NOT NULL,
        [AccountId] uniqueidentifier NOT NULL,
        [CustomerId] uniqueidentifier NOT NULL,
        [Direction] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Currency] nvarchar(3) NOT NULL,
        [BalanceBefore] decimal(18,2) NOT NULL,
        [BalanceAfter] decimal(18,2) NOT NULL,
        [TransferId] uniqueidentifier NULL,
        [Narration] nvarchar(200) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Transactions] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203191724_init9'
)
BEGIN
    CREATE TABLE [Transfers] (
        [Id] uniqueidentifier NOT NULL,
        [TransferReference] nvarchar(50) NOT NULL,
        [IdempotencyKey] nvarchar(100) NULL,
        [SourceAccountId] uniqueidentifier NOT NULL,
        [SourceCustomerId] uniqueidentifier NOT NULL,
        [DestinationAccountId] uniqueidentifier NOT NULL,
        [DestinationCustomerId] uniqueidentifier NOT NULL,
        [DestinationUserTag] nvarchar(50) NULL,
        [Amount] decimal(18,2) NOT NULL,
        [AmountCurrency] nvarchar(3) NOT NULL,
        [Fee] decimal(18,2) NULL,
        [FeeCurrency] nvarchar(3) NULL,
        [TotalDebitAmount] decimal(18,2) NOT NULL,
        [TotalDebitCurrency] nvarchar(3) NOT NULL,
        [Type] int NOT NULL,
        [Status] int NOT NULL,
        [Description] nvarchar(500) NULL,
        [Narration] nvarchar(200) NULL,
        [FailureReason] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CompletedAt] datetime2 NULL,
        CONSTRAINT [PK_Transfers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203191724_init9'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Customers_NormalizedUserTag] ON [Customers] ([NormalizedUserTag]) WHERE [NormalizedUserTag] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203191724_init9'
)
BEGIN
    CREATE INDEX [IX_Transactions_AccountId] ON [Transactions] ([AccountId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203191724_init9'
)
BEGIN
    CREATE INDEX [IX_Transactions_CreatedAt] ON [Transactions] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203191724_init9'
)
BEGIN
    CREATE INDEX [IX_Transactions_CustomerId] ON [Transactions] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203191724_init9'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Transactions_TransactionReference] ON [Transactions] ([TransactionReference]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203191724_init9'
)
BEGIN
    CREATE INDEX [IX_Transfers_CreatedAt] ON [Transfers] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203191724_init9'
)
BEGIN
    CREATE INDEX [IX_Transfers_DestinationCustomerId] ON [Transfers] ([DestinationCustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203191724_init9'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Transfers_IdempotencyKey] ON [Transfers] ([IdempotencyKey]) WHERE [IdempotencyKey] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203191724_init9'
)
BEGIN
    CREATE INDEX [IX_Transfers_SourceCustomerId] ON [Transfers] ([SourceCustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203191724_init9'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Transfers_TransferReference] ON [Transfers] ([TransferReference]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203191724_init9'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251203191724_init9', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251205123625_init10'
)
BEGIN
    DROP INDEX [IX_Customers_BVN] ON [Customers];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251205123625_init10'
)
BEGIN
    DROP INDEX [IX_Customers_CustomerReference] ON [Customers];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251205123625_init10'
)
BEGIN
    DECLARE @var14 sysname;
    SELECT @var14 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'BVN');
    IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var14 + '];');
    ALTER TABLE [Customers] DROP COLUMN [BVN];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251205123625_init10'
)
BEGIN
    DECLARE @var15 sysname;
    SELECT @var15 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Status');
    IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var15 + '];');
    ALTER TABLE [Customers] ALTER COLUMN [Status] nvarchar(50) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251205123625_init10'
)
BEGIN
    ALTER TABLE [Customers] ADD [Customer_Email] nvarchar(450) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251205123625_init10'
)
BEGIN
    ALTER TABLE [Customers] ADD [IdentityNumber] nvarchar(50) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251205123625_init10'
)
BEGIN
    ALTER TABLE [Customers] ADD [IdentityType] nvarchar(50) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251205123625_init10'
)
BEGIN
    ALTER TABLE [Customers] ADD [IsIdentityVerified] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251205123625_init10'
)
BEGIN
    CREATE INDEX [IX_Customers_CustomerReference] ON [Customers] ([CustomerReference]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251205123625_init10'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Customers_Email] ON [Customers] ([Customer_Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251205123625_init10'
)
BEGIN
    CREATE INDEX [IX_Customers_Email_IsActive] ON [Customers] ([Customer_Email], [IsActive]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251205123625_init10'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Customers_Identity] ON [Customers] ([IdentityNumber], [IdentityType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251205123625_init10'
)
BEGIN
    CREATE INDEX [IX_Customers_Status] ON [Customers] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251205123625_init10'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251205123625_init10', N'8.0.0');
END;
GO

COMMIT;
GO

