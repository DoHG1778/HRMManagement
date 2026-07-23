using HRM.Business.DTOs.Leaves;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.Leave
{
    public class EditModel : PageModel
    {
        private readonly ILeaveApiClient _leaveApiClient;

        public EditModel(ILeaveApiClient leaveApiClient)
        {
            _leaveApiClient = leaveApiClient;
        }

        [BindProperty]
        public UpdateLeaveRequestDto Input { get; set; } = new();

        public List<LeaveTypeResponseDto> LeaveTypes { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var response = await _leaveApiClient.GetMyLeaveRequestsAsync(pageSize: 100);
            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            var request = response.Data?.Items.FirstOrDefault(r => r.LeaveRequestId == id);
            if (request == null)
                return NotFound();

            if (request.Status != "PENDING")
            {
                TempData["ErrorMessage"] = "Only PENDING requests can be edited.";
                return RedirectToPage("Index");
            }

            Input = new UpdateLeaveRequestDto
            {
                LeaveTypeId = request.LeaveTypeId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Reason = request.Reason
            };

            await LoadLeaveTypes();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                await LoadLeaveTypes();
                return Page();
            }

            var response = await _leaveApiClient.UpdateLeaveRequestAsync(id, Input);
            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (response.Success)
                return RedirectToPage("Index");

            ModelState.AddModelError("", response.Message ?? "Failed to update leave request.");
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
