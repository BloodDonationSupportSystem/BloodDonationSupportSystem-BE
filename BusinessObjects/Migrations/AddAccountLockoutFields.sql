-- Migration: Add Account Lockout Fields to Users table
-- Date: 2026-01-03
-- Description: Adds FailedLoginAttempts and LockoutEnd columns for account lockout security feature

-- Check if columns exist before adding
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'FailedLoginAttempts')
BEGIN
    ALTER TABLE [Users] ADD [FailedLoginAttempts] INT NOT NULL DEFAULT 0;
    PRINT 'Added FailedLoginAttempts column to Users table';
END
ELSE
BEGIN
    PRINT 'FailedLoginAttempts column already exists';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'LockoutEnd')
BEGIN
    ALTER TABLE [Users] ADD [LockoutEnd] DATETIMEOFFSET NULL;
    PRINT 'Added LockoutEnd column to Users table';
END
ELSE
BEGIN
    PRINT 'LockoutEnd column already exists';
END

GO

-- Create index for faster lockout queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_LockoutEnd' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE INDEX [IX_Users_LockoutEnd] ON [Users] ([LockoutEnd]);
    PRINT 'Created index IX_Users_LockoutEnd';
END
GO

PRINT 'Migration completed successfully';
