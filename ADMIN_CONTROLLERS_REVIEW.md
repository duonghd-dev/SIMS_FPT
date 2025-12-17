# Rà Soát Admin Controllers

## 1. ClassController.cs ✅

### Tổng quan:

- **Status**: Hoạt động tốt sau fix errors
- **Chức năng chính**: Quản lý lớp học

### Chi tiết các action:

#### List() ✅

- Hiển thị danh sách tất cả lớp
- Sử dụng `_classService.GetAllClassesWithDetails()`
- **Tốt**: Lấy đầy đủ thông tin chi tiết cho display

#### Add (GET) ✅

- Load dropdown Departments
- Load tất cả Subjects (lọc theo department trong JS)
- Load tất cả Teachers với subjects đã gán
- Load tất cả Classes (để tính toán capacity)
- **Mới**: Thêm `ViewBag.DepartmentCapacity` để validation

#### Add (POST) ✅ (Vừa fix)

- ✅ Validate Class ID format (3-20 characters)
- ✅ Check duplicate Class ID
- ✅ **NEW**: Validate tổng học sinh không vượt capacity khoa
  - Kiểm tra: `totalStudents > dept.NumberOfStudents`
  - Error message rõ ràng: current used + available
- ✅ Call `_classService.AddClass()`
- **Fix gần đây**: Loại bỏ `??` từ int fields (không nullable)

#### Edit (GET) ✅

- Load existing class details
- Load departments + capacity info
- Loại trừ class hiện tại khỏi danh sách (vì Edit)
- **Tốt**: Tính toán capacity không bao gồm class đang edit

#### Edit (POST) ✅ (Vừa fix)

- ✅ Validate capacity tương tự Add
- ✅ Loại trừ class hiện tại (`c.ClassId != model.ClassId`)
- ✅ Call `_classService.UpdateClass()`
- **Fix gần đây**: Loại bỏ `??` từ int fields

#### Delete() ✅

- Xóa class qua service
- Return List

#### ManageStudents() ✅

- Quản lý học sinh trong lớp
- Load via `_classService.GetClassEnrollment()`

#### AddStudentsToClass() ✅

- Add multiple students to class
- Redirect về ManageStudents

#### RemoveStudentFromClass() ✅

- Remove 1 student from class
- Redirect về ManageStudents

#### LoadAllTeachersWithSubjects() (Private) ✅

- Helper method
- Load teachers + map subjects they teach
- Join subjects dựa trên TeacherIds trong Subject model

### Issues cần kiểm tra:

1. ⚠️ **CAPACITY VALIDATION**: Đang tính `NumberOfStudents` từ Department, không phải từ classes

   - `dept.NumberOfStudents` = Dung lượng khoa
   - Logic hiện tại: Tổng của tất cả classes <= Capacity
   - **Đúng rồi!**

2. ⚠️ **Null handling**:
   - `model.DepartmentId` - có thể null? Kiểm tra validate model
   - `viewModel.Class?.DepartmentId` - safe với null check

---

## 2. DepartmentController.cs ✅

### Tổng quan:

- **Status**: OK, nhưng cần check capacity logic
- **Chức năng**: Quản lý khoa

### Chi tiết:

#### List() ✅

- Hiển thị tất cả departments
- `_deptService.GetAllDepartments()`

#### Add (GET/POST) ✅

- Add department mới
- Basic validation via ModelState

#### Edit (GET/POST) ✅

- Edit department info
- Load teachers để set HeadOfDepartment

#### Detail() ✅

- Hiển thị chi tiết 1 khoa
- Load teachers + subjects trong khoa

#### Delete() ✅

- Xóa khoa

### Issues:

1. ⚠️ **Department.NumberOfStudents có ý nghĩa gì?**

   - Đây là **Dung lượng tối đa** của khoa
   - **Không phải** tổng current students
   - ClassController sử dụng đúng để validate capacity ✅

2. ⚠️ **Không có authorization check trên ClassController?**
   - DepartmentController có `[Authorize(Roles = "Admin")]`
   - **ClassController THIẾU!** - Cần thêm

---

## 3. StudentController.cs

Cần xem xét tiếp

---

## 4. SubjectController.cs

Cần xem xét tiếp

---

## 5. InstructorController.cs

Cần xem xét tiếp

---

## Issues Phát Hiện:

### 🔴 HIGH PRIORITY:

1. **ClassController thiếu [Authorize] attribute**

   - DepartmentController có
   - ClassController KHÔNG có
   - Cần thêm: `[Authorize(Roles = "Admin")]`

2. **ViewBag.DepartmentCapacity không được sử dụng trong View**
   - Add.cshtml/Edit.cshtml có JS sử dụng nó?
   - Cần verify JavaScript implementation

### 🟡 MEDIUM PRIORITY:

1. Null safety trên DepartmentId
2. Error handling khi department không tồn tại

### 🟢 LOW PRIORITY:

1. Thêm logging
2. Performance: GetAll() được gọi nhiều lần

---

## Recommendations:

### 1️⃣ Add Authorization to ClassController

```csharp
[Authorize(Roles = "Admin")]
[Area("Admin")]
public class ClassController : Controller
```

### 2️⃣ Improve Null Handling

```csharp
if (string.IsNullOrEmpty(model.DepartmentId))
{
    ModelState.AddModelError("DepartmentId", "Department is required");
}
```

### 3️⃣ Add Try-Catch for Service Calls

```csharp
try
{
    var (success, message) = _classService.AddClass(...);
    // handle
}
catch (Exception ex)
{
    ModelState.AddModelError("", "Error: " + ex.Message);
}
```

### 4️⃣ Refactor Duplicate Code

- Add POST has same ViewBag loading code as Edit POST
- Consider extracting to helper method

### 5️⃣ Performance

- Reduce multiple GetAll() calls
- Consider caching for dropdown data

---

## Summary:

- ✅ ClassController capacity validation logic chính xác
- ⚠️ Cần thêm [Authorize] attribute
- ⚠️ Cần verify View sử dụng ViewBag.DepartmentCapacity đúng cách
- ✅ Build hiện tại thành công (no errors)
