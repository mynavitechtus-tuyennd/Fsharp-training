#r "nuget: Microsoft.Azure.Functions.Worker"
#r "nuget: Microsoft.Azure.Functions.Worker.Extensions.CosmosDB"
#r "nuget: Microsoft.Extensions.Logging.Abstractions"

open System.Collections.Generic
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open Microsoft.Extensions.Logging
open System.Net

type Product = {
    id: string
    categoryId: string
    name: string
    price: float
    isActive: bool
}

module ProductSyncLogic = 
    let processChanges (products: Product list) =
        products
        |> List.filter (fun x -> x.isActive)
        |> List.iter (fun y -> printfn "Đang đồng bộ sản phẩm: [%s] - Giá: [%f]" y.name y.price)

type CosmosChangeFeedFunction(logger: ILogger<CosmosChangeFeedFunction>) =

    [<Function("SyncProductToSearch")>]
    member _.Run([<CosmosDBTrigger(
        "ShopDB",
        "Products",
        Connection = "CosmosDBConnection",
        LeaseContainerName = "leases",
        CreateLeaseContainerIfNotExists = true
    )>] changes: IReadOnlyList<Product> ) = 
        if not (isNull changes) && changes.Count > 0 then
            logger.LogInformation $"[CHANGE FEED] Phát hiện {changes.Count} sản phẩm thay đổi"
            // Dùng Seq.toList để chuyển data changes thành list của F#
            let products = changes |> Seq.toList

            ProductSyncLogic.processChanges products
        