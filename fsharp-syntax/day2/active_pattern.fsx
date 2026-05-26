// BÀI TẬP 1: Viết Active Pattern phân loại Email
// Yêu cầu:
// - Nếu email chứa đuôi "@admin.com" -> Trả về Admin
// - Nếu email chứa đuôi "@gmail.com" -> Trả về Personal
// - Còn lại -> Trả về Other
let (|Admin|Personal|Other|) (email: string) =
    if email.EndsWith("@admin.com") then
        Admin
    elif email.EndsWith("@gmail.com") then
        Personal
    else 
        Other

// BÀI TẬP 2: Sử dụng Active Pattern vừa tạo
// Yêu cầu: 
// Nhận vào một email, dùng match...with kết hợp với Active Pattern ở trên.
// - Admin -> trả về "Quyền quản trị viên"
// - Personal -> trả về "Tài khoản cá nhân"
// - Other -> trả về "Tài khoản khác"

let checkAccountType (email: string) =
    match email with
        | Admin -> "Quyền quản trị viên"
        | Personal -> "Tài khoản cá nhân"
        | Other -> "Tài khoản khác"


// ==========================================
// CHẠY THỬ (Bỏ comment để test)
// ==========================================

printfn "john@gmail.com: %s" (checkAccountType "john@gmail.com")
printfn "boss@admin.com: %s" (checkAccountType "boss@admin.com")
printfn "contact@company.com: %s" (checkAccountType "contact@company.com")

