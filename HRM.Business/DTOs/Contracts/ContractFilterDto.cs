namespace HRM.Business.DTOs.Contracts
{
    public class ContractFilterDto
    {
        public int? EmployeeId { get; set; }

        public string? Status { get; set; }

        public string? ContractType { get; set; }

        public string? Keyword { get; set; }

        public DateOnly? FromDate { get; set; }

        public DateOnly? ToDate { get; set; }

        public bool? ExpiringSoon { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}