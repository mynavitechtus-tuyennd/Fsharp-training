## 1.Các Khái niệm Chính
### 1.1. Cấu trúc Phân cấp
Cosmos DB tổ chức dữ liệu theo cấu trúc 3 tầng rõ rệt:
*   **Database (Cơ sở dữ liệu):** Là đơn vị quản lý logic cấp cao nhất, đóng vai trò như một không gian tên (namespace) chứa các Container. 
*   **Container (Bộ chứa / Collection):** Đây là nơi dữ liệu thực sự được lưu trữ và cũng là nơi cấu hình **Partition Key** và chỉ số (Indexing). Container không có schema cố định, nghĩa là có thể lưu nhiều loại tài liệu JSON khác nhau trong cùng một Container.
*   **Item (Tài liệu / Document):** Là các bản ghi JSON cụ thể. Mỗi Item có dung lượng tối đa là 2MB và bắt buộc phải có một trường `id` dạng chuỗi duy nhất trong phạm vi của một Partition Key.

### 1.2. PartitionKey, Id và "Hot Partition"
Trong Cosmos DB, khả năng mở rộng ngang (horizontal scaling) hoạt động dựa trên hai khái niệm: **Logical Partition (Phân vùng logic)** và **Physical Partition (Phân vùng vật lý)**.
*   **Id (Khóa chính):** Định danh duy nhất của một tài liệu *bên trong* một Partition Key.
*   **PartitionKey (Khóa phân vùng):** Là thuộc tính để nhóm các dữ liệu có liên quan lại với nhau. Cosmos DB sử dụng giá trị này để băm (hash) và phân phối dữ liệu vào các máy chủ vật lý.


**"Hot Partition" (Nghẽn cổ chai):** Xảy ra khi một Partition Key nhận được quá nhiều yêu cầu cùng lúc, hoặc khi chọn Partition Key đang dùng chung giá trị (VD: `"TenantId" = "default"` cho tất cả user), toàn bộ dữ liệu khi đọc ra sẽ dồn vào một máy chủ vật lý dẫn đến tắc nghẽn và giảm hiệu suất. 

**Cách chọn Partition Key tối ưu:**
*   Chọn thuộc tính có **vùng ảnh hưởng cao** , ví dụ như `CustomerId`, `DeviceId`, hoặc `UserId`.
*   Dữ liệu được truy vấn cùng nhau nên nằm chung một Partition Key để tối ưu chi phí đọc (1 RU cho Point Read).
*   Tránh chọn thuộc tính có giá trị duy nhất (VD: `id`) hoặc có giá trị chung (VD: `TenantId = "default"`).

### 1.3. Kết nối Cosmos DB với F# (`CosmosClient`)
**Nguyên tắc:** `CosmosClient` phải luôn luôn được khởi tạo dưới dạng **Singleton** và sống xuyên suốt vòng đời của ứng dụng. Việc tạo mới client liên tục sẽ làm cạn kiệt cổng mạng (socket exhaustion) và tăng độ trễ.

**Cách khởi tạo `CosmosClient`:**
```fsharp
open Microsoft.Azure.Cosmos
    let options = CosmosClientOptions(ConnectionMode = ConnectionMode.Direct)
    let cosmosClient = new CosmosClient("<connection-string>", options)

let container = cosmosClient.GetContainer("<database-name>", "<container-name>")
```

## 2. Thao tác CRUD Cơ bản với F#
Trong F#, ta sử dụng ``task {...}` để gọi các phương thức bất đồng bộ của.NET SDK. Việc đọc điểm (Point Read) bằng ID và Partition Key là thao tác rẻ nhất và nhanh nhất trong Cosmos DB, chỉ tốn khoảng 1 RU

```fsharp
type User = {
    id: string
    name: string
    age: int
}

// 1. CREATE (Tạo mới)
let createUser (container: Container) (user: User) : Task =
    task {
        let! response = container.CreateItemAsync(user, new PartitionKey(user.id))
        printfn "Created user with id: %s, RU consumed: %d" user.id response.RequestCharge
    }

// 2. READ (Đọc chính xác theo ID và PartitionKey - Nhanh nhất)
let readUser (container: Container) (id: string) (partitionKey: string) : Task =
    task {
        let! response = container.ReadItemAsync<User>(id, new PartitionKey(partitionKey))
        printfn "Read user: %s, RU consumed: %d" response.Resource.name response.RequestCharge
    }

// 3. UPSERT (Tạo mới nếu chưa có, Cập nhật nếu đã có)
let updateUser (container: Container) (user: User) : Task =
    task {
        let! response = container.UpsertItemAsync(user, new PartitionKey(user.id))
        printfn "Upserted user with id: %s, RU consumed: %d" user.id response.RequestCharge
    }

// 4. PATCH (Cập nhật một phần tài liệu)
let patchUser (container: Container) (id: string) (partitionKey: string) : Task =
    task {
        let patchOperations = [ PatchOperation.Replace("/age", 30) ]
        let! response = container.PatchItemAsync<User>(id, new PartitionKey(partitionKey), patchOperations)
        printfn "Patched user with id: %s, RU consumed: %d" id response.RequestCharge
    }

// 5. DELETE (Xóa tài liệu)
let deleteUser (container: Container) (id: string) (partitionKey: string) : Task =
    task {
        let! response = container.DeleteItemAsync<User>(id, new PartitionKey(partitionKey))
        printfn "Deleted user with id: %s, RU consumed: %d" id response.RequestCharge
    } 
```