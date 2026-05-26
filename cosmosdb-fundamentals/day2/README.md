## 3. Query dữ liệu: bọc kết quả thành Async sequence
Trong .NET SDK, query trả về một `FeedIterator<T>`. Để xử lý chuẩn phong cách F#, ta thường viết một hàm đệ quy bất đồng bộ hoặc sử dụng IAsyncEnumerable (taskSeq) để "trải phẳng" (flatten) các trang kết quả.
Đây chính là cách chúng ta có thể lấy tất cả kết quả từ một query mà không phải lo lắng về việc quản lý trang (pagination) thủ công. Dưới đây là một ví dụ về cách làm điều này
Nó giống với việc sử dụng vòng lặp white để lấy tất cả kết quả từ một iterator, nhưng ở đây chúng ta sử dụng đệ quy bất đồng bộ để đạt được điều tương tự trong F#. 

```fsharp

open System.Collections.Generic

// Hàm helper để duyệt qua các trang (pages) của truy vấn
let queryCosmosAsyncSeq (query: string) (container: Container) =
    taskSeq {
        let queryDef = QueryDefinition(query)
        // Nên chỉ định MaxConcurrency để tăng tốc truy vấn song song
        let iterator = container.GetItemQueryIterator<'T>(queryDef)

        while iterator.HasMoreResults do
            let! response = iterator.ReadNextAsync()
            for item in response do
                yield item
    }

// Sử dụng hàm trên để lấy tất cả người dùng
let allUsers = queryCosmosAsyncSeq "SELECT * FROM c WHERE c.Year > 2000" container
for user in allUsers do
    printfn "User: %s" user.name
```

## 4. Quản lý lỗi và ROP
CosmosDB giao tiếp qua HTTP/TCP nên sẽ ném ra lỗi ``CosmosException`` khi có vấn đề (VD: lỗi mạng, lỗi truy vấn, lỗi phân vùng). Để viết code F# idiomatic, ta nên bọc các thao tác này trong kiểu Result hoặc sử dụng Railway Oriented Programming (ROP) để xử lý lỗi một cách rõ ràng và dễ bảo trì.

```fsharp
open Microsoft.Azure.Cosmos

let safeReadUser (container: Container) (id: string) (partitionKey: string) =
    task {
        try
            let! response = container.ReadItemAsync<User>(id, new PartitionKey(partitionKey))
            return Ok response.Resource
        with
        | :? CosmosException as ex -> return Error $"Cosmos error: {ex.StatusCode} - {ex.Message}"
        | ex -> return Error $"Unexpected error: {ex.Message}"
    }
```

## 5. Change Feed
Change Feed là một tính năng mạnh mẽ của Cosmos DB cho phép bạn theo dõi và phản ứng với các thay đổi dữ liệu theo thời gian thực. Bạn có thể sử dụng Change Feed để xây dựng các ứng dụng phản ứng (reactive applications), đồng bộ dữ liệu, hoặc thực hiện các tác vụ xử lý nền khi dữ liệu thay đổi.

Thay vì Web API vừa ghi vào DB vừa gọi sang AI Search (dễ gây lỗi mất đồng bộ), bạn chỉ cần dùng Azure Functions bám vào Change Feed. Bất cứ khi nào tài liệu thay đổi, Function sẽ tự động thức dậy, nhận danh sách tài liệu mới và đẩy sang hệ thống khác (VD: AI Search) để cập nhật chỉ mục. Điều này giúp tách biệt các thành phần, tăng độ tin cậy và dễ dàng mở rộng hơn.

```fsharp
open Microsoft.Azure.Cosmos
open Microsoft.Azure.Functions.Worker
open Microsoft.Extensions.Logging

type CosmosChangeFeedFunction(logger: ILogger) =

    [<Function("CosmosChangeFeedFunction")>]
     member _.Run ([<CosmosDBTrigger(
            "Orders",
            "ShopDB",
            Connection = "CosmosDBConnection",
            LeaseContainerName = "leases",
            CreateLeaseContainerIfNotExists = true
        )>] changes: IReadOnlyList<Domain.Order>
    ) =
        task {
            for change in changes do
                logger.LogInformation($"Document changed: {change.id}, Name: {change.name}")
        }

```
Trong ví dụ trên, `CosmosChangeFeedFunction` sẽ tự động được kích hoạt mỗi khi có thay đổi trong container được chỉ định. Bạn có thể xử lý các thay đổi này theo cách bạn muốn, chẳng hạn như cập nhật chỉ mục tìm kiếm, gửi thông báo, hoặc thực hiện các tác vụ khác.
``LeaseContainerName`` là tên container dùng để lưu trữ thông tin về tiến trình đọc Change Feed, giúp đảm bảo rằng mỗi thay đổi chỉ được xử lý một lần và cho phép chức năng tự động phục hồi nếu có lỗi xảy ra. Việc sử dụng Change Feed giúp bạn xây dựng các ứng dụng phản ứng hiệu quả và dễ dàng mở rộng khi cần thiết.
``CreateLeaseContainerIfNotExists`` là một tùy chọn tiện lợi cho phép Azure Functions tự động tạo container lưu trữ lease nếu nó chưa tồn tại, giúp giảm bớt công việc quản lý hạ tầng và đảm bảo rằng chức năng của bạn luôn sẵn sàng để xử lý các thay đổi dữ liệu.
