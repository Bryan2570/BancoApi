using System;
using System.Collections.Generic;

namespace BancoApi.Models;

public partial class Cuentum
{
    public int IdCuenta { get; set; }

    public string NumCuenta { get; set; } = null!;

    public string TipoCuenta { get; set; } = null!;

    public decimal? SaldoInicial { get; set; }

    public bool? Estado { get; set; }
}
