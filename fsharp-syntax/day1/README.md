## 1. Kiểu dữ liệu Option và Giá trị Mutable

### Kiểu dữ liệu Option
Khác với nhiều ngôn ngữ sử dụng `null` để biểu thị giá trị không tồn tại, F# sử dụng kiểu dữ liệu `option` để an toàn hơn. `option` có hai trường hợp:
- `Some x`: biểu thị có giá trị `x`
- `None`: biểu thị không có giá trị

Ví dụ:
```fsharp
let presentNumber : int option = Some 49
let missingNumber : int option = None

let description =
    match presentNumber with
    | Some n -> sprintf "The number is %d" n
    | None -> "No number provided"
```

### Giá trị Mutable
Theo mặc định, mọi giá trị trong F# là bất biến (immutable), nghĩa là sau khi gán giá trị, bạn không thể thay đổi nó. Tuy nhiên, khi cần thay đổi giá trị, ta có thể sử dụng `mutable` và toán tử `<-` để cập nhật.

```fsharp
let mutable counter = 0
counter <- counter + 1  // Cập nhật giá trị của counter
```

## 2. Module và Tổ chức Code
Module trong F# giúp tổ chức code thành các phần có ý nghĩa, dễ quản lý. Một module có thể chứa:
- Hàm
- Type
- Submodule

Ví dụ:
```fsharp
module Math =
    let add a b = a + b
    let sub a b = a - b
```

Bạn có thể sử dụng module bằng cách gọi tên module sau open:
```fsharp
open Math
let sum = add 5 3
```

## 3. Xử lý Collections và Pipeline
F# có nhiều loại collection như `list`, `array`, và `seq`. Bạn có thể sử dụng các hàm như `map`, `filter`, và `fold` để xử lý chúng.

Về toán tử pipeline `|>`:
Toán tử pipeline `|>` giúp viết code dễ đọc hơn khi xử lý chuỗi. Nó cho phép bạn truyền kết quả của một biểu thức vào hàm tiếp theo một cách rõ ràng.

Về các hàm xử lý collection:
- `List.map`: biến đổi từng phần tử của list
- `List.filter`: lọc phần tử theo điều kiện
- `List.fold`: gom kết quả từ trái sang phải
- `List.iter`: thực hiện một hành động trên từng phần tử mà không trả về kết quả mới

Ví dụ sử dụng pipeline, kết hợp với `List.map` và `List.filter`:
```fsharp
let numbers = [1; 2; 3; 4; 5]

let result = 
    numbers
    |> List.map (fun x -> x * 2)        // Biến đổi mỗi phần tử thành gấp đôi
    |> List.filter (fun x -> x > 5)     // Lọc ra những phần tử lớn hơn 5
    |> List.fold (fun x y -> x + y) 0  // Tính tổng các phần tử còn lại
```

## 4. Xử lý bất đồng bộ với Async và Task
F# hỗ trợ lập trình bất đồng bộ thông qua `Async` và `Task`.
Mỗi mô hình đều có ưu nhược điểm riêng:
- `Async`: `async { ... }` nó tạo ra các tác vụ đóng băng, nghĩa là nó sẽ không chạy cho đến khi bạn gọi `Async.RunSynchronously` hoặc `Async.Start`.
- `Task`: `task { ... }` nó tạo ra các tác vụ chạy ngay lập tức, phù hợp với các API .NET hiện đại. Nó sử dụng thư viện `System.Threading.Tasks.Task`.

Ví dụ sử dụng `task`:
```fsharp
let readFileAsync (filePath: string): Task<string> =
    task {
        let! text = File.ReadAllTextAsync(filePath) // Dùng let! để chờ kết quả bất đồng bộ, giống với await trong C# và JavaScript
        return text
    }

Ví dụ sử dụng `async`:
```fsharp
let readFileAsync (filePath: string): Async<string> =
    async {
        do ! Async.Sleep 1000 // Giả lập delay
        let! text = File.ReadAllTextAsync(filePath) // Chuyển Task sang Async
        return text
    }

readFileAsync "example.txt" |> Async.RunSynchronously
``` 