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
        public const string AppointmentCheckIn = "AppointmentCheckIn";
        public const string AppointmentExpiring = "AppointmentExpiring";
        public const string AppointmentModified = "AppointmentModified";
        public const string AppointmentStaffAssigned = "AppointmentStaffAssigned";
        public const string AppointmentDonorResponse = "AppointmentDonorResponse";
        public const string AppointmentConfirmationRequest = "AppointmentConfirmationRequest";
        public const string AppointmentRescheduled = "AppointmentRescheduled";
        public const string AppointmentCompleted = "AppointmentCompleted";
        
        // Donation related notifications
        public const string DonationCompleted = "DonationCompleted";
        public const string NextDonationDate = "NextDonationDate";
        public const string DonationStarted = "DonationStarted";
        public const string DonationCancelled = "DonationCancelled";
        public const string DonationAssigned = "DonationAssigned";
        public const string DonationScheduled = "DonationScheduled";
        public const string DonationComplication = "DonationComplication";
        public const string DonationIncomplete = "DonationIncomplete";
        public const string DonationEventUpdate = "DonationEventUpdate";
        public const string WalkInDonation = "WalkInDonation";
        public const string DonationProcessUpdate = "DonationProcessUpdate";
        public const string DonationEligibilityCheck = "DonationEligibilityCheck";
        
        // Health check related notifications
        public const string HealthCheckPassed = "HealthCheckPassed";
        public const string HealthCheckFailed = "HealthCheckFailed";
        public const string BloodGroupUpdate = "BloodGroupUpdate";
        public const string HealthScreeningRequired = "HealthScreeningRequired";
        public const string MedicalClearanceNeeded = "MedicalClearanceNeeded";
        
        // Post-donation care notifications
        public const string PostDonationCare = "PostDonationCare";
        public const string DonationRestReminder = "DonationRestReminder";
        public const string NextDonationReminder = "NextDonationReminder";
        public const string DonationEligibilityAlert = "DonationEligibilityAlert";
        public const string PostDonationFollowUp = "PostDonationFollowUp";
        public const string DonationCertificate = "DonationCertificate";
        
        // Blood Request related notifications
        public const string BloodRequestUpdate = "BloodRequestUpdate";
        public const string BloodRequestCreated = "BloodRequestCreated";
        public const string BloodRequestApproved = "BloodRequestApproved";
        public const string BloodRequestFulfilled = "BloodRequestFulfilled";
        public const string BloodRequestCancelled = "BloodRequestCancelled";
        public const string BloodRequestExpiring = "BloodRequestExpiring";
        public const string BloodRequestProcessing = "BloodRequestProcessing";
        public const string BloodRequestMatchFound = "BloodRequestMatchFound";
        public const string BloodRequestPartiallyFulfilled = "BloodRequestPartiallyFulfilled";
        
        // Emergency request related notifications
        public const string EmergencyRequest = "EmergencyRequest";
        public const string EmergencyRequestApproval = "EmergencyRequestApproval";
        public const string EmergencyRequestFulfilled = "EmergencyRequestFulfilled";
        public const string EmergencyRequestUrgent = "EmergencyRequestUrgent";
        public const string EmergencyRequestCancelled = "EmergencyRequestCancelled";
        public const string EmergencyRequestExpiring = "EmergencyRequestExpiring";
        public const string EmergencyRequestEscalated = "EmergencyRequestEscalated";
        public const string EmergencyRequestAssigned = "EmergencyRequestAssigned";
        
        // Inventory related notifications
        public const string BloodInventoryLow = "BloodInventoryLow";
        public const string BloodInventoryExpiring = "BloodInventoryExpiring";
        public const string BloodInventoryReceived = "BloodInventoryReceived";
        public const string BloodInventoryAllocated = "BloodInventoryAllocated";
        public const string BloodInventoryCritical = "BloodInventoryCritical";
        public const string BloodInventoryRestocked = "BloodInventoryRestocked";
        public const string BloodInventoryExpired = "BloodInventoryExpired";
        
        // System and admin notifications
        public const string SystemMaintenance = "SystemMaintenance";
        public const string PolicyUpdate = "PolicyUpdate";
        public const string NewsUpdate = "NewsUpdate";
        public const string ProfileUpdate = "ProfileUpdate";
        public const string SecurityAlert = "SecurityAlert";
        public const string SystemAlert = "SystemAlert";
        public const string MaintenanceScheduled = "MaintenanceScheduled";
        public const string DataBackupComplete = "DataBackupComplete";
        
        // Location and staff notifications
        public const string LocationCapacityAlert = "LocationCapacityAlert";
        public const string StaffAssignment = "StaffAssignment";
        public const string LocationUpdate = "LocationUpdate";
        public const string StaffScheduleUpdate = "StaffScheduleUpdate";
        public const string LocationClosed = "LocationClosed";
        public const string CapacityWarning = "CapacityWarning";
        
        // Communication and reminders
        public const string WelcomeMessage = "WelcomeMessage";
        public const string ThankYouMessage = "ThankYouMessage";
        public const string FeedbackRequest = "FeedbackRequest";
        public const string SurveyInvitation = "SurveyInvitation";
        public const string CommunityUpdate = "CommunityUpdate";
        public const string GeneralReminder = "GeneralReminder";
        public const string ImportantAnnouncement = "ImportantAnnouncement";
        public const string HolidayNotice = "HolidayNotice";
        
        // New detailed notification types
        public const string DetailedAppointmentUpdate = "DetailedAppointmentUpdate";
        public const string DetailedDonationUpdate = "DetailedDonationUpdate";
        public const string DetailedRequestUpdate = "DetailedRequestUpdate";
        public const string ComprehensiveStatusUpdate = "ComprehensiveStatusUpdate";
    }
}