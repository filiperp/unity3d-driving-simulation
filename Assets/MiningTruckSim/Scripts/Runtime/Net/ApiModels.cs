using System;

namespace MiningTruckSim.Net
{
    /// <summary>
    /// DTOs de transporte para a API FastAPI (critério 7). Os nomes dos campos usam
    /// snake_case para casar com o JSON do backend via <c>JsonUtility</c> (que serializa
    /// pelos nomes dos campos). São tipos puros/serializáveis.
    /// </summary>
    [Serializable]
    public class UserDto
    {
        public int id;
        public string username;
        public string display_name;
    }

    [Serializable]
    public class UserCreateDto
    {
        public string username;
        public string display_name;
    }

    [Serializable]
    public class SessionCreateDto
    {
        public int user_id;
        public string mine;          // "easy" | "hard"
        public int cycles_planned;
    }

    [Serializable]
    public class SessionDto
    {
        public int id;
        public int user_id;
        public string mine;
        public int cycles_planned;
        public string status;
        public float total_score;
    }

    [Serializable]
    public class CycleResultCreateDto
    {
        public int cycle_index;
        public float score;
        public float time_in_band_s;
        public int off_track_penalties;
        public int alerts_handled;
        public float load_tonnes;
    }

    [Serializable]
    public class LeaderboardEntryDto
    {
        public int session_id;
        public string username;
        public string display_name;
        public string mine;
        public int cycles_planned;
        public float total_score;
    }

    /// <summary>
    /// Wrapper para desserializar arrays JSON de topo (JsonUtility não desserializa
    /// arrays diretamente; embrulhamos em <c>{"items":[...]}</c> no cliente).
    /// </summary>
    [Serializable]
    public class ListWrapper<T>
    {
        public T[] items;
    }
}
