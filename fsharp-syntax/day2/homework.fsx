// Tải các thư viện
#r "nuget: FSharp.SystemTextJson"
#r "nuget: xunit"
#r "nuget: FsUnit"

open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open Xunit
open FsUnit

// ==========================================
// CẤU HÌNH JSON CHO F#
// ==========================================

let jsonOptions = JsonSerializerOptions()
jsonOptions.Converters.Add(JsonFSharpConverter())

type Order = {
    OrderId: string
    CustomerType: string
    TotalAmount: float
}

// Khai báo các loại lỗi có thể xảy ra trong hệ thống
type OrderError =
    | FileNotFound of string
    | InvalidJson of string
    | NegativeAmount

// ==========================================
// YÊU CẦU 1: Active Patterns
// ==========================================
// TODO 1: Viết một Active Pattern tên là (|HighValue|NormalValue|) nhận vào một số float (amount).
// Nếu amount >= 1000.0 thì trả về HighValue, ngược lại trả về NormalValue

let (|HighValue|NormalValue|) (amount: float) = 
    if amount >= 1000.0 then
        HighValue
    else
        NormalValue

// ==========================================
// YÊU CẦU 2: Pattern Matching
// ==========================================

let calculateDiscount (customerType: string) (amount: float): float = 
    // TODO 2: Đối chiếu đồng thời (customerType, amount)
    // - Khách "VIP" và Đơn "HighValue" -> giảm 20% (nhân với 0.8)
    // - Khách "VIP" và Đơn "NormalValue" -> giảm 10% (nhân với 0.9)
    // - Khách thường (bất kể giá trị nào) -> không giảm (giữ nguyên amount)
    match customerType, amount with
        | "VIP", HighValue -> amount * 0.8
        | "VIP", NormalValue -> amount * 0.9
        | _ -> amount

// ==========================================
// YÊU CẦU 3 & 4: Exception, JSON Parsing & Result
// ==========================================

let processOrderFromFile (filePath: string) =
    try
        let jsonString = File.ReadAllText filePath
        try
            // TODO 3: Sử dụng JsonSerializer.Deserialize<Order>(...) với jsonOptions để parse chuỗi
            let order = JsonSerializer.Deserialize<Order>(jsonString, jsonOptions)

            if order.TotalAmount < 0.0 then
                Error NegativeAmount
            else
                let finalPrice = calculateDiscount order.CustomerType order.TotalAmount
                // TODO 4: Trả về kết quả thành công bọc trong Ok
                Ok finalPrice
        with
            | :? JsonException as ex -> Error (InvalidJson ex.Message)
    with
        // TODO 5: Bắt lỗi tệp không tồn tại (:? FileNotFoundException) và trả về Error(FileNotFound filePath)
        | :? FileNotFoundException as ex -> Error (FileNotFound ex.FileName)

// ==========================================
// YÊU CẦU 5: Unit Test bằng XUnit
// ==========================================

[<Fact>]
let ``Khách VIP mua đơn hàng giá trị cao phải được giảm 20%`` () =
    let result = calculateDiscount "VIP" 1000.0

    // Dùng 1 cách khác bằng pipeline
    // result
    //     |> should equal 800.0
    Assert.Equal(800.0, result)

// ==========================================
// CHẠY THỬ KỊCH BẢN (Giả lập trong Script)
// ==========================================
printfn "--- Bắt đầu chạy Script ---"

// 1. Tạo một file JSON ảo để test
let testFilePath = "test_order.json"
File.WriteAllText(testFilePath, """{ "OrderId": "ORD01", "CustomerType": "VIP", "TotalAmount": 1500.0 }""")

// 2. Chạy hàm xử lý
match processOrderFromFile testFilePath with
    | Ok finalPrice -> printfn "✅ Xử lý thành công! Giá cuối cùng phải thanh toán: %f" finalPrice
    | Error (FileNotFound msg) -> printfn "❌ Lỗi: Không tìm thấy file - %s" msg
    | Error (InvalidJson msg) -> printfn "❌ Lỗi: JSON không hợp lệ - %s" msg
    | Error NegativeAmount -> printfn "❌ Lỗi: Giá trị đơn hàng không được âm"