#r "nuget: Microsoft.Azure.Cosmos"

open Microsoft.Azure.Cosmos
open System.Net


// ==========================================
// 1. MÔ HÌNH DỮ LIỆU
// ==========================================
// Lưu ý: Cosmos DB phân biệt chữ hoa chữ thường. 
// Thuộc tính 'id' là trường bắt buộc và phải viết thường
type User = {
    id: string  // Cosmos DB yêu cầu trường 'id' là string
    departmentId: string // Quan hệ đến Department, dùng làm partition key
    name: string
    email: string
    age: int
}

// ==========================================
// 2. BÀI TẬP THỰC HÀNH KHÁI NIỆM
// ==========================================
module CosmosConceptsHomework =
    // YÊU CẦU 1: Khởi tạo Client an toàn
    // CosmosClient luôn phải là Singleton để tránh vắt kiệt cổng mạng (port exhaustion).

    let createCosmosClient (accountEndpoint: string) (key: string): CosmosClient =
        // TODO 1: Khởi tạo CosmosClientOptions và thiết lập ConnectionMode thành Direct (để tối ưu hiệu năng) [4]
        let options = CosmosClientOptions(ConnectionMode = ConnectionMode.Direct)
        // TODO 2: Trả về instance của CosmosClient mới với connectionString và options vừa tạo
        let cosmosClient = new CosmosClient(accountEndpoint, key, options)
        cosmosClient
    // YÊU CẦU 2: Upsert Item (Thêm mới hoặc Cập nhật)

    let upsertUserAsync (container: Container) (user: User) = 
        task {
            try
                // TODO 3: Tạo PartitionKey từ trường departmentId của usser
                let pk = new PartitionKey(user.departmentId)

                // TODO 4: Gọi hàm UpsertItemAsync của container với emp và pk
                let! response = container.UpsertItemAsync(user, pk)

                return Ok response.Resource
            with
                | ex -> return Error $"Lỗi upsert: {ex.Message}"
        }

    // YÊU CẦU 3: Point Read & Xử lý Exception bằng ROP (Result)
    let getUserAsync (container: Container) (id: string) (departmentId: string) =
          task {
            try
              // TODO 5: Khởi tạo PartitionKey từ departmentId và gọi ReadItemAsync<Employee>
              let pk = new PartitionKey(departmentId)
              let! response = container.ReadItemAsync<User>(id, pk)

              return Ok response.Resource
            with
              // TODO 6: Sử dụng Pattern Matching để bắt lỗi CosmosException khi StatusCode là HttpStatusCode.NotFound (lỗi 404)
                | :? CosmosException as ex when ex.StatusCode = HttpStatusCode.NotFound -> return Error "Lỗi 404"
                | ex -> return Error $"Lỗi hệ thống: {ex.Message}"
          }

    // YÊU CẦU 4: Query dữ liệu trong một Partition (Single-partition Query)
    let getUsersByDepartmentAsync (container: Container) (departmentId: string) =
        task {
            try
                let queryDf = QueryDefinition("SELECT * FROM c WHERE c.departmentId = @deptId").WithParameter("@deptId", departmentId)

                // Giới hạn truy vấn trong 1 Partition Key để tiết kiệm RU  
                let requestOption = QueryRequestOptions(PartitionKey = PartitionKey departmentId)
                let iterator = container.GetItemQueryIterator<User>(queryDf, requestOptions = requestOption)
                let results = ResizeArray<User>()

                while iterator.HasMoreResults do
                    let! response = iterator.ReadNextAsync()
                    results.AddRange response

                return Ok (List.ofSeq results)
            with
                | :? CosmosException as ex when ex.StatusCode = HttpStatusCode.NotFound -> return Error "Lỗi 404"
                | ex -> return Error $"Lỗi hệ thống: {ex.Message}"
        }