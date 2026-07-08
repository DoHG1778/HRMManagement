namespace HRM.Business.Common
{
    public class CurrentUser
    {
        public int UserId { get; set; }

        public int? EmployeeId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public List<string> Roles { get; set; } = new();

        public bool IsAuthenticated
        {
            get
            {
                return UserId > 0;
            }
        }

        public bool IsInRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return false;
            }

            return Roles.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
        }

        public bool HasAnyRole(params string[] roles)
        {
            if (roles == null || roles.Length == 0)
            {
                return false;
            }

            return roles.Any(IsInRole);
        }

        public bool IsAdmin()
        {
            return IsInRole("Admin");
        }

        public bool IsHr()
        {
            return IsInRole("HR");
        }

        public bool IsManager()
        {
            return IsInRole("Manager");
        }

        public bool IsPayroll()
        {
            return IsInRole("Payroll");
        }

        public bool IsEmployee()
        {
            return IsInRole("Employee");
        }
    }
}