namespace MiningTruckSim.Operation
{
    /// <summary>
    /// Fases do ciclo de operação de um caminhão de mineração (critérios 2, 3, 4).
    /// </summary>
    public enum CyclePhase
    {
        /// <summary>Início; precisa dirigir até a área de loading.</summary>
        Idle,

        /// <summary>Dentro da área de loading, ainda não estacionado/alinhado no ponto.</summary>
        ApproachingLoad,

        /// <summary>Estacionado no ponto de carregamento; escavadeira carregando.</summary>
        Loading,

        /// <summary>Carga completa; precisa dirigir até a área de unload (critério 3).</summary>
        Loaded,

        /// <summary>Dentro da área de unload, ainda não estacionado no ponto.</summary>
        ApproachingUnload,

        /// <summary>No ponto de unload com a báscula levantando para despejar.</summary>
        Unloading,

        /// <summary>Ciclo concluído.</summary>
        Done
    }
}
