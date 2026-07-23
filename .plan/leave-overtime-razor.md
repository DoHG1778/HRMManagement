# Plan: Implement Leave & Overtime cho HRM.Razor

## Mục tiêu
Port功能 Leave và Overtime từ HRM.MVC sang HRM.Razor (Razor Pages), giữ nguyên MVC.

## Tổng quan thay đổi

Tạo **13 Razor Pages** (26 file `.cshtml` + `.cshtml.cs`), **2 API Client interfaces + implementations**, **2 ViewModel files**. Sửa **Program.cs** (DI) và **_Layout.cshtml** (sidebar nav).

---

## Phần 1: ViewModels mới (`HRM.Razor/Models/ViewModels/`)

### 1.1 `LeaveRequestViewModel.cs`
```csharp
public class LeaveRequestListModel {
    int LeaveRequestId; string? EmployeeCode, EmployeeName, LeaveTypeName, Reason, Status;
    DateOnly StartDate, EndDate; int TotalDays;
    string? ApprovedByName, RejectionReason; DateTime CreatedAt;
}
public class CreateLeaveRequestModel {
    int LeaveTypeId; DateOnly StartDate, EndDate; string? Reason;
}
public class EditLeaveRequestModel : CreateLeaveRequestModel {}
public class LeaveTypeModel {
    int LeaveTypeId, MaxDaysPerYear; string LeaveTypeName; string? Description; bool IsActive;
}
public class CreateLeaveTypeModel {
    string LeaveTypeName; int MaxDaysPerYear; string? Description;
}
public class EditLeaveTypeModel : CreateLeaveTypeModel {}
public class LeaveBalanceModel {
    int LeaveBalanceId, EmployeeId, LeaveTypeId, Year, TotalDays, UsedDays, RemainingDays;
    string? EmployeeCode, EmployeeName, LeaveTypeName;
}
public class SetLeaveBalanceModel {
    int EmployeeId, LeaveTypeId, Year, TotalDays, UsedDays;
}
public class ApproveLeaveModel {
    bool IsApproved; string? RejectionReason;
}
```

### 1.2 `OvertimeRequestViewModel.cs`
```csharp
public class OvertimeRequestListModel {
    int Otid; string? EmployeeCode, EmployeeName, Reason, Status;
    DateOnly Otdate; DateTime StartTime, EndTime;
    decimal TotalHours; bool IsTransferredToPayroll; DateTime CreatedAt;
}
public class CreateOvertimeRequestModel {
    DateOnly Otdate; DateTime StartTime, EndTime; string Reason;
}
public class EditOvertimeRequestModel : CreateOvertimeRequestModel {}
public class ApproveOvertimeModel {
    bool IsApproved; string? RejectionReason;
}
```

---

## Phần 2: API Clients (`HRM.Razor/Services/`)

### 2.1 `Interfaces/ILeaveApiClient.cs`
Methods:
- `GetMyLeaveRequestsAsync(month?, year?, status?, page, size)` → `ApiResponse<PagedResult<LeaveRequestListModel>>`
- `GetPendingLeaveRequestsAsync()` → `ApiResponse<List<LeaveRequestListModel>>`
- `GetLeaveTypesAsync(isActive?)` → `ApiResponse<List<LeaveTypeModel>>`
- `CreateLeaveRequestAsync(model)` → `ApiResponse<LeaveRequestListModel>`
- `UpdateLeaveRequestAsync(id, model)` → `ApiResponse<LeaveRequestListModel>`
- `CancelLeaveRequestAsync(id)` → `ApiResponse<bool>`
- `ApproveLeaveRequestAsync(id, model)` → `ApiResponse<LeaveRequestListModel>`
- `CreateLeaveTypeAsync(model)` → `ApiResponse<LeaveTypeModel>`
- `UpdateLeaveTypeAsync(id, model)` → `ApiResponse<LeaveTypeModel>`
- `DeactivateLeaveTypeAsync(id)` → `ApiResponse<bool>`
- `ActivateLeaveTypeAsync(id)` → `ApiResponse<bool>`
- `GetLeaveBalancesAsync(employeeId, year?)` → `ApiResponse<List<LeaveBalanceModel>>`
- `SetLeaveBalanceAsync(model)` → `ApiResponse<LeaveBalanceModel>`

### 2.2 `ApiClients/LeaveApiClient.cs`
Triển khai `ILeaveApiClient`, gọi REST endpoints:
- `api/leaves/requests/me?...`
- `api/leaves/requests/pending`
- `api/leaves/types?isActive=...`
- `api/leaves/requests` (POST)
- `api/leaves/requests/{id}` (PUT)
- `api/leaves/requests/{id}/cancel` (PUT)
- `api/leaves/requests/{id}/approval` (PUT)
- `api/leaves/types` (POST)
- `api/leaves/types/{id}` (PUT)
- `api/leaves/types/{id}/deactivate` (PUT)
- `api/leaves/types/{id}/activate` (PUT)
- `api/leaves/balances/employee/{eid}?year=...`
- `api/leaves/balances` (POST)

### 2.3 `Interfaces/IOvertimeApiClient.cs`
Methods:
- `GetMyOvertimeRequestsAsync(month?, year?, status?, page, size)` → `ApiResponse<PagedResult<OvertimeRequestListModel>>`
- `GetPendingOvertimeRequestsAsync()` → `ApiResponse<List<OvertimeRequestListModel>>`
- `CreateOvertimeRequestAsync(model)` → `ApiResponse<OvertimeRequestListModel>`
- `UpdateOvertimeRequestAsync(id, model)` → `ApiResponse<OvertimeRequestListModel>`
- `CancelOvertimeRequestAsync(id)` → `ApiResponse<bool>`
- `ApproveOvertimeRequestAsync(id, model)` → `ApiResponse<OvertimeRequestListModel>`

### 2.4 `ApiClients/OvertimeApiClient.cs`
Triển khai `IOvertimeApiClient`, gọi REST endpoints:
- `api/overtimes/requests/me?...`
- `api/overtimes/requests/pending`
- `api/overtimes/requests` (POST)
- `api/overtimes/requests/{id}` (PUT)
- `api/overtimes/requests/{id}/cancel` (PUT)
- `api/overtimes/requests/{id}/approval` (PUT)

---

## Phần 3: Razor Pages

### Staff Leave (3 pages)

#### 3.1 `Pages/Leave/Index.cshtml[.cs]` — Danh sách Leave Request của Staff
- `@page` + `Layout = "_Layout"`
- `ViewData["ActivePage"] = "MyLeaveRequests"`
- `OnGetAsync(status?, month?, year?)` → gọi `GetMyLeaveRequestsAsync`
- Table với filter (status dropdown, month, year) + badge màu theo status
- Nút "Create New" → redirect page Create
- Nút Edit/Cancel (chỉ hiện khi PENDING)
- Cancel = form POST → `CancelLeaveRequestAsync` → redirect Index

#### 3.2 `Pages/Leave/Create.cshtml[.cs]` — Tạo Leave Request
- Form: select LeaveType (dropdown từ API), StartDate, EndDate, TotalDays (readonly, tính JS), Reason
- JS validate: date không quá khứ, EndDate >= StartDate, tính total days
- `OnGetAsync` load leave types dropdown
- `OnPostAsync` → `CreateLeaveRequestAsync` → redirect Index

#### 3.3 `Pages/Leave/Edit.cshtml[.cs]` — Sửa Leave Request (PENDING only)
- `@page "{id:int}"`
- Same form as Create
- `OnGetAsync(id)` load existing request + leave types
- `OnPostAsync(id)` → `UpdateLeaveRequestAsync` → redirect Index

### Staff Overtime (3 pages)

#### 3.4 `Pages/Overtime/Index.cshtml[.cs]` — Danh sách OT Request của Staff
- Same pattern as Leave/Index
- Filter: status, month, year
- Table columns: ID, Date, Start, End, Hours, Reason, Status, Actions

#### 3.5 `Pages/Overtime/Create.cshtml[.cs]` — Tạo OT Request
- Form: OTDate, StartTime (datetime-local), EndTime (datetime-local), TotalHours (readonly, tính JS), Reason
- JS validate: date không quá khứ, end > start, max 3 hours
- `OnPostAsync` → `CreateOvertimeRequestAsync` → redirect Index

#### 3.6 `Pages/Overtime/Edit.cshtml[.cs]` — Sửa OT Request (PENDING only)
- `@page "{id:int}"`
- Same form as Create
- `OnGetAsync(id)` load existing request
- `OnPostAsync(id)` → `UpdateOvertimeRequestAsync` → redirect Index

### Manager Leave (6 pages)

#### 3.7 `Pages/Leave/ManagerPending.cshtml[.cs]` — Duyệt Leave Requests
- `ViewData["ActivePage"] = "ManagerLeaveRequests"`
- Table: ID, Employee, LeaveType, StartDate, EndDate, Days, Reason, Actions
- Nút Approve (form POST) + nút Reject (show/hide rejection reason input)
- JS `showRejectForm(id)` toggle

#### 3.8 `Pages/Leave/LeaveTypes.cshtml[.cs]` — Quản lý Leave Types
- Table: ID, Name, MaxDays/Year, Description, Active, Actions
- Nút Create New → redirect CreateLeaveType
- Nút Edit → redirect EditLeaveType
- Nút Activate/Deactivate (form POST)

#### 3.9 `Pages/Leave/CreateLeaveType.cshtml[.cs]` — Tạo Leave Type
- Form: LeaveTypeName, MaxDaysPerYear, Description
- `OnPostAsync` → `CreateLeaveTypeAsync` → redirect LeaveTypes

#### 3.10 `Pages/Leave/EditLeaveType.cshtml[.cs]` — Sửa Leave Type
- `@page "{id:int}"`
- Form: LeaveTypeName, MaxDaysPerYear, Description
- `OnGetAsync(id)` load existing
- `OnPostAsync(id)` → `UpdateLeaveTypeAsync` → redirect LeaveTypes

#### 3.11 `Pages/Leave/Balances.cshtml[.cs]` — Xem Leave Balances
- Filter: EmployeeId, Year
- Table: LeaveType, Year, TotalDays, UsedDays, Remaining, Actions (Edit → SetBalance)

#### 3.12 `Pages/Leave/SetBalance.cshtml[.cs]` — Set Leave Balance
- `@page` với query params: employeeId, leaveTypeId, year
- Form: EmployeeId (readonly), LeaveType (dropdown), Year (readonly), TotalDays, UsedDays
- `OnGetAsync(employeeId, leaveTypeId, year)` load existing balance
- `OnPostAsync` → `SetLeaveBalanceAsync` → redirect Balances

### Manager Overtime (1 page)

#### 3.13 `Pages/Overtime/ManagerPending.cshtml[.cs]` — Duyệt OT Requests
- Same pattern as Leave/ManagerPending
- Table: ID, Employee, Date, Start, End, Hours, Reason, Actions

---

## Phần 4: Sửa files hiện có

### 4.1 `Program.cs` — Thêm DI registration
```csharp
builder.Services.AddScoped<ILeaveApiClient, LeaveApiClient>();
builder.Services.AddScoped<IOvertimeApiClient, OvertimeApiClient>();
```

### 4.2 `Pages/Shared/_Layout.cshtml` — Thêm sidebar nav links
Thêm vào section NGHỈ PHÉP:
- "Yêu cầu của tôi" → `/Leave` (Staff)
- "Duyệt yêu cầu" → `/Leave/ManagerPending` (Manager)
- "Loại phép" → `/Leave/LeaveTypes` (Manager)
- "Số dư phép" → `/Leave/Balances` (Manager)

Thêm vào section TĂNG CA:
- "Yêu cầu của tôi" → `/Overtime` (Staff)
- "Duyệt yêu cầu" → `/Overtime/ManagerPending` (Manager)

---

## Danh sách files

### Files mới (20 files):
1. `Models/ViewModels/LeaveRequestViewModel.cs`
2. `Models/ViewModels/OvertimeRequestViewModel.cs`
3. `Services/Interfaces/ILeaveApiClient.cs`
4. `Services/ApiClients/LeaveApiClient.cs`
5. `Services/Interfaces/IOvertimeApiClient.cs`
6. `Services/ApiClients/OvertimeApiClient.cs`
7-8. `Pages/Leave/Index.cshtml` + `.cshtml.cs`
9-10. `Pages/Leave/Create.cshtml` + `.cshtml.cs`
11-12. `Pages/Leave/Edit.cshtml` + `.cshtml.cs`
13-14. `Pages/Leave/ManagerPending.cshtml` + `.cshtml.cs`
15-16. `Pages/Leave/LeaveTypes.cshtml` + `.cshtml.cs`
17-18. `Pages/Leave/CreateLeaveType.cshtml` + `.cshtml.cs`
19-20. `Pages/Leave/EditLeaveType.cshtml` + `.cshtml.cs`
21-22. `Pages/Leave/Balances.cshtml` + `.cshtml.cs`
23-24. `Pages/Leave/SetBalance.cshtml` + `.cshtml.cs`
25-26. `Pages/Overtime/Index.cshtml` + `.cshtml.cs`
27-28. `Pages/Overtime/Create.cshtml` + `.cshtml.cs`
29-30. `Pages/Overtime/Edit.cshtml` + `.cshtml.cs`
31-32. `Pages/Overtime/ManagerPending.cshtml` + `.cshtml.cs`

### Files sửa (2 files):
- `Program.cs` (thêm 2 dòng DI)
- `Pages/Shared/_Layout.cshtml` (thêm sidebar links)

### Files KHÔNG sửa:
- MVC projects — giữ nguyên

---

## Key Design Decisions

1. **Separate page names cho Staff vs Manager** thay vì chung 1 page:
   - `Index` = Staff view, `ManagerPending` = Manager view
   - Tránh conflict `@page` routing trong cùng folder

2. **ViewModels riêng** thay vì dùng trực tiếp DTOs từ HRM.Business:
   - HRM.Razor không reference HRM.Business
   - Follow pattern hiện tại của Razor project

3. **API calls qua `IApiClient`** (Get/Post/Put/Delete):
   - Follow pattern `PositionApiClient` hiện tại
   - JWT token tự động gắn bởi `JwtAuthorizationHandler`

4. **Validate + JS inline** cho date/time calculations:
   - Leave: calculate total days
   - OT: calculate hours, max 3h validation
