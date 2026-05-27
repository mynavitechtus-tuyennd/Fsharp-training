Azure AI Search (trước đây là Azure Cognitive Search) là một dịch vụ tìm kiếm đám mây được quản lý hoàn toàn, cung cấp khả năng truy xuất thông tin mạnh mẽ như Full-text search, Vector search, và Hybrid search

## 1. Tạo index và định nghĩa schema
- Trong Azure AI Search, một index chứa các documents có thể tìm kiếm được, tương đương với khái niệm table trong CSDLQH.
- Schema của index được định nghĩa thông qua danh sách các **fields**. Mỗi field có các thuộc tính như:
    - `name`: tên của field, dùng để truy vấn
    - `type`: kiểu dữ liệu (string, int, collection, vector, etc.)
    - `searchable`: có thể tìm kiếm được hay không
    - `filterable`: có thể dùng để lọc kết quả hay không
    - `retrievable`: có thể trả về trong kết quả hay không
    - `analyzer`: cách phân tích văn bản (ví dụ: tiếng Anh, tiếng Việt)
    - `vectorDimensions`: số chiều của vector (chỉ áp dụng cho field kiểu vector)
    ....
- Bằng .NET SDK, ta có thể sử dụng `SearchIndexClient` để tạo index và sử dụng các lớp hỗ trợ như `SimpleField`, `SearchableField` để định nghĩa schema.
    - **SimpleField**: Dành cho các trường không cần phân tích toàn văn bản (full-text search) như ID, giá cả, hoặc danh mục.
    - **SearchableField**: Bắt buộc phải là kiểu chuỗi (string), luôn hỗ trợ tìm kiếm toàn văn bản và áp dụng các quy tắc ngôn ngữ học (Analyzers).

- Các thuộc tính quan trọng:
    - `IsKey`: Chỉ định trường nào là khóa chính (ID) của document. Trường này phải có kiểu string và không được trùng lặp trong index.
    - `IsSearchable`: Cho phép tìm kiếm toàn văn bản trên trường này. Chỉ áp dụng cho trường kiểu string và sẽ sử dụng analyzer để phân tích văn bản.
    - `IsFilterable`: Cho phép sử dụng trường này trong các điều kiện lọc (filter) của truy vấn.
    - `IsFacetable`: Cho phép sử dụng trường này trong các điều kiện phân nhóm (facet) của truy vấn.
    - `IsSortable`: Cho phép sắp xếp kết quả dựa trên trường này.
    - `AnalyzerName`: Xác định cách phân tích văn bản cho trường này, ví dụ: `AnalyzerName = en.lucene` cho tiếng Anh, `AnalyzerName = vi.microsoft` cho tiếng Việt.
    ....

```fsharp
open Azure
open Azure.Search.Documents.Indexes

let defineIndex () =
    let fields = [|
        SimpleField("id", SearchFieldDataType.String, IsKey = true, IsFilterable = true)
        SearchableField("content", IsSearchable = true, AnalyzerName = "en.lucene")
        SimpleField("category", SearchFieldDataType.String, IsFilterable = true, IsFacetable = true)
        SimpleField("price", SearchFieldDataType.Double, IsFilterable = true, IsSortable = true)
    |]

    SearchIndex("products", fields)
```

- Ngoài ra, có thể khai báo các thuộc tính vào Record type để dễ dàng sử dụng trong code, nhưng khi tạo index vẫn cần định nghĩa schema bằng cách sử dụng các lớp hỗ trợ như trên. Record type chỉ giúp tổ chức dữ liệu trong code, không tự động tạo ra schema cho index.

```fsharp
type Product = {
    [<SimpleField(IsKey = true, IsFilterable = true)>]
    Id: string

    [<SearchableField(IsSearchable = true, AnalyzerName = "en.lucene", IsFilterable = true, IsSortable = true)>]
    Name: string

    [<SimpleField(IsFilterable = true, IsFacetable = true, IsSortable = true)>]
    Description: string
}
```

## 2. Analyzers, Tokenizers và Custom Analyzers
- Quá trình tìm kiếm toàn văn (Full-text search) hoạt động dựa trên các token được bóc tách từ văn bản trong quá trình lập chỉ mục. 
    - **Analyzers (Bộ phân tích)**: Là thành phần xử lý chuỗi văn bản. Mặc định, Azure AI Search sử dụng Standard Lucene analyzer độc lập với ngôn ngữ
    - **Tokenizers (Bộ tách từ)**: Chia chuỗi văn bản dài thành các từ đơn lẻ (tokens) dựa trên khoảng trắng hoặc ký tự đặc biệt
    - **Token Filters (Bộ lọc token)**: Biến đổi token (ví dụ: chuyển thành chữ thường, loại bỏ stop-words)

- Hỗ trợ Tiếng Việt & Ngôn ngữ học: Đối với ngôn ngữ có dấu và quy tắc ngắt từ phức tạp như Tiếng Việt, nên gán các Analyzer chuyên dụng như ``vi.microsoft`` hoặc ``vi.lucene`` trực tiếp vào thuộc tính ``AnalyzerName`` của ``SearchableField`` để hệ thống hiểu ngữ nghĩa chính xác hơn
- Ta cũng có thể tự định nghĩa Custom Analyzer (bộ phân tích tùy chỉnh) bằng cách kết hợp một Tokenizer với các Token Filters cụ thể để phục vụ các yêu cầu như tìm kiếm ngữ âm hoặc biểu thức chính quy (Regex)

## 3. Đẩy dữ liệu vào index
- Để đưa dữ liệu vào Azure AI Search, bạn có hai mô hình: Pull (dùng Indexer lấy từ DB) và Push (đẩy trực tiếp từ Code bằng REST/SDK)
- Với mô hình Push, bạn sử dụng `SearchClient` để thực thi các hành động tải lên theo batch thông qua ``IndexDocumentsBatch``. Các hành động có thể là:
    - `Upload`: Thêm mới hoặc ghi đè document nếu đã tồn tại
    - `Merge`: Cập nhật một phần document, giữ nguyên các trường không được cập nhật
    - `Delete`: Xóa document khỏi index
    - `MergeOrUpload`: Cập nhật nếu document tồn tại, hoặc thêm mới nếu không tồn tại

```fsharp
open Azure.Search.Documents

let pushData (searchClient: SearchClient) (products: Product list) =
    task {
        // Đóng gói các tài liệu vào một Batch với hành động MergeOrUpload
        let batch = IndexDocumentsBatch.MergeOrUpload(products)
        let! response = searchClient.IndexDocumentsAsync(batch)
        return response
    }
```
- (Lưu ý đối với F#: Cần phải cài đặt ``FSharp.SystemTextJson`` để Azure SDK có thể Serialize đúng Record và Discriminated Union của F# thành JSON khi đưa lên AI Search)

## 4. Full-text search và Advance Filter  

Full-text search cơ bản:
- ``SearchClient.SearchAsync`` hỗ trợ tìm kiếm từ khóa với thuật toán xếp hạng BM25
. Lớp ``SearchOptions`` cho phép định hình kết quả trả về như: giới hạn số lượng (Size), chọn trường trả về (Select), hay sắp xếp (OrderBy)

Advance Filter: Lọc mảng đối tượng lồng nhau (Nested array/Collection):

- Azure AI Search sử dụng biểu thức ``OData`` để đánh giá bộ lọc. Khi cần lọc một trường là mảng (ví dụ: tags hoặc danh sách rooms), bạn phải sử dụng các toán tử bộ sưu tập (Collection operators) là any hoặc all cùng với các biến phạm vi (range variables)

Ví dụ: Tìm các khách sạn có mảng ``Rooms`` chứa ít nhất một phòng có giá dưới 100:

```fsharp
let searchWithFilter (searchClient: SearchClient) (query: string) =
    task {
        let options = SearchOptions()
        options.Filter <- "category eq 'Hotel' and Rooms/any(r: r/Price lt 100)"
        options.Select.Add("HotelName")

        let! response = searchClient.SearchAsync<Product>(query, options)
        let results = 
            response.Value.GetResults() 
            |> Seq.map (fun r -> r.Document) 
            |> Seq.toList
            
        return results
    }
```
