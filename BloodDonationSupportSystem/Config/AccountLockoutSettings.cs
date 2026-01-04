namespace BloodDonationSupportSystem.Config
{
    /// <summary>
    /// Configuration settings for account lockout to prevent brute force attacks
    /// </summary>
    public class AccountLockoutSettings
    {
        /// <summary>
        /// Maximum number of failed login attempts before account lockout
        /// </summary>
        public int MaxFailedAttempts { get; set; } = 5;

        /// <summary>
        /// Duration of account lockout in minutes
        /// </summary>
        public int LockoutDurationMinutes { get; set; } = 15;
    }
}
