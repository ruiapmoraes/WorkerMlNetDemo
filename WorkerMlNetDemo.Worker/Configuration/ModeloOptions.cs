using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerMlNetDemo.Worker.Configuration;

public sealed class ModeloOptions
{
    public const string SectionName = "Modelo";
    public string CaminhoArquivoModelo { get; set; } = "tempo-rota.zip";
    public string CaminhoArquivoCsv { get; set; } = "dados-tempo-rota.csv";
}
