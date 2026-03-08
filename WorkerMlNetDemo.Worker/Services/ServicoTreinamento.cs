using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Text;
using WorkerMlNetDemo.Worker.Models;

namespace WorkerMlNetDemo.Worker.Services;

/// <summary>
/// Provides training and persistence functionality for a machine learning model that estimates route times based on
/// input data.
/// </summary>
/// <remarks>This class is sealed and cannot be inherited. It generates synthetic training data and saves the
/// trained model to a file in the application's base directory. Logging is performed to provide feedback on training
/// progress and model metrics.</remarks>
/// <param name="logger">The logger used to record informational messages during model training and saving operations.</param>
public sealed class ServicoTreinamento(ILogger<ServicoTreinamento> logger)
{
    private readonly ILogger<ServicoTreinamento> _logger = logger;
    private readonly string _modeloPath = 
        Path.Combine(AppContext.BaseDirectory, "tempo-rota.zip");    

    public void TreinarESalvarModelo()
    {
        var mlContext = new MLContext(seed: 1);

        // 1. Gerar dados fakes
        var dadosTreino = GerarDadosFake(500);

        // 2. Treinar modelo
        var dataView = mlContext.Data.LoadFromEnumerable(dadosTreino);

        // 3. Definir pipeline de treinamento
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
        var metricas = mlContext.Regression.Evaluate(previsoes,
            labelColumnName: nameof(DadosRotaEntrada.TempoEstimadoMinutos));

        mlContext.Model.Save(modelo, dataView.Schema, _modeloPath);

          _logger.LogInformation("Modelo treinado e salvo em: {Path}", _modeloPath);
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
