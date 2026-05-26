// ==========================================
// BÀI TẬP: XỬ LÝ NGOẠI LỆ (EXCEPTIONS)
// ==========================================

// BÀI TẬP 1: Bắt lỗi với try...with
// Yêu cầu:
// - Nhận vào đường dẫn file.
// - Trong khối try, dùng File.ReadAllText(filePath) để đọc nội dung và in ra màn hình.
// - Khối with sử dụng Pattern Matching để bắt lỗi:
//   + Lỗi `:? FileNotFoundException` -> In ra "Lỗi: Không tìm thấy file!"
//   + Lỗi `:? UnauthorizedAccessException` -> In ra "Lỗi: Không có quyền truy cập!"
//   + Catch-all (`ex`) -> In ra "Lỗi không xác định: [Message]"
open System
open System.IO

let safeReadFile filePath =
    try
        let content = File.ReadAllText(filePath)
        printfn "Nội dung file: \n %s" content
    with
        | :? FileNotFoundException -> printfn "Lỗi: không tìm thấy file!"
        | :? UnauthorizedAccessException -> printfn "Lỗi: không có quyền truy cập!"
        | exn -> printfn "Lỗi không xác định: %s" exn.Message

// BÀI TẬP 2: Dọn dẹp tài nguyên với try...finally
// Yêu cầu: 
// - In ra "Đang mở kết nối mạng..."
// - Trong khối try, có thể gây ra lỗi (ví dụ chia cho 0 hoặc failwith)
// - Khối finally LUÔN LUÔN phải in ra "Đã đóng kết nối mạng." dù có lỗi hay không.

let simulateNetworkCall () =
    printfn "--- Bắt đầu ---"
    printfn "Đang mở kết nối mạng..."

    try
        failwith "Mạng đứt đột ngột."
        printfn "Tải dữ liệu thành công."
    finally
        printfn "Đã đóng kết nối mạng."

// ==========================================
// CHẠY THỬ (Bỏ comment để test)
// ==========================================


printfn "--- BÀI 1: ĐỌC FILE ---"
// Thử đọc một file không tồn tại
safeReadFile "file_khong_ton_tai.txt"

printfn "\n--- BÀI 2: THỬ NGHIỆM FINALLY ---"
// Vì simulateNetworkCall ném ra lỗi không được bắt (try...finally không bắt lỗi),
// chúng ta bọc nó bằng một try...with bên ngoài để chương trình không bị văng.
try
    simulateNetworkCall ()
with
| ex -> printfn "Đã bắt được lỗi từ bên ngoài: %s" ex.Message


