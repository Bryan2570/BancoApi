using System;
using System.Collections.Generic;

namespace BancoApi.Models;

public partial class Cliente
{
    public int IdCliente { get; set; }

    public string Contrasena { get; set; } = null!;

    public bool Estado { get; set; }

    public int IdPersona { get; set; }

    public virtual Persona IdPersonaNavigation { get; set; } = null!;
}
