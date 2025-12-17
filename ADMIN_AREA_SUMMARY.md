# Admin Area - Tóm Tắt Chi Tiết

## 📋 Tổng Quan Cấu Trúc

Admin Area (`Areas/Admin`) là module quản lý chính của ứng dụng SIMS, cho phép quản lý toàn bộ hệ thống giáo dục.

```
Areas/Admin/
├── Controllers/          (6 controllers)
├── Views/               (6 modules)
└── Shared/              (Layout & Components)
```

---

## 🎮 Controllers (6 Controllers)

### 1. **HomeController**

- **Chức năng**: Hiển thị Dashboard quản trị
- **Thuộc tính**: `[Authorize(Roles = "Admin")]`
- **Actions**:
  - `Dashboard()` - Thống kê tổng quan (sinh viên, khoa, giáo viên)

**Dữ liệu hiển thị**:

- Số lượng sinh viên (từ CSV)
- Số lượng khoa (từ CSV)
- Số lượng giáo viên (từ CSV)
- Biểu đồ giới tính sinh viên
- Danh sách 5 sinh viên mới nhất

---

### 2. **ClassController** ⭐ (FIXED)

- **Chức năng**: Quản lý lớp học
- **Repository**: `IClassRepository`, `ISubjectRepository`, `ITeacherRepository`
- **Actions**:
  - `List()` - Danh sách lớp với JOIN dữ liệu (FIX: `c.SubjectId` thay vì `c.SubjectName`)
  - `Add()` - Tạo lớp mới
  - `Edit()` - Chỉnh sửa lớp
  - `Delete()` - Xóa lớp
  - `ManageStudents()` - Quản lý sinh viên trong lớp

**Data Mapping** (List action):

```csharp
// JOIN lấy tên Môn học từ SubjectId
join s in subjects on c.SubjectId equals s.SubjectId
// JOIN lấy tên Giáo viên từ TeacherId
join t in teachers on c.TeacherName equals t.TeacherId
```

**FIX đã áp dụng**:

- ✅ Sửa join condition từ `c.SubjectName` → `c.SubjectId`
- ✅ Cập nhật error message hiển thị đúng trường

---

### 3. **StudentController**

- **Chức năng**: Quản lý sinh viên
- **Service**: `StudentService`
- **Actions**:
  - `List(string className)` - Danh sách sinh viên (hỗ trợ tìm kiếm)
  - `Add()` - Thêm sinh viên mới
  - `Edit()` - Chỉnh sửa thông tin
  - `Delete()` - Xóa sinh viên

**Tìm kiếm**: Hỗ trợ tìm theo ID hoặc Tên

---

### 4. **InstructorController** (Teachers)

- **Chức năng**: Quản lý giáo viên
- **Service**: `TeacherService`
- **Dependencies**: `ITeacherRepository`, `IDepartmentRepository`
- **Actions**:
  - `List()` - Danh sách giáo viên
  - `Add()` - Thêm giáo viên mới (Dropdown chọn Khoa)
  - `Edit()` - Chỉnh sửa thông tin
  - `DeleteTeacher()` - Xóa giáo viên

**Features**:

- Tích hợp Khoa (Department Dropdown)
- Validation trùng ID

---

### 5. **DepartmentController**

- **Chức năng**: Quản lý khoa/bộ môn
- **Dependencies**: `IDepartmentRepository`, `ITeacherRepository`, `ISubjectRepository`
- **Actions**:
  - `List()` - Danh sách khoa
  - `Add()` - Thêm khoa mới
  - `Edit()` - Chỉnh sửa
  - `Detail()` - Chi tiết khoa (Giáo viên, Môn học)
  - `Delete()` - Xóa khoa

**Features**:

- Hiển thị số lượng Giáo viên trong khoa
- Hiển thị số lượng Môn học trong khoa

---

### 6. **SubjectController**

- **Chức năng**: Quản lý môn học
- **Dependencies**: `ISubjectRepository`, `IDepartmentRepository`
- **Actions**:
  - `List()` - Danh sách môn học
  - `Add()` - Thêm môn mới (chọn Khoa)
  - `Edit()` - Chỉnh sửa
  - `Delete()` - Xóa môn

**Features**:

- Dropdown chọn Khoa khi thêm/sửa
- Validation trùng SubjectId

---

## 🎨 Views Module (6 Modules)

### 1. **Home/** - Dashboard

- `Dashboard.cshtml` - Trang chính quản trị
  - 4 thẻ thống kê (Students, Teachers, Departments, Revenue)
  - Biểu đồ doanh thu (Revenue Chart)
  - Biểu đồ giới tính sinh viên (Gender Chart)
  - Bảng 5 sinh viên mới nhất

---

### 2. **Class/** - Quản Lý Lớp

| File                    | Mục đích                                              |
| ----------------------- | ----------------------------------------------------- |
| `List.cshtml`           | Danh sách lớp (ID, Tên, Môn học, GV, Semester, Sĩ số) |
| `Add.cshtml`            | Form thêm lớp mới                                     |
| `Edit.cshtml`           | Form chỉnh sửa lớp                                    |
| `ManageStudents.cshtml` | Quản lý sinh viên trong lớp                           |

**Cột trong List**:

- ID, Tên lớp, Môn học, Giáo viên, Kỳ học, Số sinh viên
- Action buttons: Manage Students, Edit, Delete

---

### 3. **Student/** - Quản Lý Sinh Viên

| File            | Mục đích                        |
| --------------- | ------------------------------- |
| `List.cshtml`   | Danh sách sinh viên với Avatar  |
| `Add.cshtml`    | Form thêm sinh viên             |
| `Edit.cshtml`   | Form chỉnh sửa                  |
| `Detail.cshtml` | Chi tiết sinh viên (với Avatar) |

**Avatar Details**:

- Fallback: `/assets/img/profiles/avatar-01.jpg`
- Hỗ trợ ImagePath custom
- onerror handler cho ảnh bị lỗi

---

### 4. **Instructor/** - Quản Lý Giáo Viên

| File            | Mục đích                                                                 |
| --------------- | ------------------------------------------------------------------------ |
| `List.cshtml`   | Danh sách giáo viên (Avatar, Giới tính, Chứng chỉ, Kinh nghiệm) ⭐ FIXED |
| `Add.cshtml`    | Form thêm giáo viên (Dropdown Khoa)                                      |
| `Edit.cshtml`   | Form chỉnh sửa                                                           |
| `Detail.cshtml` | Chi tiết giáo viên (Avatar, Thông tin chi tiết)                          |

**Avatar Fix Applied**:

- ❌ Cũ: `/assets/img/603A6862.JPG` (không tồn tại)
- ✅ Mới: `/assets/img/profiles/avatar-02.jpg` (consistent)

---

### 5. **Department/** - Quản Lý Khoa

| File            | Mục đích                           |
| --------------- | ---------------------------------- |
| `List.cshtml`   | Danh sách khoa                     |
| `Add.cshtml`    | Form thêm khoa                     |
| `Edit.cshtml`   | Form chỉnh sửa                     |
| `Detail.cshtml` | Chi tiết khoa (Giáo viên, Môn học) |

---

### 6. **Subject/** - Quản Lý Môn Học

| File          | Mục đích                  |
| ------------- | ------------------------- |
| `List.cshtml` | Danh sách môn             |
| `Add.cshtml`  | Form thêm môn (chọn Khoa) |
| `Edit.cshtml` | Form chỉnh sửa            |

---

## 🔗 Shared Components

### \_AdminLayout.cshtml

**Header**:

- Logo (full & small)
- Search bar
- Notifications (badge badge-pill)
- User dropdown menu

**Sidebar Navigation**:

```
Dashboard
├── Students
├── Teachers
├── Departments
├── Subjects
└── Class Student
```

**Features**:

- Responsive (Mobile toggle button)
- Role-based menu display (Admin/Instructor/Student)
- User profile dropdown

**Avatar Issue Fix**:

- Main avatar: `~/assets/img/default-avatar-1-32.svg`
- Fallback consistent across all views

### \_ValidationScriptsPartial.cshtml

- jQuery validation scripts

### Error.cshtml

- Error page layout

---

## 📊 Data Models & Relationships

```
ClassModel
├── ClassId (PK)
├── ClassName
├── SubjectId → SubjectModel
├── TeacherId → TeacherModel
├── Semester
└── NumberOfStudents

SubjectModel
├── SubjectId (PK)
├── SubjectName
└── DepartmentId → DepartmentModel

TeacherModel (Users với Role="Instructor")
├── TeacherId (PK)
├── Name
├── Gender
├── Qualification
├── Experience
├── ImagePath (Avatar)
└── DepartmentId → DepartmentModel

DepartmentModel
├── DepartmentId (PK)
├── DepartmentName
└── Description

StudentModel
├── StudentId (PK)
├── FullName
├── Gender
├── ImagePath (Avatar)
└── ... other fields
```

---

## 🐛 Issues Fixed

### Issue #1: Subject Display "Unknown ()"

**Problem**: Classes list hiển thị môn học dạng "Unknown ()"

**Cause**: ClassController.List() - JOIN condition sai

```csharp
// ❌ Cũ
join s in subjects on c.SubjectName equals s.SubjectId

// ✅ Mới
join s in subjects on c.SubjectId equals s.SubjectId
```

**Files Updated**:

- `Areas/Admin/Controllers/ClassController.cs` (Line 45, 55)

---

### Issue #2: Avatar Image Path Error

**Problem**: Instructor List hiển thị ảnh avatar bị lỗi

**Cause**: Path `/assets/img/603A6862.JPG` không tồn tại

**Fix Applied**:

```csharp
// ❌ Cũ
src="@(string.IsNullOrEmpty(item.ImagePath) ? "/assets/img/603A6862.JPG" : item.ImagePath)"

// ✅ Mới
src="@(string.IsNullOrEmpty(item.ImagePath) ? "/assets/img/profiles/avatar-02.jpg" : item.ImagePath)"
```

**Files Updated**:

- `Areas/Admin/Views/Instructor/List.cshtml` (Line 63-64)
- Consistent fallback: `/assets/img/profiles/avatar-02.jpg`

---

## 🔐 Security & Authorization

- Controllers: `[Authorize(Roles = "Admin")]`
- HomeController: Mandatory Admin role
- Instructors: Mandatory Admin role (CRUD operations)
- Students: Partial authorization (List/Add may be accessible)

---

## 📁 Asset Dependencies

**Required Avatar Assets**:

```
wwwroot/assets/img/
├── profiles/
│   ├── avatar-01.jpg (Students)
│   └── avatar-02.jpg (Instructors)
├── default-avatar-1-32.svg (Header)
├── logo.png
├── logo-small.png
└── ... other assets
```

---

## 🎯 Tóm Tắt

✅ **6 Controllers** quản lý đầy đủ CRUD cho:

- Classes (Lớp)
- Students (Sinh viên)
- Instructors/Teachers (Giáo viên)
- Departments (Khoa)
- Subjects (Môn học)
- Dashboard (Thống kê)

✅ **2 Issues Fixed**:

1. Subject display error (JOIN condition)
2. Avatar image path error (consistent fallback)

✅ **Responsive Layout** với Sidebar, Header, Mobile toggle

✅ **Role-based Authorization** cho Admin access
