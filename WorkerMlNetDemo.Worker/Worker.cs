using Microsoft.ML;
using System.Globalization;

namespace WorkerMlNetDemo.Worker;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly string _modeloPath = Path.Combine(AppContext.BaseDirectory, "modelo-tempo-rota.zip");

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

        var mlContext = new MLContext(seed: 1);

        // 1. Gerar dados fakes
        var dadosTreino = GerarDadosFakes(300);

        // 2. Treinar modelo
        var modelo = TreinarModelo(mlContext, dadosTreino);

        // 3. Salvar modelo
        SalvarModelo(mlContext, modelo, dadosTreino);

        // 4. Carregar modelo para previsão
        var predictor = CriarPredictionEngine(mlContext);

        while (!stoppingToken.IsCancellationRequested)
        {
            var novaEntrada = new DadosRotaEntrada
            {
                DistanciaKm = 18.5f,
                TransitoNivel = 7f,
                Chuva = 1f,
                HoraDoDia = 18f
            };

            var previsao = predictor.Predict(novaEntrada);

            _logger.LogInformation(
                   "Previsão -> Distância: {distancia} km | Trânsito: {transito} | Chuva: {chuva} | Hora: {hora} => Tempo estimado: {tempo} min",
                   novaEntrada.DistanciaKm,
                   novaEntrada.TransitoNivel,
                   novaEntrada.Chuva,
                   novaEntrada.HoraDoDia,
                   previsao.TempoEstimadoMinutos.ToString("F2", CultureInfo.InvariantCulture));

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        }
    }

    private PredictionEngine<DadosRotaEntrada, DadosRotaSaida> CriarPredictionEngine(MLContext mlContext)
    {
        if (!File.Exists(_modeloPath))
            throw new FileNotFoundException("Modelo não encontrado.", _modeloPath);

        var modeloCarregado = mlContext.Model.Load(_modeloPath, out var _);
        return mlContext.Model.CreatePredictionEngine<DadosRotaEntrada, DadosRotaSaida>(modeloCarregado);
    }

    private void SalvarModelo(MLContext mlContext, ITransformer modelo, List<DadosRotaEntrada> dados)
    {
        var dataView = mlContext.Data.LoadFromEnumerable(dados);
        mlContext.Model.Save(modelo, dataView.Schema, _modeloPath);
        _logger.LogInformation("Modelo salvo em: {path}", _modeloPath);
    }

    private ITransformer TreinarModelo(MLContext mlContext, List<DadosRotaEntrada> dadosTreino)
    {
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

        _logger.LogInformation("Modelo treinado com sucesso.");
        _logger.LogInformation("R²: {r2:F4}", metricas.RSquared);
        _logger.LogInformation("RMSE: {rmse:F4}", metricas.RootMeanSquaredError);

        return modelo;
    }

    private static List<DadosRotaEntrada> GerarDadosFakes(int quantidade)
    {
        var random = new Random(123);
        var lista = new List<DadosRotaEntrada>();

        for (int i = 0; i < quantidade; i++)
        {
            var distancia = (float)(random.NextDouble() * 45 + 1); // 1 a 46 km
            var transito = (float)(random.NextDouble() * 10); // 0 a 10
            var chuva = random.Next(0, 2) == 1 ? 1f : 0f; // true ou false
            var hora = (float)random.Next(0, 24); // 0 a 23 horas

            // Fórmula fake, mas coerente
            var tempoBase = distancia * 2.2f; // 2.2 min por km
            var impactoTransito = transito * 3.5f; // cada nível de trânsito adiciona 3.5 min
            var impactoChuva = chuva == 1f ? 8f : 0f; // chuva adiciona 8 min
            var impactoHoraPico = (hora >= 7 && hora <= 9) || (hora >= 17 && hora <= 19) ? 12f : 0f; // hora pico adiciona 12 min

            var tempoFinal = tempoBase + impactoTransito + impactoChuva + impactoHoraPico;

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