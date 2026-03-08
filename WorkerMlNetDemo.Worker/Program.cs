using WorkerMlNetDemo.Worker;
using WorkerMlNetDemo.Worker.Configuration;
using WorkerMlNetDemo.Worker.Services;
using WorkerMlNetDemo.Worker.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<ModeloOptions>(
    builder.Configuration.GetSection(ModeloOptions.SectionName));

builder.Services.AddSingleton<ServicoTreinamento>();
builder.Services.AddSingleton<ServicoPrevisao>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
