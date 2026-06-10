namespace MiningTruckSim.Config
{
    /// <summary>
    /// Portador estático da configuração escolhida no menu, lido pela cena de operação
    /// (critério 8). Evita acoplar o menu à cena: o menu grava aqui e a cena lê. Também
    /// guarda o usuário ativo (critério 7) para a integração com o backend na Sprint 7.
    /// </summary>
    public static class OperationContext
    {
        /// <summary>Configuração selecionada (mina + N ciclos). Default = mina fácil.</summary>
        public static OperationConfig Config { get; set; } = OperationConfig.Default;

        /// <summary>Id do usuário ativo no backend (0 = nenhum). Preenchido na Sprint 7.</summary>
        public static int ActiveUserId { get; set; }

        /// <summary>Nome do usuário ativo (para exibição). Preenchido na Sprint 7.</summary>
        public static string ActiveUserName { get; set; } = "Convidado";
    }
}
