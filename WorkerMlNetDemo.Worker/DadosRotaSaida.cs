using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerMlNetDemo.Worker;

public sealed class DadosRotaSaida
{
    [ColumnName("Score")]
    public float TempoEstimadoMinutos { get; set; }
}
