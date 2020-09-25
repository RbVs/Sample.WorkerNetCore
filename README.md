# Sample.WorkerNetCore

Demonstriert, wie ein einfacher .NET Core Workerservice erstellt und als Service unter Windows installiert wird. 

#### Nuget Dependencies

> * Microsoft.Extensions.Hosting" Version="3.1.8"
> * Microsoft.Extensions.Hosting.WindowsServices" Version="3.1.8" 
> * Serilog.AspNetCore" Version="3.4.0"
> * Serilog.Sinks.File" Version="4.1.0"

#### Setup

Überschreiben des Logger mit Serilog.

```csharp
public static void Main(string[] args)
{
   Log.Logger = new LoggerConfiguration()
      .MinimumLevel.Debug()
      .MinimumLevel.Override("Microsoft", LogEventLevel. Warning)
      .Enrich.FromLogContext()
      .WriteTo.File(@"C:\Temp\Workerservice\Sample_WorkerNetCore_Log.txt")
      .CreateLogger();

       CreateHostBuilder(args).Build().Run();
}
```

UseSerilog hinzufügen.
```csharp
private static IHostBuilder CreateHostBuilder(string[] args)
{
   return Host.CreateDefaultBuilder(args)
      .UseWindowsService()
      .ConfigureServices((hostContext, services) =>
      {
         services.AddHostedService<Worker>();
      })
      .UseSerilog();
}
```
#### Worker Funktionen 
Die `StartAsync()` Methode kann verwendet werden, um ein Event beim initialen starten auszuführen.
```csharp
public override Task StartAsync(CancellationToken cancellationToken)
{
    client = new HttpClient();
    _logger.LogInformation("Worker is starting");
    return base.StartAsync(cancellationToken);
}
```
Die `StopAsync()` Methode kann verwendet werden, um ein Event vor dem Beenden auszuführen.
```csharp
public override Task StopAsync(CancellationToken cancellationToken)
{
    client.Dispose();
    _logger.LogInformation("Worker is shutting down");
    return base.StopAsync(cancellationToken);
}
```
Die `ExecuteAsync` Method wird während der gesamten Laufzeit ausgeführt. Mit `Task.Delay()` kann die Ausführung verzögert bzw. zur periodischen Ausführung genutzt werden.
```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    var uri = "https://google.com";

    while (!stoppingToken.IsCancellationRequested)
    {
        var result = await client.GetAsync(uri, stoppingToken);

        if (result.IsSuccessStatusCode)
        {
            _logger.LogInformation($"{uri} is up and running!");
        }
        else
        {
            _logger.LogInformation($"{uri} is down!");
        }
                
        await Task.Delay(5000, stoppingToken);
    }
}
```


#### Service Installation
Die Installation erfolgt über Powershell. Um einen Service installieren zu können, benötigt der Benutzer Administrator Rechte.

Folgender Befehl installiert den Service:
> sc.exe create WorkerServiceName binpath= C:\Temp\Workerservice\PublishDirectory.exe start= auto
* **sc.exe**: Service Control Manager 
* **WorkerServiceName**: Name deines Windows Service
* **binpath=**: Pfad zum Ordner in dem der Service veröffentlicht wurde
* **start= auto**: Legt fest, dass der Servcice automatisch gestartet wird


#### Service Deinstallation
Folgender Befehl deinstalliert den Service:
> sc.exe delete WorkerServiceName