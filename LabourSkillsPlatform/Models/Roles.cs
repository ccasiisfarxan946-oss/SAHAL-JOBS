namespace LabourSkillsPlatform.Models
{
    /// <summary>
    /// Central place for the three RBAC role names (Chapter 3, 3.2.1 Design - RBAC).
    /// Avoids "magic strings" scattered across [Authorize(Roles = "...")] attributes.
    /// </summary>
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Employer = "Employer";
        public const string Labourer = "Labourer";
    }
}
