using Microsoft.Extensions.Options;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        return Path.Combine(AppContext.BaseDirectory, _options.CaminhoArquivoModelo);
    }

    public string ObterCaminhoCompletoCsv()
    {
        return Path.Combine(AppContext.BaseDirectory, _options.CaminhoArquivoCsv);
    }

    public bool ModeloExiste()
    {
        return File.Exists(ObterCaminhoCompletoModelo());
    }

    public bool CsvExiste()
    {
        return File.Exists(ObterCaminhoCompletoCsv());
    }

    public void GarantirCsv()
    {
        var caminhoCsv = ObterCaminhoCompletoCsv();

        if (CsvExiste())
        {
            _logger.LogInformation("CSV já existe em: {CaminhoCsv}", caminhoCsv);
            return;
        }

        _logger.LogInformation("CSV não encontrado. Gerando arquivo fake em: {CaminhoCsv}", caminhoCsv);

        var dados = GerarDadosFake(1000);

        using var writer = new StreamWriter(caminhoCsv, false);
        writer.WriteLine("DistanciaKm,TransitoNivel,Chuva,HoraDoDia,TempoEstimadoMinutos");

        foreach (var item in dados)
        {
            writer.WriteLine(
                string.Join(",",
                    item.DistanciaKm.ToString(CultureInfo.InvariantCulture),
                    item.TransitoNivel.ToString(CultureInfo.InvariantCulture),
                    item.Chuva.ToString(CultureInfo.InvariantCulture),
                    item.HoraDoDia.ToString(CultureInfo.InvariantCulture),
                    item.TempoEstimadoMinutos.ToString(CultureInfo.InvariantCulture)));
        }

        _logger.LogInformation("CSV gerado com sucesso.");
    }

    public void TreinarESalvarModelo()
    {
        GarantirCsv();

        var mlContext = new MLContext(seed: 1);
        var caminhoCsv = ObterCaminhoCompletoCsv();
        var caminhoModelo = ObterCaminhoCompletoModelo();

        var dataView = mlContext.Data.LoadFromTextFile<DadosRotaEntrada>(
            path: caminhoCsv,
            hasHeader: true,
            separatorChar: ',');

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

        mlContext.Model.Save(modelo, dataView.Schema, caminhoModelo);

        _logger.LogInformation("Modelo treinado a partir do CSV e salvo em: {CaminhoModelo}", caminhoModelo);
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