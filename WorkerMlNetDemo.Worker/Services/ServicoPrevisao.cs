using Microsoft.Extensions.Options;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Text;
using WorkerMlNetDemo.Worker.Configuration;
using WorkerMlNetDemo.Worker.Models;

namespace WorkerMlNetDemo.Worker.Services;

public sealed class ServicoPrevisao
{
    private readonly ILogger<ServicoPrevisao> _logger;
    private readonly ModeloOptions _options;
    private readonly MLContext _mlContext;

    private PredictionEngine<DadosRotaEntrada, DadosRotaSaida>? _predictionEngine;

    public ServicoPrevisao(
        ILogger<ServicoPrevisao> logger,
        IOptions<ModeloOptions> options)
    {
        _logger = logger;
        _options = options.Value;
        _mlContext = new MLContext(seed: 1);
    }

    public void CarregarModelo()
    {
        var caminhoModelo = Path.Combine(AppContext.BaseDirectory, _options.CaminhoArquivoModelo);

        if (!File.Exists(caminhoModelo))
            throw new FileNotFoundException("Modelo não encontrado.", caminhoModelo);

        var modelo = _mlContext.Model.Load(caminhoModelo, out _);

        _predictionEngine = _mlContext.Model.CreatePredictionEngine<DadosRotaEntrada, DadosRotaSaida>(modelo);

        _logger.LogInformation("Modelo carregado com sucesso de: {CaminhoModelo}", caminhoModelo);
    }

    public DadosRotaSaida Prever(DadosRotaEntrada entrada)
    {
        if (_predictionEngine is null)
            throw new InvalidOperationException("O modelo ainda não foi carregado.");

        return _predictionEngine.Predict(entrada);
    }
}
