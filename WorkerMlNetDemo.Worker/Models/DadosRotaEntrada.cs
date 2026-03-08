using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerMlNetDemo.Worker.Models;

public sealed class DadosRotaEntrada
{
    [LoadColumn(0)]
    public float DistanciaKm { get; set; }
    [LoadColumn(1)]
    public float TransitoNivel { get; set; }
    [LoadColumn(2)]
    public float Chuva { get; set; } // 0 para não, 1 para sim
    [LoadColumn(3)]
    public float HoraDoDia { get; set; }

    // Label
    [LoadColumn(4)]
    public float TempoEstimadoMinutos { get; set; }
}
