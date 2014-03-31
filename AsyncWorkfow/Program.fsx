#r "System.Net"
#r "FSharp.Data.TypeProviders"
#r "System.Data.Services.Client"
 
module ODataTest =
    // Use the OData type provider to create types that can be used to access the Northwind database.
    // Add References to FSharp.Data.TypeProviders and System.Data.Services.Client
    open Microsoft.FSharp.Data.TypeProviders
    open System.Net
    open System.Linq

    let runAll() = 
        let proxy = new WebProxy("http://localhost:8888", true) :> IWebProxy
        WebRequest.DefaultWebProxy = proxy 
        type Northwind = ODataService<"http://services.odata.org/Northwind/Northwind.svc">
    
        let db = Northwind.GetDataContext()

        // A query expression.
        let query1 = query { for customer in db.Customers do
                             select customer }

        query1 |> Seq.iter (fun customer -> printfn "Company: %s Contact: %s" customer.CompanyName customer.ContactName)

        let query2 = query {
                        for employee in db.Employees do
                        //where ((set [1; 2; 5; 10] |> Set.toSeq).Contains(employee.EmployeeID))
                        select employee
                     }
        query2 |> Seq.iter (fun employee -> printfn "%A %A - %A" employee.FirstName employee.LastName employee.Title)
    //runAll()
  
module AsyncTest =
    open System
    open System.Threading
    open System.Net
    open Microsoft.FSharp.Control.WebExtensions

    let urlList = [ "Microsoft.com", "http://www.microsoft.com/" 
                    "MSDN", "http://msdn.microsoft.com/" 
                    "Bing", "http://www.bing.com" ]

    let sleepRandom = 
        let random = new Random()
        let ms = random.Next(10000, 20000)
        Thread.Sleep(ms)
        Thread.CurrentThread.ManagedThreadId
 
    let sleepRandomAsync =
        async {
            return sleepRandom
        }

    let fetchAsync(name, url:string) =
        async { 
            try 
                let uri = new System.Uri(url)
                let webClient = new WebClient()
                let parentThreadId = Thread.CurrentThread.ManagedThreadId
                printfn "%A download started (%A):%A" name parentThreadId url
                let! html = webClient.AsyncDownloadString(uri)
                printfn "%A download complete(%A):%A" name parentThreadId url
            with
                | ex -> printfn "%s" (ex.Message);
        }

    let testAsync(id:int) =
        async { 
            let parentThreadId = Thread.CurrentThread.ManagedThreadId
            let! asyncThreadId = sleepRandomAsync
            printfn "%d - Thread %A -> Async Thread %A" id parentThreadId asyncThreadId
        }

    let testSync(id:int) =
        let parentThreadId = Thread.CurrentThread.ManagedThreadId
        printfn "%d - Thread %A -> Sync Thread %A" id parentThreadId sleepRandom 
        
    let runAll() =
//        urlList
//        |> Seq.map fetchAsync
//        |> Async.Parallel 
//        |> Async.RunSynchronously
//        |> ignore

        seq {0..100}
        |> Seq.map testAsync
        |> Async.Parallel
        |> Async.RunSynchronously
        |> ignore

        seq {0..100}
        |> Seq.map testSync
        |> ignore

    runAll()

#if COMPILED
module BoilerPlateForForm = 
    [<System.STAThread>]
    do ()
    do System.Windows.Forms.Application.Run()
#endif
