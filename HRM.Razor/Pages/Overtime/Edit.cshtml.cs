using HRM.Business.DTOs.Overtimes;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.Overtime
{
    public class EditModel : PageModel
    {
        private readonly IOvertimeApiClient _overtimeApiClient;

        public EditModel(IOvertimeApiClient overtimeApiClient)
        {
            _overtimeApiClient = overtimeApiClient;
        }

        [BindProperty]
        public UpdateOvertimeRequestDto Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var response = await _overtimeApiClient.GetMyOvertimeRequestsAsync(pageSize: 100);
            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            var request = response.Data?.Items.FirstOrDefault(r => r.Otid == id);
            if (request == null)
                return NotFound();

            if (request.Status != "PENDING")
            {
                TempData["ErrorMessage"] = "Only PENDING requests can be edited.";
                return RedirectToPage("Index");
            }

            Input = new UpdateOvertimeRequestDto
            {
                Otdate = request.Otdate,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Reason = request.Reason ?? string.Empty
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
                return Page();

            var response = await _overtimeApiClient.UpdateOvertimeRequestAsync(id, Input);
            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (response.Success)
                return RedirectToPage("Index");

            ModelState.AddModelError("", response.Message ?? "Failed to update overtime request.");
            return Page();
        }
    }
}
