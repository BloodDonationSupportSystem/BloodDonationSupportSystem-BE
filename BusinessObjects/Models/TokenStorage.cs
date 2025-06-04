using System;
using System.Collections.Generic;
using System.Linq;

namespace BusinessObjects.Models
{
    // Simple in-memory storage for tokens that would normally be stored in the database
    public static class TokenStorage
    {
        private static readonly Dictionary<Guid, string> _emailVerificationTokens = new();
        private static readonly Dictionary<Guid, bool> _emailVerificationStatus = new();
        private static readonly Dictionary<Guid, string> _passwordResetTokens = new();
        private static readonly Dictionary<Guid, DateTimeOffset> _passwordResetExpiry = new();
        
        // Refresh token storage
        private static readonly Dictionary<string, RefreshTokenInfo> _refreshTokens = new();

        // Email verification methods
        public static void SetEmailVerificationToken(Guid userId, string token)
        {
            _emailVerificationTokens[userId] = token;
            _emailVerificationStatus[userId] = false;
        }

        public static string GetEmailVerificationToken(Guid userId)
        {
            return _emailVerificationTokens.TryGetValue(userId, out var token) ? token : null;
        }

        public static void SetEmailVerified(Guid userId)
        {
            _emailVerificationStatus[userId] = true;
            _emailVerificationTokens.Remove(userId);
        }

        public static bool IsEmailVerified(Guid userId)
        {
            return _emailVerificationStatus.TryGetValue(userId, out var verified) && verified;
        }

        // Password reset methods
        public static void SetPasswordResetToken(Guid userId, string token, TimeSpan expiry)
        {
            _passwordResetTokens[userId] = token;
            _passwordResetExpiry[userId] = DateTimeOffset.UtcNow.Add(expiry);
        }

        public static string GetPasswordResetToken(Guid userId)
        {
            return _passwordResetTokens.TryGetValue(userId, out var token) ? token : null;
        }

        public static bool ValidatePasswordResetToken(Guid userId, string token)
        {
            if (!_passwordResetTokens.TryGetValue(userId, out var storedToken) || storedToken != token)
                return false;

            if (!_passwordResetExpiry.TryGetValue(userId, out var expiry) || expiry < DateTimeOffset.UtcNow)
                return false;

            return true;
        }

        public static void ClearPasswordResetToken(Guid userId)
        {
            _passwordResetTokens.Remove(userId);
            _passwordResetExpiry.Remove(userId);
        }
        
        // Refresh token methods
        public static void StoreRefreshToken(string token, Guid userId, DateTimeOffset expiryDate)
        {
            _refreshTokens[token] = new RefreshTokenInfo
            {
                UserId = userId,
                ExpiryDate = expiryDate,
                IsUsed = false,
                IsRevoked = false
            };
        }
        
        public static RefreshTokenInfo GetRefreshToken(string token)
        {
            return _refreshTokens.TryGetValue(token, out var tokenInfo) ? tokenInfo : null;
        }
        
        public static void MarkRefreshTokenAsUsed(string token)
        {
            if (_refreshTokens.TryGetValue(token, out var tokenInfo))
            {
                tokenInfo.IsUsed = true;
            }
        }
        
        public static void RevokeRefreshToken(string token)
        {
            if (_refreshTokens.TryGetValue(token, out var tokenInfo))
            {
                tokenInfo.IsRevoked = true;
            }
        }
        
        // Helper class for refresh token info
        public class RefreshTokenInfo
        {
            public Guid UserId { get; set; }
            public DateTimeOffset ExpiryDate { get; set; }
            public bool IsUsed { get; set; }
            public bool IsRevoked { get; set; }
        }
    }
}