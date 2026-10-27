using UnityEngine;
using FishNet.Managing;

public class NetworkHUD : MonoBehaviour
{
    private NetworkManager _networkManager;

    private void Awake()
    {
        _networkManager = GetComponent<NetworkManager>();
    }

    private void OnGUI()
    {
        if (_networkManager == null) return;

        // 화면 좌상단에 버튼 배치
        GUILayout.BeginArea(new Rect(10, 10, 150, 200));

        // 서버/클라이언트 상태에 따라 버튼 표시
        if (!_networkManager.IsServerStarted && !_networkManager.IsClientStarted)
        {
            if (GUILayout.Button("Start Host (Server + Client)"))
            {
                _networkManager.ServerManager.StartConnection();
                _networkManager.ClientManager.StartConnection();
            }

            if (GUILayout.Button("Start Server Only"))
            {
                _networkManager.ServerManager.StartConnection();
            }

            if (GUILayout.Button("Start Client Only"))
            {
                _networkManager.ClientManager.StartConnection();
            }
        }
        else
        {
            if (GUILayout.Button("Stop Network"))
            {
                _networkManager.ServerManager.StopConnection(true);
                _networkManager.ClientManager.StopConnection();
            }
        }

        GUILayout.EndArea();
    }
}