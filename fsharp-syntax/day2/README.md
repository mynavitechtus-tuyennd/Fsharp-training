## 1. Pattern matching và điều kiện
Trong F#, luồng điều kiện không chỉ là chuyển hướng code, mà là việc tính toán và trả về các giá trị.

### 1. If / elif / else
Khác với nhiều ngôn ngữ khác, `if/elif/else` trong F# là **biểu thức (expression)**không phải câu lệnh (statement) , có nghĩa là nó trả về giá trị. Tất cả các nhánh phải trả về cùng kiểu dữ liệu.

Ví dụ:

```fsharp
let classifyScore score =
    if score >= 8.0 then "A"
    elif score >= 6.5 then "B"
    elif score >= 5.0 then "C"
    else "D"
```

### 2. match ... with
Pattern matching với `match ... with` là một trong những điểm mạnh của F#. Nó mạnh hơn nhiều so với switch/case ở các ngôn ngữ khác vì có thể phân tích cấu trúc dữ liệu phức tạp. Ngoài ra nó còn cảnh báo khi có các trường hợp chưa được xử lý. (Về cơ bản thì giống với hàm match trong PHP)

Ví dụ:
```fsharp
let getDiscountMember memberLevel =
    match memberLevel with 
        | "Gold" -> 0.2
        | "Silver" -> 0.1
        | "Bronze" -> 0.05
        | _ -> 0.0
```

### 3. Active Patterns
Active Pattern là một cách để đặt tên cho logic phân loại phức tạp, giúp code trở nên dễ đọc hơn. Chúng rất hữu ích khi bạn muốn phân loại dữ liệu theo nhiều tiêu chí khác nhau.

Ví dụ:
```fsharp
let (|Even|Odd|) number =
    if number % 2 = 0 then Even else Odd

match n with
    | Even -> printfn "Số chẵn"
    | Odd -> printfn "Số lẻ"
```

## 2. Xử lý File và Json

Xử lý file:
- F# truy cập trực tiếp vào hệ sinh thái của .NET, nên bạn có thể dễ dàng sử dụng các thư viện như `System.IO` để đọc/ghi file và `Newtonsoft.Json` để xử lý JSON.

Ví dụ đọc file:
```fsharp
open System.IO
let readFile (filePath: string) : string =
    File.ReadAllText(filePath)
```

Ví dụ ghi file:
```fsharp
open System.IO
let writeFile (filePath: string) (content: string) : unit =
    File.WriteAllText(filePath, content)
```

Parsing Json:
F# sử dụng thư viện `System.Text.Json` của .NET. Tuy nhiên, F# có các kiểu dữ liệu đặc thù như record, discriminated union, mà thư viện này không tự động hiểu được. Nên cần phải cài đặt thêm thư viện `FSharp.SystemTextJson` để hỗ trợ tốt hơn.
```fsharp
open System.Text.Json
open FSharp.SystemTextJson
open System.Text.Json.Serialization

let options = JsonSerializerOptions()
options.Converters.Add(JsonFSharpConverter())

type User = {
    Id: int
    Name: string
}

let MyUser = { Id = 1; Name = "An" }
let json = JsonSerializer.Serialize(MyUser, options)
let parsed = JsonSerializer.Deserialize<User>(json, options)
```

## 3. Exception handling

Xử lý lỗi với `try ... with`:
- Để bắt các lỗi exception, bạn sử dụng `try ... with` trong F#. Đây là cách để xử lý các tình huống không mong muốn mà có thể xảy ra trong quá trình thực thi.
- Bạn có thể bắt các loại lỗi cụ thể bằng cách chỉ định kiểu exception trong phần `with`.
- Ngoài ra, bạn cũng có thể bắt tất cả các lỗi không xác định bằng cách sử dụng một mẫu chung.
- Để gán chi tiết 1 exception vào with, sử dụng cú pháp `| :? ExceptionType -> ...`
- Có thể lấy ra được message của lỗi bằng cách truy cập thuộc tính `Message` của exception.
ví dụ:
```fsharp
try
    let result = 10 / 0
    printfn "Kết quả: %d" result
with
    | :? System.DivideByZeroException as ex -> printfn "Lỗi: Chia cho 0 không hợp lệ"
    | ex -> printfn "Lỗi không xác định: %s" ex.Message
```

Xử lý lỗi với Result type:
- Thay vì ném ra Exception làm gián đoạn chương trình, F# ưu tiên sử dụng kiểu `Result<T, string>` để trả về kết quả thành công hoặc lỗi dưới dạng giá trị.
- `Result` có hai trường hợp: `Ok(value)` cho kết quả thành công và `Error(errorMessage)` cho lỗi. Điều này giúp bạn xử lý lỗi một cách rõ ràng và có thể dự đoán được, thay vì phải bắt lỗi bằng try...with.
- Đây chính là mô hình Railway Oriented Programming, giúp code dễ đọc và bảo trì hơn.
Ví dụ:
```fsharp
    let safeDivide a b =
        if b = 0 then Error "Chia cho 0 không hợp lệ"
        else Ok (a / b)
    
    match safeDivide 10 0 with
        | Ok result -> printfn "Kết quả: %d" result
        | Error msg -> printfn "Lỗi: %s" msg
```

## 4. Unit testing với xUnit
- Code F# có thể được kiểm thử bằng các framework như xUnit, NUnit, hoặc Expecto. Trong đó, xUnit là một trong những framework phổ biến nhất cho .NET.
- Nhờ triết lý functional, bạn chủ yếu chỉ việc đưa vào input và output mà không cần phải mock các dependency phức tạp như trong OOP, giúp việc viết unit test trở nên đơn giản và hiệu quả hơn.
- Đặc biệt hơn, F# cho phép đặt tên hàm test bằng text có khoảng trắng khi bọc dấu ``...`` (Chưa thấy ngôn ngữ nào khác có tính năng này)

Ví dụ:
```fsharp
open Xunit

[<Fact>]
let ``Test safeDivide with valid input`` () =
    let result = safeDivide 10 2
    match result with
    | Ok value -> Assert.Equal(5, value)
    | Error _ -> Assert.True(false, "Expected Ok result")

[<Fact>]
let ``Test safeDivide with division by zero`` () =
    let result = safeDivide 10 0
    match result with
    | Ok _ -> Assert.True(false, "Expected Error result")
    | Error msg -> Assert.Equal("Chia cho 0 không hợp lệ", msg)
```

## 5. Cách hoạt động của script F#
- F# script (.fsx) là một file chứa code F# chạy trên môi trường F# Interactive (FSI) - một REPL (Read-Eval-Print Loop) cho F#. Khi bạn chạy một file .fsx, FSI sẽ đọc và thực thi từng dòng code một cách tuần tự.
- Có thể sử dụng cú pháp `#r` để tham chiếu đến các thư viện bên ngoài.

Ví dụ:
```fsharp
#r "nuget: Newtonsoft.Json"
open Newtonsoft.Json
let json = JsonConvert.SerializeObject({ Name = "An"; Age = 30 })
printfn "%s" json
```