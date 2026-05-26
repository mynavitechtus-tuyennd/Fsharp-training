// Tải thư viện FSharp.Data trực tiếp từ NuGet
#r "nuget: FSharp.Data"
open FSharp.Data

// ==========================================
// BÀI TẬP: XỬ LÝ JSON VỚI TYPE PROVIDER
// ==========================================

// BÀI TẬP 1: Định nghĩa cấu trúc JSON (Type Provider)
// Yêu cầu: Sử dụng JsonProvider với một chuỗi JSON mẫu đại diện cho Sản phẩm.
// Mẫu: """ { "id": 1, "title": "Laptop", "price": 999.99, "tags": ["electronics", "computers"] } """

type ProductJson = JsonProvider<"""{ "id": 1, "title": "Laptop", "price": 999.99, "tags": ["electronics", "computers"] }""">

// Dữ liệu JSON thực tế (Lưu ý: Đây là một MẢNG các sản phẩm)
let jsonString = """
[
    { "id": 101, "title": "Bàn phím cơ", "price": 150.0, "tags": ["accessories"] },
    { "id": 102, "title": "Chuột không dây", "price": 45.5, "tags": ["accessories", "wireless"] }
]
"""

// BÀI TẬP 2: Phân tích (Parse) chuỗi JSON
// Yêu cầu: Dùng hàm Parse của ProductJson để đọc jsonString. 
// Trả về một danh sách/mảng các sản phẩm.

let parseProducts (json: string) =
    ProductJson.ParseList json

// BÀI TẬP 3: In ra màn hình
// Yêu cầu: Lặp qua danh sách lấy được, in ra Tên và Giá của từng sản phẩm.
// Điểm kỳ diệu: Mặc dù JSON viết là "title" và "price", F# Type Provider 
// sẽ tự động tạo cho bạn thuộc tính viết hoa: p.Title, p.Price!

let printProducts (products: ProductJson.Root array) =
    // TODO: Dùng Array.iter hoặc vòng lặp for p in products do ...
    // Gợi ý in: printfn "- %s: $%M" p.Title p.Price
    products
    |> Array.iter (fun p -> printfn "- %s: $%M" p.Title p.Price)

// ==========================================
// CHẠY THỬ (Bỏ comment để test)
// ==========================================

let products = parseProducts jsonString
printfn "Danh sách sản phẩm:"
printProducts products
