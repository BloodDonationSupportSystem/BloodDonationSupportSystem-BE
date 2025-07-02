namespace Shared.Constants
{
    public static class NotificationTypes
    {
        // Appointment related notifications
        public const string AppointmentUpdate = "AppointmentUpdate";
        public const string AppointmentCreated = "AppointmentCreated";
        public const string AppointmentApproved = "AppointmentApproved";
        public const string AppointmentRejected = "AppointmentRejected";
        public const string AppointmentCancelled = "AppointmentCancelled";
        public const string AppointmentReminder = "AppointmentReminder";
        
        // Donation related notifications
        public const string DonationCompleted = "DonationCompleted";
        public const string NextDonationDate = "NextDonationDate";
        public const string DonationCancelled = "DonationCancelled";
        public const string DonationAssigned = "DonationAssigned";
        public const string DonationRestReminder = "DonationRestReminder";
        
        // Blood Request related notifications
        public const string BloodRequestUpdate = "BloodRequestUpdate";
        public const string BloodRequestCreated = "BloodRequestCreated";
        public const string BloodRequestFulfilled = "BloodRequestFulfilled";
        public const string BloodRequestCancelled = "BloodRequestCancelled";
        
        // Emergency request related notifications
        public const string EmergencyRequest = "EmergencyRequest";
        public const string EmergencyRequestApproval = "EmergencyRequestApproval";
        public const string EmergencyRequestFulfilled = "EmergencyRequestFulfilled";
    }
}