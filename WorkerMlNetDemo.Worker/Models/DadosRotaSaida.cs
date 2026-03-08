using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerMlNetDemo.Worker.Models;

public sealed class DadosRotaSaida
{
    [ColumnName("Score")]
    public float TempoEstimadoMinutos { get; set; }
}
