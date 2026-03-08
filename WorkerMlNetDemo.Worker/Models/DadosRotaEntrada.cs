using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerMlNetDemo.Worker.Models;

public sealed class DadosRotaEntrada
{
    public float DistanciaKm { get; set; }
    public float TransitoNivel { get; set; }
    public float Chuva { get; set; } // 0 para não, 1 para sim
    public float HoraDoDia { get; set; }

    // Label
    public float TempoEstimadoMinutos { get; set; }
}
