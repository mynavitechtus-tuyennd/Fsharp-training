open System
open System.Threading.Tasks

// ==========================================
// MÔ HÌNH DỮ LIỆU
// ==========================================
type RegisterRequest = { Name: string; Email: string }
type User = { Id: int; Name: string; Email: string }

// ==========================================
// PHẦN 1: LOGIC ĐỒNG BỘ (Synchronous)
// ==========================================

// BÀI TẬP 1: Validate đầu vào
// Yêu cầu: 
// - Name không được rỗng ("").
// - Email phải chứa ký tự "@" (dùng req.Email.Contains("@")).
// - Trả về Ok(req) nếu hợp lệ, Error("Thông báo lỗi") nếu không.

let validateRequest (req: RegisterRequest) =
    if req.Name = "" then
        Error "Name không được rỗng"
    elif not(req.Email.Contains("@")) then
        Error "Email phải chứa ký tự @"
    else
        Ok req

// ==========================================
// PHẦN 2: MÔ PHỎNG DATABASE (Asynchronous)
// ==========================================
// Hàm giả lập kiểm tra DB xem email đã tồn tại chưa (Trả về Task<bool>)
let checkEmailExistsAsync (email: string) : Task<bool> =
    task {
        do! Task.Delay(500) // Giả lập độ trễ mạng 0.5s
        // Giả sử chỉ có email "admin@test.com" là đã tồn tại
        return email = "admin@test.com"
    }

let saveUserToDbAsync (req: RegisterRequest): Task<User> =
    task {
        do! Task.Delay(500)

        return { Id = 20; Name = req.Name; Email = req.Email }
    }

// ==========================================
// PHẦN 3: KẾT HỢP LUỒNG (The Master Challenge)
// ==========================================

// BÀI TẬP 2: Viết hàm Main xử lý toàn bộ luồng đăng ký
// Gợi ý các bước bên trong khối `task { ... }`:
// 1. Dùng match..with để gọi `validateRequest req`.
// 2. Nếu trả về Error msg -> return Error msg
// 3. Nếu trả về Ok validReq -> 
//      a. Gọi DB bằng cú pháp: let! isExist = checkEmailExistsAsync validReq.Email
//      b. Nếu isExist = true -> return Error "Email đã được sử dụng!"
//      c. Nếu isExist = false -> 
//             let! newUser = saveUserToDbAsync validReq
//             return Ok newUser

let registerUserAsync (req: RegisterRequest): Task<Result<User, string>> =
    task {
        let validate = validateRequest req
        match validate with
            | Error msg -> return Error msg
            | Ok validReq -> 
                let! isExists = checkEmailExistsAsync validReq.Email
                if isExists then
                    return Error "Email đã được sử dụng!"
                else
                    let! newUser = saveUserToDbAsync validReq
                    return Ok newUser
    }

let runTest testName req =
    task {
        printfn "--- TEST: %s ---" testName
        let! result = registerUserAsync req 
        
        match result with
        | Ok user -> printfn "Thành công! User tạo mới có ID = %d\n" user.Id
        | Error msg -> printfn "Từ chối: %s\n" msg
    } |> Task.WaitAll // Ép chạy đồng bộ ở ngoài cùng để xem console

let req1 = { Name = ""; Email = "john@test.com" }
let req2 = { Name = "Alice"; Email = "alicetest.com" } // Thiếu @
let req3 = { Name = "Admin"; Email = "admin@test.com" } // Email trùng DB
let req4 = { Name = "Bob"; Email = "bob@test.com" } // Hợp lệ

runTest "Tên rỗng" req1
runTest "Sai format Email" req2
runTest "Email trùng lặp" req3
runTest "Đăng ký thành công" req4