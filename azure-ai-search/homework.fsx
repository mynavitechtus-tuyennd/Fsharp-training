#r "nuget: Azure.Search.Documents"
#r "nuget: System.Text.Json"
#r "nuget: FSharp.SystemTextJson"

open System
open System.Threading.Tasks
open Azure
open Azure.Core.Serialization
open Azure.Search.Documents
open Azure.Search.Documents.Indexes
open Azure.Search.Documents.Indexes.Models
open Azure.Search.Documents.Models
open System.Text.Json
open System.Text.Json.Serialization
open FSharp.SystemTextJson

// ==========================================
// 1. CẤU HÌNH CHO F# RECORD
// ==========================================

let jsonOptions = JsonSerializerOptions()
jsonOptions.Converters.Add(JsonFSharpConverter())

let createSearchClient (endpoint: Uri) (indexName: string) (credential: AzureKeyCredential) =
    let clientOptions = SearchClientOptions()
    clientOptions.Serializer <- JsonObjectSerializer(jsonOptions)
    SearchClient(endpoint, indexName, credential, clientOptions)

// Domain Model
type Product = {
    id: string
    categoryId: string
    name: string
    price: float
    tags: string list
}

module SearchRepository = 
    // Định nghĩa schema cho index
    let createOrUpdateIndexAsync (indexClient: SearchIndexClient) (indexName: string) = 
        task {
            let fields : SearchField array = [|
                SearchField("id", SearchFieldDataType.String, IsKey = true)
                SearchField("categoryId", SearchFieldDataType.String, IsFilterable = true)
                // Sửa thành "vi.microsoft" và thêm ngoặc đơn
                SearchField("name", SearchFieldDataType.String , AnalyzerName = LexicalAnalyzerName("vi.microsoft"), IsSearchable = true)
                SearchField("price", SearchFieldDataType.Double, IsFilterable = true, IsSortable = true)
                // Thêm ngoặc đơn cho hàm Collection()
                SearchField("tags", SearchFieldDataType.Collection(SearchFieldDataType.String), IsFilterable = true)
            |]

            let searchIndex = SearchIndex(indexName, fields)
            
            // Gọi hàm tạo Index của thư viện
            let! _ = indexClient.CreateOrUpdateIndexAsync(searchIndex)
            printfn "Tạo Index thành công!"
        }

    // Đưa dữ liệu vào index 
    let pushProductAsync (searchClient: SearchClient) (products: Product list) =
        task {
            let batch = IndexDocumentsBatch.MergeOrUpload(products)

            let indexOptions = IndexDocumentsOptions()
            indexOptions.ThrowOnAnyError <- true
            let! _ = searchClient.IndexDocumentsAsync(batch, indexOptions)

            printfn "Đã Push dữ liệu vào Search Index!"
        }
    
    // Full-text search và advance filtering
    let searchProductsAsync (searchClient: SearchClient) (query: string) (text: string) =
        task {
            let searchText = if String.IsNullOrWhiteSpace(query) then text else query

            let searchOptions = SearchOptions()
            searchOptions.Filter <- "price lt 1000 and tags/any(t: t eq 'bestseller')"
            searchOptions.QueryType <- SearchQueryType.Full
            searchOptions.SearchFields.Add("name")
            searchOptions.SearchFields.Add("tags")
            searchOptions.OrderBy.Add("price desc") 

            let! response = searchClient.SearchAsync<Product>(searchText, searchOptions)
            let results = 
                response.Value.GetResults()
                |> Seq.map (fun r -> r.Document)
                |> Seq.toList

            return results
        }