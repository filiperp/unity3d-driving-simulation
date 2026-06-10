using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace MiningTruckSim.Net
{
    /// <summary>
    /// Cliente da API FastAPI (critério 7) via <see cref="UnityWebRequest"/>. Cobre o
    /// fluxo de jogo: criar/listar usuários, criar a operação (sessão), enviar o
    /// resultado de cada ciclo, finalizar a operação e ler o leaderboard.
    ///
    /// Cada chamada é uma corrotina com callbacks de sucesso/erro, para não bloquear o
    /// jogo. A URL base aponta para o backend local por padrão.
    /// </summary>
    public sealed class MiningApiClient : MonoBehaviour
    {
        [Tooltip("URL base da API FastAPI (sem barra final).")]
        public string baseUrl = "http://127.0.0.1:8000";

        public float timeoutSeconds = 10f;

        // ---- Usuários --------------------------------------------------------
        public Coroutine CreateUser(string username, string displayName,
            Action<UserDto> onOk, Action<string> onError)
        {
            var body = new UserCreateDto { username = username, display_name = displayName };
            return StartCoroutine(Send("POST", "/users", JsonUtility.ToJson(body), onOk, onError));
        }

        public Coroutine ListUsers(Action<UserDto[]> onOk, Action<string> onError)
        {
            return StartCoroutine(SendArray("/users", onOk, onError));
        }

        // ---- Operação / sessão ----------------------------------------------
        public Coroutine CreateSession(int userId, string mineId, int cyclesPlanned,
            Action<SessionDto> onOk, Action<string> onError)
        {
            var body = new SessionCreateDto
            {
                user_id = userId,
                mine = mineId,
                cycles_planned = cyclesPlanned
            };
            return StartCoroutine(Send("POST", "/sessions", JsonUtility.ToJson(body), onOk, onError));
        }

        public Coroutine AddCycle(int sessionId, CycleResultCreateDto cycle,
            Action<string> onOk, Action<string> onError)
        {
            return StartCoroutine(Send<object>(
                "POST", $"/sessions/{sessionId}/cycles", JsonUtility.ToJson(cycle),
                _ => onOk?.Invoke("ok"), onError));
        }

        public Coroutine FinishSession(int sessionId,
            Action<SessionDto> onOk, Action<string> onError)
        {
            return StartCoroutine(Send<SessionDto>(
                "POST", $"/sessions/{sessionId}/finish", "{}", onOk, onError));
        }

        // ---- Leaderboard -----------------------------------------------------
        public Coroutine GetLeaderboard(Action<LeaderboardEntryDto[]> onOk, Action<string> onError)
        {
            return StartCoroutine(SendArray("/leaderboard", onOk, onError));
        }

        // ---- Infra -----------------------------------------------------------
        private IEnumerator Send<T>(string method, string path, string json,
            Action<T> onOk, Action<string> onError) where T : class
        {
            using UnityWebRequest req = BuildRequest(method, path, json);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(DescribeError(req));
                yield break;
            }

            try
            {
                T parsed = typeof(T) == typeof(object)
                    ? null
                    : JsonUtility.FromJson<T>(req.downloadHandler.text);
                onOk?.Invoke(parsed);
            }
            catch (Exception e)
            {
                onError?.Invoke($"Falha ao interpretar resposta: {e.Message}");
            }
        }

        /// <summary>Desserializa um endpoint que devolve um array JSON de topo.</summary>
        private IEnumerator SendArray<T>(string path, Action<T[]> onOk, Action<string> onError)
        {
            using UnityWebRequest req = BuildRequest("GET", path, null);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(DescribeError(req));
                yield break;
            }

            try
            {
                // JsonUtility não lê arrays de topo: embrulhamos em {"items":[...]}.
                string wrapped = "{\"items\":" + req.downloadHandler.text + "}";
                ListWrapper<T> list = JsonUtility.FromJson<ListWrapper<T>>(wrapped);
                onOk?.Invoke(list?.items ?? Array.Empty<T>());
            }
            catch (Exception e)
            {
                onError?.Invoke($"Falha ao interpretar lista: {e.Message}");
            }
        }

        private UnityWebRequest BuildRequest(string method, string path, string json)
        {
            var req = new UnityWebRequest(baseUrl + path, method)
            {
                downloadHandler = new DownloadHandlerBuffer(),
                timeout = Mathf.CeilToInt(timeoutSeconds)
            };

            if (!string.IsNullOrEmpty(json))
            {
                byte[] payload = Encoding.UTF8.GetBytes(json);
                req.uploadHandler = new UploadHandlerRaw(payload);
                req.SetRequestHeader("Content-Type", "application/json");
            }

            return req;
        }

        private static string DescribeError(UnityWebRequest req)
        {
            string detail = req.downloadHandler != null ? req.downloadHandler.text : "";
            return $"[{(int)req.responseCode}] {req.error} {detail}".Trim();
        }
    }
}
