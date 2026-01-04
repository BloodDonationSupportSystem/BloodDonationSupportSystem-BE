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
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE TABLE [BloodGroups] (
        [Id] uniqueidentifier NOT NULL,
        [GroupName] nvarchar(450) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_BloodGroups] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE TABLE [ComponentTypes] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [ShelfLifeDays] int NOT NULL,
        CONSTRAINT [PK_ComponentTypes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE TABLE [Locations] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Address] nvarchar(max) NOT NULL,
        [Latitude] nvarchar(max) NOT NULL,
        [Longitude] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Locations] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE TABLE [Roles] (
        [Id] uniqueidentifier NOT NULL,
        [RoleName] nvarchar(450) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE TABLE [EmergencyRequests] (
        [Id] uniqueidentifier NOT NULL,
        [PatientName] nvarchar(max) NOT NULL,
        [QuantityUnits] int NOT NULL,
        [RequestDate] datetimeoffset NOT NULL,
        [UrgencyLevel] nvarchar(max) NOT NULL,
        [ContactInfo] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [BloodGroupId] uniqueidentifier NOT NULL,
        [ComponentTypeId] uniqueidentifier NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [LastUpdatedBy] nvarchar(max) NULL,
        [CreatedTime] datetimeoffset NULL,
        [LastUpdatedTime] datetimeoffset NULL,
        [DeletedTime] datetimeoffset NULL,
        CONSTRAINT [PK_EmergencyRequests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EmergencyRequests_BloodGroups_BloodGroupId] FOREIGN KEY ([BloodGroupId]) REFERENCES [BloodGroups] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_EmergencyRequests_ComponentTypes_ComponentTypeId] FOREIGN KEY ([ComponentTypeId]) REFERENCES [ComponentTypes] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] uniqueidentifier NOT NULL,
        [UserName] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [Password] nvarchar(max) NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [PhoneNumber] nvarchar(max) NOT NULL,
        [LastLogin] datetimeoffset NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Users_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE TABLE [BlogPosts] (
        [Id] uniqueidentifier NOT NULL,
        [Title] nvarchar(max) NOT NULL,
        [Body] nvarchar(max) NOT NULL,
        [IsPublished] bit NOT NULL,
        [AuthorId] uniqueidentifier NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [LastUpdatedBy] nvarchar(max) NULL,
        [CreatedTime] datetimeoffset NULL,
        [LastUpdatedTime] datetimeoffset NULL,
        [DeletedTime] datetimeoffset NULL,
        CONSTRAINT [PK_BlogPosts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BlogPosts_Users_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE TABLE [BloodRequests] (
        [Id] uniqueidentifier NOT NULL,
        [QuantityUnits] int NOT NULL,
        [RequestDate] datetimeoffset NOT NULL,
        [Status] nvarchar(450) NOT NULL,
        [NeededByDate] datetimeoffset NOT NULL,
        [RequestedBy] uniqueidentifier NOT NULL,
        [BloodGroupId] uniqueidentifier NOT NULL,
        [ComponentTypeId] uniqueidentifier NOT NULL,
        [LocationId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_BloodRequests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BloodRequests_BloodGroups_BloodGroupId] FOREIGN KEY ([BloodGroupId]) REFERENCES [BloodGroups] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BloodRequests_ComponentTypes_ComponentTypeId] FOREIGN KEY ([ComponentTypeId]) REFERENCES [ComponentTypes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BloodRequests_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Locations] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BloodRequests_Users_RequestedBy] FOREIGN KEY ([RequestedBy]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE TABLE [Documents] (
        [Id] uniqueidentifier NOT NULL,
        [Title] nvarchar(max) NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [DocumentType] nvarchar(max) NOT NULL,
        [CreatedDate] datetimeoffset NOT NULL,
        [CreatedBy] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_Documents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Documents_Users_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE TABLE [DonorProfiles] (
        [Id] uniqueidentifier NOT NULL,
        [DateOfBirth] datetimeoffset NOT NULL,
        [Gender] bit NOT NULL,
        [LastDonationDate] datetimeoffset NOT NULL,
        [HealthStatus] nvarchar(max) NOT NULL,
        [LastHealthCheckDate] datetimeoffset NOT NULL,
        [TotalDonations] int NOT NULL,
        [Address] nvarchar(max) NOT NULL,
        [Latitude] nvarchar(max) NOT NULL,
        [Longitude] nvarchar(max) NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [BloodGroupId] uniqueidentifier NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [LastUpdatedBy] nvarchar(max) NULL,
        [CreatedTime] datetimeoffset NULL,
        [LastUpdatedTime] datetimeoffset NULL,
        [DeletedTime] datetimeoffset NULL,
        CONSTRAINT [PK_DonorProfiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DonorProfiles_BloodGroups_BloodGroupId] FOREIGN KEY ([BloodGroupId]) REFERENCES [BloodGroups] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DonorProfiles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] uniqueidentifier NOT NULL,
        [Type] nvarchar(max) NOT NULL,
        [Message] nvarchar(max) NOT NULL,
        [IsRead] bit NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [LastUpdatedBy] nvarchar(max) NULL,
        [CreatedTime] datetimeoffset NULL,
        [LastUpdatedTime] datetimeoffset NULL,
        [DeletedTime] datetimeoffset NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notifications_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE TABLE [DonationEvents] (
        [Id] uniqueidentifier NOT NULL,
        [QuantityUnits] int NOT NULL,
        [CollectedAt] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [DonorId] uniqueidentifier NOT NULL,
        [BloodGroupId] uniqueidentifier NOT NULL,
        [ComponentTypeId] uniqueidentifier NOT NULL,
        [LocationId] uniqueidentifier NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [LastUpdatedBy] nvarchar(max) NULL,
        [CreatedTime] datetimeoffset NULL,
        [LastUpdatedTime] datetimeoffset NULL,
        [DeletedTime] datetimeoffset NULL,
        CONSTRAINT [PK_DonationEvents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DonationEvents_BloodGroups_BloodGroupId] FOREIGN KEY ([BloodGroupId]) REFERENCES [BloodGroups] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DonationEvents_ComponentTypes_ComponentTypeId] FOREIGN KEY ([ComponentTypeId]) REFERENCES [ComponentTypes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DonationEvents_DonorProfiles_DonorId] FOREIGN KEY ([DonorId]) REFERENCES [DonorProfiles] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DonationEvents_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Locations] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE TABLE [BloodInventories] (
        [Id] int NOT NULL IDENTITY,
        [QuantityUnits] int NOT NULL,
        [ExpirationDate] datetimeoffset NOT NULL,
        [Status] nvarchar(450) NOT NULL,
        [InventorySource] nvarchar(max) NOT NULL,
        [BloodGroupId] uniqueidentifier NOT NULL,
        [ComponentTypeId] uniqueidentifier NOT NULL,
        [DonationEventId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_BloodInventories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BloodInventories_BloodGroups_BloodGroupId] FOREIGN KEY ([BloodGroupId]) REFERENCES [BloodGroups] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BloodInventories_ComponentTypes_ComponentTypeId] FOREIGN KEY ([ComponentTypeId]) REFERENCES [ComponentTypes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BloodInventories_DonationEvents_DonationEventId] FOREIGN KEY ([DonationEventId]) REFERENCES [DonationEvents] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE TABLE [RequestMatches] (
        [Id] uniqueidentifier NOT NULL,
        [MatchDate] datetimeoffset NOT NULL,
        [UnitsAssigned] int NOT NULL,
        [RequestId] uniqueidentifier NOT NULL,
        [EmergencyRequestId] uniqueidentifier NOT NULL,
        [DonationEventId] uniqueidentifier NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [LastUpdatedBy] nvarchar(max) NULL,
        [CreatedTime] datetimeoffset NULL,
        [LastUpdatedTime] datetimeoffset NULL,
        [DeletedTime] datetimeoffset NULL,
        CONSTRAINT [PK_RequestMatches] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RequestMatches_BloodRequests_RequestId] FOREIGN KEY ([RequestId]) REFERENCES [BloodRequests] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RequestMatches_DonationEvents_DonationEventId] FOREIGN KEY ([DonationEventId]) REFERENCES [DonationEvents] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RequestMatches_EmergencyRequests_EmergencyRequestId] FOREIGN KEY ([EmergencyRequestId]) REFERENCES [EmergencyRequests] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BlogPosts_AuthorId] ON [BlogPosts] ([AuthorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BloodGroups_GroupName] ON [BloodGroups] ([GroupName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BloodInventories_BloodGroupId_ComponentTypeId_ExpirationDate_Status] ON [BloodInventories] ([BloodGroupId], [ComponentTypeId], [ExpirationDate], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BloodInventories_ComponentTypeId] ON [BloodInventories] ([ComponentTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BloodInventories_DonationEventId] ON [BloodInventories] ([DonationEventId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BloodRequests_BloodGroupId_ComponentTypeId_Status] ON [BloodRequests] ([BloodGroupId], [ComponentTypeId], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BloodRequests_ComponentTypeId] ON [BloodRequests] ([ComponentTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BloodRequests_LocationId] ON [BloodRequests] ([LocationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BloodRequests_RequestedBy] ON [BloodRequests] ([RequestedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ComponentTypes_Name] ON [ComponentTypes] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Documents_CreatedBy] ON [Documents] ([CreatedBy]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DonationEvents_BloodGroupId] ON [DonationEvents] ([BloodGroupId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DonationEvents_ComponentTypeId] ON [DonationEvents] ([ComponentTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DonationEvents_DonorId] ON [DonationEvents] ([DonorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DonationEvents_LocationId] ON [DonationEvents] ([LocationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DonorProfiles_BloodGroupId] ON [DonorProfiles] ([BloodGroupId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DonorProfiles_UserId] ON [DonorProfiles] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EmergencyRequests_BloodGroupId] ON [EmergencyRequests] ([BloodGroupId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EmergencyRequests_ComponentTypeId] ON [EmergencyRequests] ([ComponentTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Locations_Name] ON [Locations] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RequestMatches_DonationEventId] ON [RequestMatches] ([DonationEventId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RequestMatches_EmergencyRequestId] ON [RequestMatches] ([EmergencyRequestId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RequestMatches_RequestId] ON [RequestMatches] ([RequestId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Roles_RoleName] ON [Roles] ([RoleName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Users_RoleId] ON [Users] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250603034354_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250603034354_InitialCreate', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250606041134_AddDonorProfileAvailabilityFields'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'UserName');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Users] ALTER COLUMN [UserName] nvarchar(450) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250606041134_AddDonorProfileAvailabilityFields'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Email');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Users] ALTER COLUMN [Email] nvarchar(450) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250606041134_AddDonorProfileAvailabilityFields'
)
BEGIN
    ALTER TABLE [DonorProfiles] ADD [IsAvailableForEmergency] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250606041134_AddDonorProfileAvailabilityFields'
)
BEGIN
    ALTER TABLE [DonorProfiles] ADD [NextAvailableDonationDate] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250606041134_AddDonorProfileAvailabilityFields'
)
BEGIN
    ALTER TABLE [DonorProfiles] ADD [PreferredDonationTime] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250606041134_AddDonorProfileAvailabilityFields'
)
BEGIN
    CREATE TABLE [RefreshTokens] (
        [Id] uniqueidentifier NOT NULL,
        [Token] nvarchar(450) NOT NULL,
        [ExpiryDate] datetimeoffset NOT NULL,
        [IsUsed] bit NOT NULL,
        [IsRevoked] bit NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RefreshTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250606041134_AddDonorProfileAvailabilityFields'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250606041134_AddDonorProfileAvailabilityFields'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_UserName] ON [Users] ([UserName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250606041134_AddDonorProfileAvailabilityFields'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RefreshTokens_Token] ON [RefreshTokens] ([Token]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250606041134_AddDonorProfileAvailabilityFields'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250606041134_AddDonorProfileAvailabilityFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250606041134_AddDonorProfileAvailabilityFields', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250612112834_AddNullableNavigationProperties'
)
BEGIN
    DROP TABLE [RequestMatches];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250612112834_AddNullableNavigationProperties'
)
BEGIN
    ALTER TABLE [Notifications] ADD [ReferenceId] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250612112834_AddNullableNavigationProperties'
)
BEGIN
    ALTER TABLE [Notifications] ADD [Title] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250612112834_AddNullableNavigationProperties'
)
BEGIN
    ALTER TABLE [EmergencyRequests] ADD [Address] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250612112834_AddNullableNavigationProperties'
)
BEGIN
    ALTER TABLE [EmergencyRequests] ADD [HospitalName] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250612112834_AddNullableNavigationProperties'
)
BEGIN
    ALTER TABLE [EmergencyRequests] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250612112834_AddNullableNavigationProperties'
)
BEGIN
    ALTER TABLE [EmergencyRequests] ADD [Latitude] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250612112834_AddNullableNavigationProperties'
)
BEGIN
    ALTER TABLE [EmergencyRequests] ADD [LocationId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250612112834_AddNullableNavigationProperties'
)
BEGIN
    ALTER TABLE [EmergencyRequests] ADD [Longitude] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250612112834_AddNullableNavigationProperties'
)
BEGIN
    ALTER TABLE [EmergencyRequests] ADD [MedicalNotes] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250612112834_AddNullableNavigationProperties'
)
BEGIN
    CREATE TABLE [BloodDonationWorkflows] (
        [Id] uniqueidentifier NOT NULL,
        [RequestId] uniqueidentifier NOT NULL,
        [RequestType] nvarchar(50) NOT NULL,
        [DonorId] uniqueidentifier NULL,
        [BloodGroupId] uniqueidentifier NOT NULL,
        [ComponentTypeId] uniqueidentifier NOT NULL,
        [InventoryId] int NULL,
        [Status] nvarchar(50) NOT NULL,
        [StatusDescription] nvarchar(255) NOT NULL,
        [AppointmentDate] datetimeoffset NULL,
        [AppointmentLocation] nvarchar(255) NOT NULL,
        [AppointmentConfirmed] bit NOT NULL,
        [DonationDate] datetimeoffset NULL,
        [DonationLocation] nvarchar(255) NOT NULL,
        [QuantityDonated] float NULL,
        [CreatedTime] datetimeoffset NOT NULL,
        [LastUpdatedTime] datetimeoffset NULL,
        [CompletedTime] datetimeoffset NULL,
        [DeletedTime] datetimeoffset NULL,
        [Notes] nvarchar(1000) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_BloodDonationWorkflows] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BloodDonationWorkflows_BloodGroups_BloodGroupId] FOREIGN KEY ([BloodGroupId]) REFERENCES [BloodGroups] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BloodDonationWorkflows_BloodInventories_InventoryId] FOREIGN KEY ([InventoryId]) REFERENCES [BloodInventories] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BloodDonationWorkflows_ComponentTypes_ComponentTypeId] FOREIGN KEY ([ComponentTypeId]) REFERENCES [ComponentTypes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BloodDonationWorkflows_DonorProfiles_DonorId] FOREIGN KEY ([DonorId]) REFERENCES [DonorProfiles] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250612112834_AddNullableNavigationProperties'
)
BEGIN
    CREATE TABLE [DonorReminderSettings] (
        [Id] uniqueidentifier NOT NULL,
        [DonorProfileId] uniqueidentifier NOT NULL,
        [EnableReminders] bit NOT NULL,
        [DaysBeforeEligible] int NOT NULL,
        [EmailNotifications] bit NOT NULL,
        [InAppNotifications] bit NOT NULL,
        [CreatedTime] datetimeoffset NOT NULL,
        [LastUpdatedTime] datetimeoffset NULL,
        [LastReminderSentTime] datetimeoffset NULL,
        CONSTRAINT [PK_DonorReminderSettings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DonorReminderSettings_DonorProfiles_DonorProfileId] FOREIGN KEY ([DonorProfileId]) REFERENCES [DonorProfiles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250612112834_AddNullableNavigationProperties'
)
BEGIN
    CREATE INDEX [IX_EmergencyRequests_LocationId] ON [EmergencyRequests] ([LocationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250612112834_AddNullableNavigationProperties'
)
BEGIN
    CREATE INDEX [IX_BloodDonationWorkflows_BloodGroupId_ComponentTypeId] ON [BloodDonationWorkflows] ([BloodGroupId], [ComponentTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250612112834_AddNullableNavigationProperties'
)
BEGIN
    CREATE INDEX [IX_BloodDonationWorkflows_ComponentTypeId] ON [BloodDonationWorkflows] ([ComponentTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250612112834_AddNullableNavigationProperties'
)
BEGIN
    CREATE INDEX [IX_BloodDonationWorkflows_DonorId] ON [BloodDonationWorkflows] ([DonorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250612112834_AddNullableNavigationProperties'
)
BEGIN
    CREATE INDEX [IX_BloodDonationWorkflows_InventoryId] ON [BloodDonationWorkflows] ([InventoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250612112834_AddNullableNavigationProperties'
)
BEGIN
    CREATE INDEX [IX_BloodDonationWorkflows_RequestId] ON [BloodDonationWorkflows] ([RequestId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250612112834_AddNullableNavigationProperties'
)
BEGIN
    CREATE INDEX [IX_BloodDonationWorkflows_Status_IsActive] ON [BloodDonationWorkflows] ([Status], [IsActive]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250612112834_AddNullableNavigationProperties'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DonorReminderSettings_DonorProfileId] ON [DonorReminderSettings] ([DonorProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250612112834_AddNullableNavigationProperties'
)
BEGIN
    CREATE INDEX [IX_DonorReminderSettings_EnableReminders_LastReminderSentTime] ON [DonorReminderSettings] ([EnableReminders], [LastReminderSentTime]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250612112834_AddNullableNavigationProperties'
)
BEGIN
    ALTER TABLE [EmergencyRequests] ADD CONSTRAINT [FK_EmergencyRequests_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Locations] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250612112834_AddNullableNavigationProperties'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250612112834_AddNullableNavigationProperties', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250625081014_NullableDonationDates'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DonorProfiles]') AND [c].[name] = N'LastDonationDate');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [DonorProfiles] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [DonorProfiles] ALTER COLUMN [LastDonationDate] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250625081014_NullableDonationDates'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250625081014_NullableDonationDates', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626035659_AddDonationAppointmentRequest'
)
BEGIN
    CREATE TABLE [DonationAppointmentRequests] (
        [Id] uniqueidentifier NOT NULL,
        [DonorId] uniqueidentifier NOT NULL,
        [PreferredDate] datetimeoffset NOT NULL,
        [PreferredTimeSlot] nvarchar(max) NOT NULL,
        [LocationId] uniqueidentifier NOT NULL,
        [BloodGroupId] uniqueidentifier NULL,
        [ComponentTypeId] uniqueidentifier NULL,
        [RequestType] nvarchar(450) NOT NULL,
        [InitiatedByUserId] uniqueidentifier NULL,
        [Status] nvarchar(450) NOT NULL,
        [Notes] nvarchar(max) NULL,
        [RejectionReason] nvarchar(max) NULL,
        [ReviewedByUserId] uniqueidentifier NULL,
        [ReviewedAt] datetimeoffset NULL,
        [ConfirmedDate] datetimeoffset NULL,
        [ConfirmedTimeSlot] nvarchar(max) NULL,
        [ConfirmedLocationId] uniqueidentifier NULL,
        [DonorAccepted] bit NULL,
        [DonorResponseAt] datetimeoffset NULL,
        [DonorResponseNotes] nvarchar(max) NULL,
        [WorkflowId] uniqueidentifier NULL,
        [IsUrgent] bit NOT NULL,
        [Priority] int NOT NULL,
        [ExpiresAt] datetimeoffset NULL,
        [CreatedBy] nvarchar(max) NULL,
        [LastUpdatedBy] nvarchar(max) NULL,
        [CreatedTime] datetimeoffset NULL,
        [LastUpdatedTime] datetimeoffset NULL,
        [DeletedTime] datetimeoffset NULL,
        CONSTRAINT [PK_DonationAppointmentRequests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DonationAppointmentRequests_BloodDonationWorkflows_WorkflowId] FOREIGN KEY ([WorkflowId]) REFERENCES [BloodDonationWorkflows] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DonationAppointmentRequests_BloodGroups_BloodGroupId] FOREIGN KEY ([BloodGroupId]) REFERENCES [BloodGroups] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DonationAppointmentRequests_ComponentTypes_ComponentTypeId] FOREIGN KEY ([ComponentTypeId]) REFERENCES [ComponentTypes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DonationAppointmentRequests_DonorProfiles_DonorId] FOREIGN KEY ([DonorId]) REFERENCES [DonorProfiles] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DonationAppointmentRequests_Locations_ConfirmedLocationId] FOREIGN KEY ([ConfirmedLocationId]) REFERENCES [Locations] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DonationAppointmentRequests_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Locations] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DonationAppointmentRequests_Users_InitiatedByUserId] FOREIGN KEY ([InitiatedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DonationAppointmentRequests_Users_ReviewedByUserId] FOREIGN KEY ([ReviewedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626035659_AddDonationAppointmentRequest'
)
BEGIN
    CREATE INDEX [IX_DonationAppointmentRequests_BloodGroupId] ON [DonationAppointmentRequests] ([BloodGroupId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626035659_AddDonationAppointmentRequest'
)
BEGIN
    CREATE INDEX [IX_DonationAppointmentRequests_ComponentTypeId] ON [DonationAppointmentRequests] ([ComponentTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626035659_AddDonationAppointmentRequest'
)
BEGIN
    CREATE INDEX [IX_DonationAppointmentRequests_ConfirmedLocationId] ON [DonationAppointmentRequests] ([ConfirmedLocationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626035659_AddDonationAppointmentRequest'
)
BEGIN
    CREATE INDEX [IX_DonationAppointmentRequests_DonorId] ON [DonationAppointmentRequests] ([DonorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626035659_AddDonationAppointmentRequest'
)
BEGIN
    CREATE INDEX [IX_DonationAppointmentRequests_ExpiresAt] ON [DonationAppointmentRequests] ([ExpiresAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626035659_AddDonationAppointmentRequest'
)
BEGIN
    CREATE INDEX [IX_DonationAppointmentRequests_InitiatedByUserId] ON [DonationAppointmentRequests] ([InitiatedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626035659_AddDonationAppointmentRequest'
)
BEGIN
    CREATE INDEX [IX_DonationAppointmentRequests_IsUrgent_Priority] ON [DonationAppointmentRequests] ([IsUrgent], [Priority]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626035659_AddDonationAppointmentRequest'
)
BEGIN
    CREATE INDEX [IX_DonationAppointmentRequests_LocationId_PreferredDate] ON [DonationAppointmentRequests] ([LocationId], [PreferredDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626035659_AddDonationAppointmentRequest'
)
BEGIN
    CREATE INDEX [IX_DonationAppointmentRequests_ReviewedByUserId] ON [DonationAppointmentRequests] ([ReviewedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626035659_AddDonationAppointmentRequest'
)
BEGIN
    CREATE INDEX [IX_DonationAppointmentRequests_Status_RequestType] ON [DonationAppointmentRequests] ([Status], [RequestType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626035659_AddDonationAppointmentRequest'
)
BEGIN
    CREATE INDEX [IX_DonationAppointmentRequests_WorkflowId] ON [DonationAppointmentRequests] ([WorkflowId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626035659_AddDonationAppointmentRequest'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250626035659_AddDonationAppointmentRequest', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626053210_AddLocationManagement'
)
BEGIN
    ALTER TABLE [Locations] ADD [ContactEmail] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626053210_AddLocationManagement'
)
BEGIN
    ALTER TABLE [Locations] ADD [ContactPhone] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626053210_AddLocationManagement'
)
BEGIN
    ALTER TABLE [Locations] ADD [CreatedBy] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626053210_AddLocationManagement'
)
BEGIN
    ALTER TABLE [Locations] ADD [CreatedTime] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626053210_AddLocationManagement'
)
BEGIN
    ALTER TABLE [Locations] ADD [DeletedTime] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626053210_AddLocationManagement'
)
BEGIN
    ALTER TABLE [Locations] ADD [Description] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626053210_AddLocationManagement'
)
BEGIN
    ALTER TABLE [Locations] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626053210_AddLocationManagement'
)
BEGIN
    ALTER TABLE [Locations] ADD [LastUpdatedBy] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626053210_AddLocationManagement'
)
BEGIN
    ALTER TABLE [Locations] ADD [LastUpdatedTime] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626053210_AddLocationManagement'
)
BEGIN
    CREATE TABLE [LocationCapacities] (
        [Id] uniqueidentifier NOT NULL,
        [LocationId] uniqueidentifier NOT NULL,
        [TimeSlot] nvarchar(450) NOT NULL,
        [TotalCapacity] int NOT NULL,
        [DayOfWeek] int NULL,
        [EffectiveDate] datetimeoffset NULL,
        [ExpiryDate] datetimeoffset NULL,
        [IsActive] bit NOT NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedBy] nvarchar(max) NULL,
        [LastUpdatedBy] nvarchar(max) NULL,
        [CreatedTime] datetimeoffset NULL,
        [LastUpdatedTime] datetimeoffset NULL,
        [DeletedTime] datetimeoffset NULL,
        CONSTRAINT [PK_LocationCapacities] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_LocationCapacity_TotalCapacity] CHECK (TotalCapacity >= 0),
        CONSTRAINT [FK_LocationCapacities_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Locations] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626053210_AddLocationManagement'
)
BEGIN
    CREATE TABLE [LocationOperatingHours] (
        [Id] uniqueidentifier NOT NULL,
        [LocationId] uniqueidentifier NOT NULL,
        [DayOfWeek] int NOT NULL,
        [MorningStartTime] time NOT NULL,
        [MorningEndTime] time NOT NULL,
        [AfternoonStartTime] time NOT NULL,
        [AfternoonEndTime] time NOT NULL,
        [EveningStartTime] time NULL,
        [EveningEndTime] time NULL,
        [IsClosed] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedBy] nvarchar(max) NULL,
        [LastUpdatedBy] nvarchar(max) NULL,
        [CreatedTime] datetimeoffset NULL,
        [LastUpdatedTime] datetimeoffset NULL,
        [DeletedTime] datetimeoffset NULL,
        CONSTRAINT [PK_LocationOperatingHours] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LocationOperatingHours_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Locations] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626053210_AddLocationManagement'
)
BEGIN
    CREATE TABLE [LocationStaffAssignments] (
        [Id] uniqueidentifier NOT NULL,
        [LocationId] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Role] nvarchar(max) NOT NULL,
        [CanManageCapacity] bit NOT NULL,
        [CanApproveAppointments] bit NOT NULL,
        [CanViewReports] bit NOT NULL,
        [AssignedDate] datetimeoffset NOT NULL,
        [UnassignedDate] datetimeoffset NULL,
        [IsActive] bit NOT NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedBy] nvarchar(max) NULL,
        [LastUpdatedBy] nvarchar(max) NULL,
        [CreatedTime] datetimeoffset NULL,
        [LastUpdatedTime] datetimeoffset NULL,
        [DeletedTime] datetimeoffset NULL,
        CONSTRAINT [PK_LocationStaffAssignments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LocationStaffAssignments_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Locations] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_LocationStaffAssignments_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626053210_AddLocationManagement'
)
BEGIN
    CREATE INDEX [IX_LocationCapacities_EffectiveDate_ExpiryDate_IsActive] ON [LocationCapacities] ([EffectiveDate], [ExpiryDate], [IsActive]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626053210_AddLocationManagement'
)
BEGIN
    CREATE INDEX [IX_LocationCapacities_LocationId_TimeSlot_DayOfWeek] ON [LocationCapacities] ([LocationId], [TimeSlot], [DayOfWeek]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626053210_AddLocationManagement'
)
BEGIN
    CREATE UNIQUE INDEX [IX_LocationOperatingHours_LocationId_DayOfWeek] ON [LocationOperatingHours] ([LocationId], [DayOfWeek]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626053210_AddLocationManagement'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_LocationStaffAssignments_LocationId_UserId] ON [LocationStaffAssignments] ([LocationId], [UserId]) WHERE IsActive = 1');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626053210_AddLocationManagement'
)
BEGIN
    CREATE INDEX [IX_LocationStaffAssignments_LocationId_UserId_IsActive] ON [LocationStaffAssignments] ([LocationId], [UserId], [IsActive]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626053210_AddLocationManagement'
)
BEGIN
    CREATE INDEX [IX_LocationStaffAssignments_UserId_IsActive] ON [LocationStaffAssignments] ([UserId], [IsActive]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626053210_AddLocationManagement'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250626053210_AddLocationManagement', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250626053941_UpdateLocationDtos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250626053941_UpdateLocationDtos', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250627085609_UpdateUserCreatedTime'
)
BEGIN
    ALTER TABLE [Users] ADD [CreatedTime] datetimeoffset NOT NULL DEFAULT '0001-01-01T00:00:00.0000000+00:00';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250627085609_UpdateUserCreatedTime'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250627085609_UpdateUserCreatedTime', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250627091450_AddUserIsActivated'
)
BEGIN
    ALTER TABLE [Users] ADD [IsActivated] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250627091450_AddUserIsActivated'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250627091450_AddUserIsActivated', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250627104102_RemoveLocationOperatingHours'
)
BEGIN
    DROP TABLE [LocationOperatingHours];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250627104102_RemoveLocationOperatingHours'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250627104102_RemoveLocationOperatingHours', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250630131345_AddCheckInAndStatusTimesToDonationAppointmentRequest'
)
BEGIN
    ALTER TABLE [DonationAppointmentRequests] ADD [CancelledTime] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250630131345_AddCheckInAndStatusTimesToDonationAppointmentRequest'
)
BEGIN
    ALTER TABLE [DonationAppointmentRequests] ADD [CheckInTime] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250630131345_AddCheckInAndStatusTimesToDonationAppointmentRequest'
)
BEGIN
    ALTER TABLE [DonationAppointmentRequests] ADD [CompletedTime] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250630131345_AddCheckInAndStatusTimesToDonationAppointmentRequest'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250630131345_AddCheckInAndStatusTimesToDonationAppointmentRequest', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702062151_RemoveBloodDonationWorkflow'
)
BEGIN
    ALTER TABLE [DonationAppointmentRequests] DROP CONSTRAINT [FK_DonationAppointmentRequests_BloodDonationWorkflows_WorkflowId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702062151_RemoveBloodDonationWorkflow'
)
BEGIN
    DROP TABLE [BloodDonationWorkflows];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702062151_RemoveBloodDonationWorkflow'
)
BEGIN
    DROP INDEX [IX_DonationAppointmentRequests_WorkflowId] ON [DonationAppointmentRequests];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702062151_RemoveBloodDonationWorkflow'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DonationAppointmentRequests]') AND [c].[name] = N'WorkflowId');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [DonationAppointmentRequests] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [DonationAppointmentRequests] DROP COLUMN [WorkflowId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702062151_RemoveBloodDonationWorkflow'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DonationEvents]') AND [c].[name] = N'Status');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [DonationEvents] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [DonationEvents] ALTER COLUMN [Status] nvarchar(50) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702062151_RemoveBloodDonationWorkflow'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DonationEvents]') AND [c].[name] = N'DonorId');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [DonationEvents] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [DonationEvents] ALTER COLUMN [DonorId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702062151_RemoveBloodDonationWorkflow'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DonationEvents]') AND [c].[name] = N'CollectedAt');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [DonationEvents] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [DonationEvents] ALTER COLUMN [CollectedAt] nvarchar(255) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702062151_RemoveBloodDonationWorkflow'
)
BEGIN
    ALTER TABLE [DonationEvents] ADD [AppointmentConfirmed] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702062151_RemoveBloodDonationWorkflow'
)
BEGIN
    ALTER TABLE [DonationEvents] ADD [AppointmentDate] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702062151_RemoveBloodDonationWorkflow'
)
BEGIN
    ALTER TABLE [DonationEvents] ADD [AppointmentLocation] nvarchar(255) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702062151_RemoveBloodDonationWorkflow'
)
BEGIN
    ALTER TABLE [DonationEvents] ADD [CompletedTime] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702062151_RemoveBloodDonationWorkflow'
)
BEGIN
    ALTER TABLE [DonationEvents] ADD [DonationDate] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702062151_RemoveBloodDonationWorkflow'
)
BEGIN
    ALTER TABLE [DonationEvents] ADD [DonationLocation] nvarchar(255) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702062151_RemoveBloodDonationWorkflow'
)
BEGIN
    ALTER TABLE [DonationEvents] ADD [InventoryId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702062151_RemoveBloodDonationWorkflow'
)
BEGIN
    ALTER TABLE [DonationEvents] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702062151_RemoveBloodDonationWorkflow'
)
BEGIN
    ALTER TABLE [DonationEvents] ADD [Notes] nvarchar(1000) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702062151_RemoveBloodDonationWorkflow'
)
BEGIN
    ALTER TABLE [DonationEvents] ADD [QuantityDonated] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702062151_RemoveBloodDonationWorkflow'
)
BEGIN
    ALTER TABLE [DonationEvents] ADD [RequestId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702062151_RemoveBloodDonationWorkflow'
)
BEGIN
    ALTER TABLE [DonationEvents] ADD [RequestType] nvarchar(50) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702062151_RemoveBloodDonationWorkflow'
)
BEGIN
    ALTER TABLE [DonationEvents] ADD [StaffId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702062151_RemoveBloodDonationWorkflow'
)
BEGIN
    ALTER TABLE [DonationEvents] ADD [StatusDescription] nvarchar(255) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702062151_RemoveBloodDonationWorkflow'
)
BEGIN
    CREATE INDEX [IX_DonationEvents_InventoryId] ON [DonationEvents] ([InventoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702062151_RemoveBloodDonationWorkflow'
)
BEGIN
    CREATE INDEX [IX_DonationEvents_StaffId] ON [DonationEvents] ([StaffId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702062151_RemoveBloodDonationWorkflow'
)
BEGIN
    ALTER TABLE [DonationEvents] ADD CONSTRAINT [FK_DonationEvents_BloodInventories_InventoryId] FOREIGN KEY ([InventoryId]) REFERENCES [BloodInventories] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702062151_RemoveBloodDonationWorkflow'
)
BEGIN
    ALTER TABLE [DonationEvents] ADD CONSTRAINT [FK_DonationEvents_Users_StaffId] FOREIGN KEY ([StaffId]) REFERENCES [Users] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702062151_RemoveBloodDonationWorkflow'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250702062151_RemoveBloodDonationWorkflow', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702062610_YourNextMigration'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250702062610_YourNextMigration', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702081519_UpdateDonationEventDtos'
)
BEGIN
    ALTER TABLE [DonationEvents] ADD [ActionTaken] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702081519_UpdateDonationEventDtos'
)
BEGIN
    ALTER TABLE [DonationEvents] ADD [BloodPressure] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702081519_UpdateDonationEventDtos'
)
BEGIN
    ALTER TABLE [DonationEvents] ADD [CheckInTime] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702081519_UpdateDonationEventDtos'
)
BEGIN
    ALTER TABLE [DonationEvents] ADD [ComplicationDetails] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702081519_UpdateDonationEventDtos'
)
BEGIN
    ALTER TABLE [DonationEvents] ADD [ComplicationType] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702081519_UpdateDonationEventDtos'
)
BEGIN
    ALTER TABLE [DonationEvents] ADD [DonationStartTime] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702081519_UpdateDonationEventDtos'
)
BEGIN
    ALTER TABLE [DonationEvents] ADD [Height] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702081519_UpdateDonationEventDtos'
)
BEGIN
    ALTER TABLE [DonationEvents] ADD [HemoglobinLevel] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702081519_UpdateDonationEventDtos'
)
BEGIN
    ALTER TABLE [DonationEvents] ADD [IsUsable] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702081519_UpdateDonationEventDtos'
)
BEGIN
    ALTER TABLE [DonationEvents] ADD [MedicalNotes] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702081519_UpdateDonationEventDtos'
)
BEGIN
    ALTER TABLE [DonationEvents] ADD [RejectionReason] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702081519_UpdateDonationEventDtos'
)
BEGIN
    ALTER TABLE [DonationEvents] ADD [Temperature] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702081519_UpdateDonationEventDtos'
)
BEGIN
    ALTER TABLE [DonationEvents] ADD [Weight] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702081519_UpdateDonationEventDtos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250702081519_UpdateDonationEventDtos', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702191432_AddDonationEventFields'
)
BEGIN
    DROP INDEX [IX_Users_Email] ON [Users];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702191432_AddDonationEventFields'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Email');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [Users] ALTER COLUMN [Email] nvarchar(450) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702191432_AddDonationEventFields'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DonorProfiles]') AND [c].[name] = N'Address');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [DonorProfiles] DROP CONSTRAINT [' + @var8 + '];');
    ALTER TABLE [DonorProfiles] ALTER COLUMN [Address] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702191432_AddDonationEventFields'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]) WHERE [Email] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250702191432_AddDonationEventFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250702191432_AddDonationEventFields', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703045856_MergeEmergencyRequestToBloodRequest'
)
BEGIN
    DROP TABLE [EmergencyRequests];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703045856_MergeEmergencyRequestToBloodRequest'
)
BEGIN
    DROP INDEX [IX_BloodRequests_BloodGroupId_ComponentTypeId_Status] ON [BloodRequests];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703045856_MergeEmergencyRequestToBloodRequest'
)
BEGIN
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BloodRequests]') AND [c].[name] = N'RequestedBy');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [BloodRequests] DROP CONSTRAINT [' + @var9 + '];');
    ALTER TABLE [BloodRequests] ALTER COLUMN [RequestedBy] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703045856_MergeEmergencyRequestToBloodRequest'
)
BEGIN
    DECLARE @var10 sysname;
    SELECT @var10 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BloodRequests]') AND [c].[name] = N'NeededByDate');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [BloodRequests] DROP CONSTRAINT [' + @var10 + '];');
    ALTER TABLE [BloodRequests] ALTER COLUMN [NeededByDate] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703045856_MergeEmergencyRequestToBloodRequest'
)
BEGIN
    ALTER TABLE [BloodRequests] ADD [Address] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703045856_MergeEmergencyRequestToBloodRequest'
)
BEGIN
    ALTER TABLE [BloodRequests] ADD [ContactInfo] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703045856_MergeEmergencyRequestToBloodRequest'
)
BEGIN
    ALTER TABLE [BloodRequests] ADD [CreatedBy] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703045856_MergeEmergencyRequestToBloodRequest'
)
BEGIN
    ALTER TABLE [BloodRequests] ADD [CreatedTime] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703045856_MergeEmergencyRequestToBloodRequest'
)
BEGIN
    ALTER TABLE [BloodRequests] ADD [DeletedTime] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703045856_MergeEmergencyRequestToBloodRequest'
)
BEGIN
    ALTER TABLE [BloodRequests] ADD [HospitalName] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703045856_MergeEmergencyRequestToBloodRequest'
)
BEGIN
    ALTER TABLE [BloodRequests] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703045856_MergeEmergencyRequestToBloodRequest'
)
BEGIN
    ALTER TABLE [BloodRequests] ADD [IsEmergency] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703045856_MergeEmergencyRequestToBloodRequest'
)
BEGIN
    ALTER TABLE [BloodRequests] ADD [LastUpdatedBy] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703045856_MergeEmergencyRequestToBloodRequest'
)
BEGIN
    ALTER TABLE [BloodRequests] ADD [LastUpdatedTime] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703045856_MergeEmergencyRequestToBloodRequest'
)
BEGIN
    ALTER TABLE [BloodRequests] ADD [Latitude] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703045856_MergeEmergencyRequestToBloodRequest'
)
BEGIN
    ALTER TABLE [BloodRequests] ADD [Longitude] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703045856_MergeEmergencyRequestToBloodRequest'
)
BEGIN
    ALTER TABLE [BloodRequests] ADD [MedicalNotes] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703045856_MergeEmergencyRequestToBloodRequest'
)
BEGIN
    ALTER TABLE [BloodRequests] ADD [PatientName] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703045856_MergeEmergencyRequestToBloodRequest'
)
BEGIN
    ALTER TABLE [BloodRequests] ADD [UrgencyLevel] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703045856_MergeEmergencyRequestToBloodRequest'
)
BEGIN
    CREATE INDEX [IX_BloodRequests_BloodGroupId_ComponentTypeId_Status_IsEmergency] ON [BloodRequests] ([BloodGroupId], [ComponentTypeId], [Status], [IsEmergency]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703045856_MergeEmergencyRequestToBloodRequest'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250703045856_MergeEmergencyRequestToBloodRequest', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250704051146_RemoveInventoryIdFromDonationEvent'
)
BEGIN
    ALTER TABLE [DonationEvents] DROP CONSTRAINT [FK_DonationEvents_BloodInventories_InventoryId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250704051146_RemoveInventoryIdFromDonationEvent'
)
BEGIN
    DROP INDEX [IX_DonationEvents_InventoryId] ON [DonationEvents];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250704051146_RemoveInventoryIdFromDonationEvent'
)
BEGIN
    DECLARE @var11 sysname;
    SELECT @var11 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DonationEvents]') AND [c].[name] = N'InventoryId');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [DonationEvents] DROP CONSTRAINT [' + @var11 + '];');
    ALTER TABLE [DonationEvents] DROP COLUMN [InventoryId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250704051146_RemoveInventoryIdFromDonationEvent'
)
BEGIN
    ALTER TABLE [BloodRequests] ADD [FulfilledByStaffId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250704051146_RemoveInventoryIdFromDonationEvent'
)
BEGIN
    ALTER TABLE [BloodRequests] ADD [FulfilledDate] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250704051146_RemoveInventoryIdFromDonationEvent'
)
BEGIN
    ALTER TABLE [BloodRequests] ADD [IsPickedUp] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250704051146_RemoveInventoryIdFromDonationEvent'
)
BEGIN
    ALTER TABLE [BloodRequests] ADD [PickupDate] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250704051146_RemoveInventoryIdFromDonationEvent'
)
BEGIN
    ALTER TABLE [BloodRequests] ADD [PickupNotes] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250704051146_RemoveInventoryIdFromDonationEvent'
)
BEGIN
    ALTER TABLE [BloodInventories] ADD [FulfilledDate] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250704051146_RemoveInventoryIdFromDonationEvent'
)
BEGIN
    ALTER TABLE [BloodInventories] ADD [FulfilledRequestId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250704051146_RemoveInventoryIdFromDonationEvent'
)
BEGIN
    ALTER TABLE [BloodInventories] ADD [FulfillmentNotes] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250704051146_RemoveInventoryIdFromDonationEvent'
)
BEGIN
    CREATE INDEX [IX_BloodInventories_FulfilledRequestId] ON [BloodInventories] ([FulfilledRequestId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250704051146_RemoveInventoryIdFromDonationEvent'
)
BEGIN
    ALTER TABLE [BloodInventories] ADD CONSTRAINT [FK_BloodInventories_BloodRequests_FulfilledRequestId] FOREIGN KEY ([FulfilledRequestId]) REFERENCES [BloodRequests] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250704051146_RemoveInventoryIdFromDonationEvent'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250704051146_RemoveInventoryIdFromDonationEvent', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250704052130_UpdateBloodRequestFulfillmentFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250704052130_UpdateBloodRequestFulfillmentFields', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250705053801_AddRelatedBloodRequestToAppointment'
)
BEGIN
    ALTER TABLE [DonationAppointmentRequests] ADD [RelatedBloodRequestId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250705053801_AddRelatedBloodRequestToAppointment'
)
BEGIN
    CREATE INDEX [IX_DonationAppointmentRequests_RelatedBloodRequestId] ON [DonationAppointmentRequests] ([RelatedBloodRequestId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250705053801_AddRelatedBloodRequestToAppointment'
)
BEGIN
    ALTER TABLE [DonationAppointmentRequests] ADD CONSTRAINT [FK_DonationAppointmentRequests_BloodRequests_RelatedBloodRequestId] FOREIGN KEY ([RelatedBloodRequestId]) REFERENCES [BloodRequests] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250705053801_AddRelatedBloodRequestToAppointment'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250705053801_AddRelatedBloodRequestToAppointment', N'8.0.0');
END;
GO

COMMIT;
GO

