namespace MiningTruckSim.Alerts
{
    /// <summary>
    /// Tipos de alerta de operação (critério 9). Cada tipo tem um procedimento de
    /// conserto distinto (ver <see cref="AlertCatalog"/>).
    /// </summary>
    public enum AlertType
    {
        /// <summary>Pressão de óleo alta — exige reduzir RPM e aguardar estabilizar.</summary>
        HighOilPressure,

        /// <summary>Filtro de ar entupido — exige parar e executar a troca/limpeza do filtro.</summary>
        CloggedFilter,

        /// <summary>Saída do trilho — exige retornar para dentro da tolerância do trilho.</summary>
        OffTrack,

        /// <summary>Excesso de carga — exige aliviar a carga (descarregar) até a faixa nominal.</summary>
        Overload
    }
}
