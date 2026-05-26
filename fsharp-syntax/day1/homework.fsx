open System
open System.Threading.Tasks

// ==========================================
// YÊU CẦU: Tạo Module
// ==========================================

module ProductProcessor = 
    // YÊU CẦU 2: Giá trị Mutable
    // TODO: Khai báo một biến mutable tên là `processCount` có giá trị ban đầu là 0
    let mutable processCount = 0

    // Một Record đơn giản đại diện cho Sản phẩm
    type Product = {Id: int; Name: string; PriceString: string}

    // ==========================================
    // YÊU CẦU 3: Kiểu dữ liệu Option
    // ==========================================
    // Hàm này thử chuyển đổi một chuỗi sang số float

    let parseFloat (priceString: string): float option = 
        let success, value = Double.TryParse priceString
        if success = true then
            Some value
        else
            None

    // ==========================================
    // YÊU CẦU 4: Xử lý Collections và Pipeline
    // ==========================================

    let calculateTotalValidValue (products: Product list): float =
        products
        |> List.map (fun x -> parseFloat x.PriceString)
        |> List.filter (fun y -> y.IsSome)
        |> List.map (fun z -> z.Value)
        |> List.fold (fun a b -> a + b) 0

    // ==========================================
    // YÊU CẦU 5: Xử lý bất đồng bộ với Task
    // ==========================================

    let processProductAction (products: Product list) =
        task {
            // TODO: Tăng biến processCount lên 1
            processCount <- processCount + 1

            printfn "Đang tính toán tổng giá trị..."
            let total = calculateTotalValidValue products
            do! Task.Delay 1000
            printfn "Đã xử lý xong lần thứ %d. Tổng giá trị hợp lệ là: %f" processCount total
        }

// ==========================================
// CHẠY THỬ
// ==========================================
// Mở module và chạy hàm Task
open ProductProcessor

let rawProducts = [
    { Id = 1; Name = "Laptop"; PriceString = "1500.50" }
    { Id = 2; Name = "Mouse"; PriceString = "Không bán" } // Lỗi
    { Id = 3; Name = "Keyboard"; PriceString = "100.00" }
    { Id = 4; Name = "Monitor"; PriceString = "" } // Lỗi
]

task {
    do! processProductAction rawProducts
    do! processProductAction rawProducts // Gọi lần 2 để test biến mutable tăng lên
} |> Task.WaitAll