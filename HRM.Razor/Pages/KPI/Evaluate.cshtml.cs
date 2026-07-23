using HRM.Razor.Models;
using HRM.Razor.Models.ViewModels.Kpis;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.KPI
{
    [Authorize(Roles = "Manager,Department Manager,Admin,System Administrator")]
    public class EvaluateModel : PageModel
    {
        private readonly IKpiApiClient _kpiApiClient;

        public EvaluateModel(IKpiApiClient kpiApiClient)
        {
            _kpiApiClient = kpiApiClient;
        }

        public PagedResultModel<KpiAssignmentViewModel> Assignments { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty]
        public int AssignmentId { get; set; }

        [BindProperty]
        public decimal ManagerScore { get; set; }

        [BindProperty]
        public string? ManagerComment { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["ActivePage"] = "KPI";

            var response = await _kpiApiClient.GetKpiAssignmentsAsync(
                status: Status,
                pageNumber: 1,
                pageSize: 100
            );

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
                Assignments = response.Data;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostEvaluateAsync()
        {
            var model = new EvaluateKpiViewModel
            {
                ManagerScore = ManagerScore,
                ManagerComment = ManagerComment
            };

            var response = await _kpiApiClient.EvaluateKpiAsync(
                AssignmentId,
                model
            );

            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (response.StatusCode == 403)
            {
                return RedirectToPage("/Account/AccessDenied");
            }

            if (response.Success)
            {
                TempData["SuccessMessage"] =
                    "Đánh giá KPI thành công.";
            }
            else
            {
                TempData["ErrorMessage"] =
                    response.Message ?? "Đánh giá KPI thất bại.";
            }

            return RedirectToPage();
        }
    }
}