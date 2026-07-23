using HRM.Business.DTOs.Overtimes;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.Overtime
{
    public class CreateModel : PageModel
    {
        private readonly IOvertimeApiClient _overtimeApiClient;

        public CreateModel(IOvertimeApiClient overtimeApiClient)
        {
            _overtimeApiClient = overtimeApiClient;
        }

        [BindProperty]
        public CreateOvertimeRequestDto Input { get; set; } = new()
        {
            Otdate = DateOnly.FromDateTime(DateTime.Today),
            StartTime = DateTime.Today.AddHours(8),
            EndTime = DateTime.Today.AddHours(17)
        };

        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var response = await _overtimeApiClient.CreateOvertimeRequestAsync(Input);
            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (response.Success)
                return RedirectToPage("Index");

            ModelState.AddModelError("", response.Message ?? "Failed to create overtime request.");
            return Page();
        }
    }
}
