using System;
using System.Collections.Generic;
using System.Text;
using WorkerMlNetDemo.Worker.Models;
using WorkerMlNetDemo.Worker.Services;

namespace WorkerMlNetDemo.Worker.Workers;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ServicoTreinamento _servicoTreinamento;
    private readonly ServicoPrevisao _servicoPrevisao;

    public Worker(
        ILogger<Worker> logger,
        ServicoTreinamento servicoTreinamento,
        ServicoPrevisao servicoPrevisao)
    {
        _logger = logger;
        _servicoTreinamento = servicoTreinamento;
        _servicoPrevisao = servicoPrevisao;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker iniciado em: {Time}", DateTimeOffset.Now);

        _servicoTreinamento.TreinarESalvarModelo();
        _servicoPrevisao.CarregarModelo();

        var random = new Random();

        while (!stoppingToken.IsCancellationRequested)
        {
            var entrada = new DadosRotaEntrada
            {
                DistanciaKm = (float)(random.NextDouble() * 30 + 1),
                TransitoNivel = (float)(random.NextDouble() * 10),
                Chuva = random.Next(0, 2) == 1 ? 1f : 0f,
                HoraDoDia = random.Next(0, 24)
            };

            var previsao = _servicoPrevisao.Prever(entrada);

            _logger.LogInformation(
                "Previsão -> Distância: {Distancia:F1} km | Trânsito: {Transito:F1} | Chuva: {Chuva} | Hora: {Hora} => Tempo estimado: {Tempo:F2} min",
                entrada.DistanciaKm,
                entrada.TransitoNivel,
                entrada.Chuva,
                entrada.HoraDoDia,
                previsao.TempoEstimadoMinutos);

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
