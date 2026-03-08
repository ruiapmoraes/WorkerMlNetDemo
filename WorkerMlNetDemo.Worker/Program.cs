using WorkerMlNetDemo.Worker;
using WorkerMlNetDemo.Worker.Services;
using WorkerMlNetDemo.Worker.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<ServicoTreinamento>();
builder.Services.AddSingleton<ServicoPrevisao>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
