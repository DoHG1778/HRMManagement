using HRM.Business.DTOs.Leaves;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.Leave
{
    public class SetBalancePageModel : PageModel
    {
        private readonly ILeaveApiClient _leaveApiClient;

        public SetBalancePageModel(ILeaveApiClient leaveApiClient)
        {
            _leaveApiClient = leaveApiClient;
        }

        [BindProperty]
        public SetLeaveBalanceRequestDto Input { get; set; } = new();

        public List<LeaveTypeResponseDto> LeaveTypes { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int employeeId, int leaveTypeId, int year)
        {
            await LoadLeaveTypes();

            var balanceResponse = await _leaveApiClient.GetLeaveBalancesAsync(employeeId, year);
            var existing = balanceResponse.Data?.FirstOrDefault(b => b.LeaveTypeId == leaveTypeId);

            Input = new SetLeaveBalanceRequestDto
            {
                EmployeeId = employeeId,
                LeaveTypeId = leaveTypeId,
                Year = year,
                TotalDays = existing?.TotalDays ?? 0,
                UsedDays = existing?.UsedDays ?? 0
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadLeaveTypes();
                return Page();
            }

            var response = await _leaveApiClient.SetLeaveBalanceAsync(Input);
            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (response.Success)
                return RedirectToPage("Balances", new { employeeId = Input.EmployeeId, year = Input.Year });

            ModelState.AddModelError("", response.Message ?? "Failed to set balance.");
            await LoadLeaveTypes();
            return Page();
        }

        private async Task LoadLeaveTypes()
        {
            var response = await _leaveApiClient.GetLeaveTypesAsync();
            LeaveTypes = response.Data ?? new List<LeaveTypeResponseDto>();
        }
    }
}
