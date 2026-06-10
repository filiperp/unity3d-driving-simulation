using MiningTruckSim.Config;
using MiningTruckSim.Net;
using UnityEngine;

namespace MiningTruckSim.UI
{
    /// <summary>
    /// Login/seleção de usuário contra o backend (critério 7). Permite criar um novo
    /// usuário ou escolher um existente; o selecionado é gravado em
    /// <see cref="OperationContext"/> para a operação ser associada a ele. Tolerante a
    /// API offline: nesse caso segue como "Convidado" (jogo local). IMGUI provisório.
    /// </summary>
    public sealed class UserLoginMenu : MonoBehaviour
    {
        public MiningApiClient api;

        private string _newUsername = "";
        private string _newDisplayName = "";
        private UserDto[] _users = System.Array.Empty<UserDto>();
        private string _status = "";
        private Vector2 _scroll;
        private GUIStyle _title;

        private void Start()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (api == null)
            {
                return;
            }

            _status = "Carregando usuários…";
            api.ListUsers(
                users =>
                {
                    _users = users;
                    _status = $"{users.Length} usuário(s).";
                },
                err => _status = "API offline — jogando como Convidado. " + err);
        }

        private void OnGUI()
        {
            _title ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            const float w = 420f;
            const float h = 360f;
            var rect = new Rect(20, 20, w, h);
            GUI.Box(rect, GUIContent.none);

            GUILayout.BeginArea(new Rect(rect.x + 16, rect.y + 12, rect.width - 32, rect.height - 24));
            GUILayout.Label("Usuário", _title);
            GUILayout.Label($"Ativo: {OperationContext.ActiveUserName}");
            GUILayout.Space(8);

            GUILayout.Label("Criar novo:");
            _newUsername = NamedField("username", _newUsername);
            _newDisplayName = NamedField("nome", _newDisplayName);
            if (GUILayout.Button("Criar e selecionar") && !string.IsNullOrWhiteSpace(_newUsername))
            {
                CreateUser();
            }

            GUILayout.Space(10);
            GUILayout.Label("Ou escolher existente:");
            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(120));
            foreach (UserDto u in _users)
            {
                if (GUILayout.Button($"{u.display_name} (@{u.username})"))
                {
                    Select(u);
                }
            }

            GUILayout.EndScrollView();

            GUILayout.Space(6);
            GUILayout.Label(_status);
            GUILayout.EndArea();
        }

        private static string NamedField(string label, string value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(80));
            string result = GUILayout.TextField(value ?? "");
            GUILayout.EndHorizontal();
            return result;
        }

        private void CreateUser()
        {
            _status = "Criando usuário…";
            api.CreateUser(_newUsername, string.IsNullOrWhiteSpace(_newDisplayName) ? _newUsername : _newDisplayName,
                user =>
                {
                    Select(user);
                    Refresh();
                },
                err => _status = "Erro ao criar: " + err);
        }

        private void Select(UserDto user)
        {
            OperationContext.ActiveUserId = user.id;
            OperationContext.ActiveUserName = string.IsNullOrEmpty(user.display_name)
                ? user.username
                : user.display_name;
            _status = $"Selecionado: {OperationContext.ActiveUserName}";
        }
    }
}
