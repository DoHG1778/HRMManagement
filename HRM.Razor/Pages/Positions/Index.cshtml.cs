using HRM.Razor.Models.ViewModels;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.Positions
{
    [Authorize(Roles = "Admin,HR,HR Staff,System Administrator")]
    public class IndexModel : PageModel
    {
        private readonly IPositionApiClient _positionApiClient;

        public IndexModel(IPositionApiClient positionApiClient)
        {
            _positionApiClient = positionApiClient;
        }

        public List<PositionModel> Positions { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? IsActive { get; set; }

        // Statistics
        public int TotalPositions { get; set; }
        public int ActivePositions { get; set; }
        public int InactivePositions { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["ActivePage"] = "Positions";

            var response = await _positionApiClient.GetPositionsAsync(IsActive);

            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (response.StatusCode == 403)
            {
                return RedirectToPage("/Account/AccessDenied");
            }

            if (response.Success && response.Data != null)
            {
                var allList = response.Data;

                TotalPositions = allList.Count;
                ActivePositions = allList.Count(p => p.IsActive);
                InactivePositions = allList.Count(p => !p.IsActive);

                if (!string.IsNullOrWhiteSpace(Keyword))
                {
                    var kw = Keyword.Trim().ToLower();
                    Positions = allList
                        .Where(p => p.PositionName.ToLower().Contains(kw) || (p.Description != null && p.Description.ToLower().Contains(kw)))
                        .ToList();
                }
                else
                {
                    Positions = allList;
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int id, bool targetIsActive)
        {
            if (id <= 0)
            {
                return RedirectToPage("/Positions/Index");
            }

            var response = targetIsActive
                ? await _positionApiClient.ActivatePositionAsync(id)
                : await _positionApiClient.DeactivatePositionAsync(id);

            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.Message ?? "Không thể thay đổi trạng thái chức vụ.";
            }
            else
            {
                TempData["SuccessMessage"] = targetIsActive
                    ? "Đã kích hoạt lại chức vụ thành công!"
                    : "Đã chuyển chức vụ sang trạng thái ngừng sử dụng.";
            }

            return RedirectToPage("/Positions/Index");
        }
    }
}
