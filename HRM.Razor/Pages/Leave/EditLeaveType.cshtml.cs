using HRM.Business.DTOs.Leaves;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.Leave
{
    public class EditLeaveTypePageModel : PageModel
    {
        private readonly ILeaveApiClient _leaveApiClient;

        public EditLeaveTypePageModel(ILeaveApiClient leaveApiClient)
        {
            _leaveApiClient = leaveApiClient;
        }

        [BindProperty]
        public UpdateLeaveTypeRequestDto Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var response = await _leaveApiClient.GetLeaveTypesAsync();
            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            var type = response.Data?.FirstOrDefault(t => t.LeaveTypeId == id);
            if (type == null) return NotFound();

            Input = new UpdateLeaveTypeRequestDto
            {
                LeaveTypeName = type.LeaveTypeName,
                MaxDaysPerYear = type.MaxDaysPerYear,
                Description = type.Description,
                IsActive = type.IsActive
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
                return Page();

            var response = await _leaveApiClient.UpdateLeaveTypeAsync(id, Input);
            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (response.Success)
                return RedirectToPage("LeaveTypes");

            ModelState.AddModelError("", response.Message ?? "Failed to update leave type.");
            return Page();
        }
    }
}
