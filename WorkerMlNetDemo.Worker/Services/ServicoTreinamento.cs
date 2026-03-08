using Microsoft.Extensions.Options;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Text;
using WorkerMlNetDemo.Worker.Configuration;
using WorkerMlNetDemo.Worker.Models;

namespace WorkerMlNetDemo.Worker.Services;

public sealed class ServicoTreinamento
{
    private readonly ILogger<ServicoTreinamento> _logger;
    private readonly ModeloOptions _options;

    public ServicoTreinamento(
        ILogger<ServicoTreinamento> logger,
        IOptions<ModeloOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public string ObterCaminhoCompletoModelo()
    {
        return Path.Combine(AppContext.BaseDirectory, _options.CaminhoArquivo);
    }

    public bool ModeloExiste()
    {
        return File.Exists(ObterCaminhoCompletoModelo());
    }

    public void TreinarESalvarModelo()
    {
        var mlContext = new MLContext(seed: 1);

        var dadosTreino = GerarDadosFake(500);
        var dataView = mlContext.Data.LoadFromEnumerable(dadosTreino);

        var pipeline = mlContext.Transforms.Concatenate(
                            "Features",
                            nameof(DadosRotaEntrada.DistanciaKm),
                            nameof(DadosRotaEntrada.TransitoNivel),
                            nameof(DadosRotaEntrada.Chuva),
                            nameof(DadosRotaEntrada.HoraDoDia))
                       .Append(mlContext.Regression.Trainers.Sdca(
                            labelColumnName: nameof(DadosRotaEntrada.TempoEstimadoMinutos),
                            featureColumnName: "Features"));

        var modelo = pipeline.Fit(dataView);

        var previsoes = modelo.Transform(dataView);
        var metricas = mlContext.Regression.Evaluate(
            previsoes,
            labelColumnName: nameof(DadosRotaEntrada.TempoEstimadoMinutos));

        var caminhoModelo = ObterCaminhoCompletoModelo();
        mlContext.Model.Save(modelo, dataView.Schema, caminhoModelo);

        _logger.LogInformation("Modelo treinado e salvo em: {CaminhoModelo}", caminhoModelo);
        _logger.LogInformation("R²: {R2:F4}", metricas.RSquared);
        _logger.LogInformation("RMSE: {RMSE:F4}", metricas.RootMeanSquaredError);
    }

    private static List<DadosRotaEntrada> GerarDadosFake(int quantidade)
    {
        var random = new Random(123);
        var lista = new List<DadosRotaEntrada>();

        for (int i = 0; i < quantidade; i++)
        {
            var distancia = (float)(random.NextDouble() * 45 + 1);
            var transito = (float)(random.NextDouble() * 10);
            var chuva = random.Next(0, 2) == 1 ? 1f : 0f;
            var hora = (float)random.Next(0, 24);

            var tempoBase = distancia * 2.2f;
            var impactoTransito = transito * 3.5f;
            var impactoChuva = chuva == 1f ? 8f : 0f;
            var impactoHorarioPico = (hora >= 7 && hora <= 9) || (hora >= 17 && hora <= 19) ? 12f : 0f;

            var tempoFinal = tempoBase + impactoTransito + impactoChuva + impactoHorarioPico;

            lista.Add(new DadosRotaEntrada
            {
                DistanciaKm = distancia,
                TransitoNivel = transito,
                Chuva = chuva,
                HoraDoDia = hora,
                TempoEstimadoMinutos = tempoFinal
            });
        }

        return lista;
    }
}