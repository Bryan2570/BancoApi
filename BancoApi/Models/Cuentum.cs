using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BancoApi.Models;

public partial class Cuentum
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdCuenta { get; set; }

    public string NumCuenta { get; set; } = null!;

    public string TipoCuenta { get; set; } = null!;

    public decimal? SaldoInicial { get; set; }

    public bool? Estado { get; set; }

    [ForeignKey("idCliente")]
    public int IdCliente { get; set; }

  
}
