using HRM.Business.DTOs.Leaves;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.Leave
{
    public class CreateLeaveTypePageModel : PageModel
    {
        private readonly ILeaveApiClient _leaveApiClient;

        public CreateLeaveTypePageModel(ILeaveApiClient leaveApiClient)
        {
            _leaveApiClient = leaveApiClient;
        }

        [BindProperty]
        public CreateLeaveTypeRequestDto Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var response = await _leaveApiClient.CreateLeaveTypeAsync(Input);
            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (response.Success)
                return RedirectToPage("LeaveTypes");

            ModelState.AddModelError("", response.Message ?? "Failed to create leave type.");
            return Page();
        }
    }
}
