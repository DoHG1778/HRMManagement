using HRM.Business.Common;
using HRM.Razor.Models.ViewModels;
using HRM.Razor.Models.ViewModels.Payroll;
using HRM.Razor.Services.Interfaces;

namespace HRM.Razor.Services.ApiClients
{
    public class PayrollApiClient : IPayrollApiClient
    {
        private readonly IApiClient _apiClient;

        public PayrollApiClient(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<
    ApiResponse<
        HRM.Razor.Models.PagedResultModel<PayrollItemModel>
    >
>
GetPayrollsAsync(
    int? month = null,
    int? year = null,
    int? departmentId = null,
    int? employeeId = null,
    string? status = null,
    int pageNumber = 1,
    int pageSize = 10)
        {
            var queryParams = new List<string>();

            if (month.HasValue && month.Value > 0)
                queryParams.Add($"payrollMonth={month.Value}");

            if (year.HasValue && year.Value > 0)
                queryParams.Add($"payrollYear={year.Value}");

            if (departmentId.HasValue && departmentId.Value > 0)
                queryParams.Add($"departmentId={departmentId.Value}");

            if (employeeId.HasValue && employeeId.Value > 0)
                queryParams.Add($"employeeId={employeeId.Value}");

            if (!string.IsNullOrWhiteSpace(status))
                queryParams.Add(
                    $"status={Uri.EscapeDataString(status.Trim())}");

            queryParams.Add($"pageNumber={pageNumber}");
            queryParams.Add($"pageSize={pageSize}");

            var queryString = string.Join("&", queryParams);

            var endpoint = string.IsNullOrEmpty(queryString)
                ? "api/payrolls"
                : $"api/payrolls?{queryString}";

            return await _apiClient.GetAsync<
                HRM.Razor.Models.PagedResultModel<PayrollItemModel>
            >(endpoint);
        }

        public async Task<ApiResponse<PayrollItemModel>> GetPayrollByIdAsync(int payrollId)
        {
            return await _apiClient.GetAsync<PayrollItemModel>($"api/payrolls/{payrollId}");
        }

        public async Task<ApiResponse<List<PayrollItemModel>>> GenerateMonthlyPayrollAsync(GeneratePayrollViewModel model)
        {
            var requestData = new
            {
                PayrollMonth = model.PayrollMonth,
                PayrollYear = model.PayrollYear,
                DepartmentId = model.DepartmentId,
                EmployeeId = model.EmployeeId
            };
            return await _apiClient.PostAsync<List<PayrollItemModel>>("api/payrolls/generate", requestData);
        }

        public async Task<ApiResponse<List<PayrollItemModel>>> CalculatePayrollAsync(CalculatePayrollViewModel model)
        {
            var requestData = new
            {
                PayrollMonth = model.PayrollMonth,
                PayrollYear = model.PayrollYear,
                DepartmentId = model.DepartmentId,
                EmployeeId = model.EmployeeId,
                DefaultAllowance = model.DefaultAllowance,
                DefaultBonus = model.DefaultBonus,
                DefaultDeduction = model.DefaultDeduction,
                OvertimeCoefficient = model.OvertimeCoefficient,
                StandardWorkingDays = model.StandardWorkingDays,
                StandardWorkingHoursPerDay = model.StandardWorkingHoursPerDay
            };
            return await _apiClient.PostAsync<List<PayrollItemModel>>("api/payrolls/calculate", requestData);
        }

        public async Task<ApiResponse<PayrollItemModel>> UpdatePayrollDetailAsync(int payrollId, List<PayrollDetailItemModel> items)
        {
            var requestData = new
            {
                Items = items.Select(i => new
                {
                    ItemType = i.ItemType?.ToUpper() ?? "OTHER",
                    Description = i.Description?.Trim() ?? string.Empty,
                    Amount = i.Amount,
                    SourceType = i.SourceType,
                    SourceId = i.SourceId
                }).ToList()
            };
            return await _apiClient.PutAsync<PayrollItemModel>($"api/payrolls/{payrollId}/details", requestData);
        }

        public async Task<ApiResponse<bool>> ConfirmPayrollAsync(int month, int year, int? departmentId = null, int? employeeId = null)
        {
            var requestData = new
            {
                PayrollMonth = month,
                PayrollYear = year,
                DepartmentId = departmentId,
                EmployeeId = employeeId
            };
            return await _apiClient.PutAsync<bool>("api/payrolls/confirm", requestData);
        }

        public async Task<ApiResponse<PayrollItemModel>> GetMyPayslipAsync(int month, int year)
        {
            return await _apiClient.GetAsync<PayrollItemModel>($"api/payrolls/my-payslip?month={month}&year={year}");
        }

        public async Task<ApiResponse<PayrollReportViewModel>> GetPayrollReportAsync(int month, int year, int? departmentId = null, int? employeeId = null)
        {
            var queryParams = new List<string>
            {
                $"month={month}",
                $"year={year}"
            };
            if (departmentId.HasValue && departmentId.Value > 0) queryParams.Add($"departmentId={departmentId.Value}");
            if (employeeId.HasValue && employeeId.Value > 0) queryParams.Add($"employeeId={employeeId.Value}");

            var endpoint = $"api/payrolls/export?{string.Join("&", queryParams)}";
            return await _apiClient.GetAsync<PayrollReportViewModel>(endpoint);
        }
    }
}
