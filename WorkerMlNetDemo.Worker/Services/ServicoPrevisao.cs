using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Text;
using WorkerMlNetDemo.Worker.Models;

namespace WorkerMlNetDemo.Worker.Services;

public sealed class ServicoPrevisao
{
    private readonly ILogger<ServicoPrevisao> _logger;
    private readonly string _modeloPath;
    private readonly MLContext _mlContext;
    private PredictionEngine<DadosRotaEntrada, DadosRotaSaida>? _predictionEngine;

    public ServicoPrevisao(ILogger<ServicoPrevisao> logger)
    {
        _logger = logger;
        _modeloPath = Path.Combine(AppContext.BaseDirectory, "tempo-rota.zip");
        _mlContext = new MLContext(seed: 1);
    }

    public void CarregarModelo()
    {
        if (!File.Exists(_modeloPath))
            throw new FileNotFoundException("Modelo não encontrado.", _modeloPath);

        var modelo = _mlContext.Model.Load(_modeloPath, out _);
        _predictionEngine = _mlContext.Model.CreatePredictionEngine<DadosRotaEntrada, DadosRotaSaida>(modelo);

        _logger.LogInformation("Modelo carregado com sucesso de: {Path}", _modeloPath);
    }

    public DadosRotaSaida Prever(DadosRotaEntrada entrada)
    {
        if (_predictionEngine is null)
            throw new InvalidOperationException("O modelo ainda não foi carregado.");

        return _predictionEngine.Predict(entrada);
    }
}
