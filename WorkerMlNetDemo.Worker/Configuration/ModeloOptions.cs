using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerMlNetDemo.Worker.Configuration;

public sealed class ModeloOptions
{
    public const string SectionName = "Modelo";
    public string CaminhoArquivo { get; set; } = "tempo-rota.zip";
}
