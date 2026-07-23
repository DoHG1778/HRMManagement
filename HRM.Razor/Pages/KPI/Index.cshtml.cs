using HRM.Razor.Models;
using HRM.Razor.Models.ViewModels.Kpis;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.KPI
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IKpiApiClient _kpiApiClient;

        public IndexModel(IKpiApiClient kpiApiClient)
        {
            _kpiApiClient = kpiApiClient;
        }

        public PagedResultModel<KpiAssignmentViewModel> Assignments { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty]
        public int AssignmentId { get; set; }

        [BindProperty]
        public decimal ActualValue { get; set; }

        [BindProperty]
        public decimal ProgressPercent { get; set; }

        [BindProperty]
        public string? EmployeeComment { get; set; }

        [BindProperty]
        public decimal? EmployeeSelfScore { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["ActivePage"] = "KPI";

            var response =
                await _kpiApiClient.GetMyKpiAssignmentsAsync(
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
                var uniqueItems = response.Data.Items
                    .GroupBy(x => x.Kpiid)
                    .Select(x => x
                    .OrderByDescending(a => a.AssignmentId)
                        .First())
                    .ToList();

                response.Data.Items = uniqueItems;

                Assignments = response.Data;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateProgressAsync()
        {
            var model = new UpdateKpiProgressViewModel
            {
                ActualValue = ActualValue,
                ProgressPercent = ProgressPercent,
                EmployeeComment = EmployeeComment,
                EmployeeSelfScore = EmployeeSelfScore
            };

            var response = await _kpiApiClient.UpdateKpiProgressAsync(
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
                    "Cập nhật tiến độ KPI thành công.";
            }
            else
            {
                TempData["ErrorMessage"] =
                    response.Message ?? "Cập nhật tiến độ KPI thất bại.";
            }

            return RedirectToPage();
        }
    }
}