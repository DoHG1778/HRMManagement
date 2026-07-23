using HRM.Business.DTOs.Leaves;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.Leave
{
    public class CreateModel : PageModel
    {
        private readonly ILeaveApiClient _leaveApiClient;

        public CreateModel(ILeaveApiClient leaveApiClient)
        {
            _leaveApiClient = leaveApiClient;
        }

        [BindProperty]
        public CreateLeaveRequestDto Input { get; set; } = new()
        {
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today)
        };

        public List<LeaveTypeResponseDto> LeaveTypes { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var response = await _leaveApiClient.GetLeaveTypesAsync(isActive: true);
            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }
            LeaveTypes = response.Data ?? new List<LeaveTypeResponseDto>();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadLeaveTypes();
                return Page();
            }

            var response = await _leaveApiClient.CreateLeaveRequestAsync(Input);
            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (response.Success)
                return RedirectToPage("Index");

            ModelState.AddModelError("", response.Message ?? "Failed to create leave request.");
            await LoadLeaveTypes();
            return Page();
        }

        private async Task LoadLeaveTypes()
        {
            var response = await _leaveApiClient.GetLeaveTypesAsync(isActive: true);
            LeaveTypes = response.Data ?? new List<LeaveTypeResponseDto>();
        }
    }
}
