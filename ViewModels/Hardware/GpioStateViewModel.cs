using System;
using System.Collections.Generic;

namespace Integracao.ControlID.PoC.ViewModels.Hardware
{
    /// <summary>
    /// ViewModel para exibir o estado dos GPIOs (entradas e saídas digitais).
    /// </summary>
    public class GpioStateViewModel
    {
        public GpioStateDto GpioState { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;
    }

    // DTO simplificado para estado dos GPIOs
    public class GpioStateDto
    {
        public Dictionary<string, bool> Inputs { get; set; } = new();
        public Dictionary<string, bool> Outputs { get; set; } = new();
        public DateTime? RetrievedAt { get; set; }
        public string Info { get; set; } = string.Empty;
        public string RawJson { get; set; } = string.Empty;
    }
}

