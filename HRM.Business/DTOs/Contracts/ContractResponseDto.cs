namespace HRM.Business.DTOs.Contracts
{
    public class ContractResponseDto
    {
        public int ContractId { get; set; }

        public int EmployeeId { get; set; }

        public string? EmployeeCode { get; set; }

        public string? EmployeeName { get; set; }

        public string ContractType { get; set; } = string.Empty;

        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public decimal Salary { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? ContractFileUrl { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}