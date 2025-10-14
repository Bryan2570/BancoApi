using System;
using System.Collections.Generic;

namespace BancoApi.Models;

public partial class Movimiento
{
    public int IdMovimiento { get; set; }

    public DateTime Fecha { get; set; }

    public string TipoMovimiento { get; set; } = null!;

    public decimal Valor { get; set; }

    public decimal Saldo { get; set; }
}
