using Google.Cloud.Firestore;

namespace Models
{
    [FirestoreData]
    public class User
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty]
        public string email { get; set; } = string.Empty;

        [FirestoreProperty]
        public string name { get; set; } = string.Empty;

        [FirestoreProperty]
        public string role { get; set; } = string.Empty;

        [FirestoreProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    
    public class UserCreationRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class PasswordResetRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}