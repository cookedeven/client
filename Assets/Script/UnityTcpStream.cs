using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

public class UnityTcpStream : MonoBehaviour
{
    [SerializeField]
    private TcpClient client;
    private NetworkStream stream;
    [SerializeField]
    public IPAddress serverAddress;
    public string serverIpv4; // 연결할 서버 IP
    public int serverPort; // 연결할 서버 포트
    public string uuid; // UUID
    private Dictionary<string, object> data_cache = new Dictionary<string, object>();

    private static UnityTcpStream _instance;

    public static UnityTcpStream instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UnityTcpStream>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("RecoveryNetworkManager");
                    _instance = obj.AddComponent<UnityTcpStream>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        Debug.Log("UnityTcpStream Awake called");
        InitializedConnectServer(serverIpv4, serverPort);
        InitializedUuid();
    }

    public void InitializedConnectServer(string ipv4, int port)
    {
        // MonoBehaviour의 Awake 메서드에서 TcpClient를 초기화합니다.
        if (string.IsNullOrEmpty(ipv4) || port <= 0)
        {
            Debug.LogError("Server IP or Port is not set.");
            return;
        }
        InitializedTcpStream();
    }

    public string InitializedUuid()
    {
        return InitializedUuidInternal("get", null);
    }

    public string InitializedUuid(string uuid)
    {
        return InitializedUuidInternal("check", uuid);
    }

    private string InitializedUuidInternal(string command, string uuidValue)
    {
        TcpStreamData data = SendAndReceive("uuid", command, uuidValue, new Dictionary<string, object>(), new List<string>());
        Debug.Log($"Received data: {data}");

        if (data.send_type == "ok")
        {
            uuid = data.uuid;
            return uuid;
        }
        else
        {
            Debug.LogError($"Failed to {(command == "get" ? "get" : "check")} UUID. type: {data.send_type}, error_level: {data.command}");
            return null;
        }
    }


    public T NewGetData<T>(string name_space)
    {
        List<string> request_data = new List<string>();
        request_data.Add(name_space);

        TcpStreamData data = SendAndReceive("uuid", "get", uuid, new Dictionary<string, object>(), request_data);
        Debug.Log($"Received data: {data.send_data[name_space]}");
        if (data.send_data.ContainsKey(name_space))
        {
            data_cache[name_space] = data.send_data[name_space];
            return JsonConvert.DeserializeObject<T>(data.send_data[name_space].ToString());
        }
        else
        {
            Debug.LogError($"Key '{name_space}' not found in send_data.");
            return default;
        }
    }

    public object GetData(string name_space)
    {
        object cachedData = data_cache.GetValueOrDefault(name_space);
        if (cachedData != null)
        {
            Debug.Log($"Returning cached data for {name_space}");
            return cachedData;
        } else
        {
            Debug.LogWarning($"No cached data found for {name_space}.");
            return null;
        }
    }

    private TcpStreamData SendAndReceive(string send_type, string command, string uuid, Dictionary<string, object> send_data, List<string> request_data)
    {
        // JSON 문자열을 생성합니다.
        string send_message = TcpStreamConverter.GenerateJsonString(send_type, command, uuid, send_data, request_data);
        Debug.Log($"Send message: {send_message}");
        // 서버에 메시지를 보냅니다.
        ServerWrite(send_message);
        // 서버로부터 응답을 읽습니다.
        string read_data = ServerRead(1024);
        Debug.Log($"Read data: {read_data}");
        // JSON 문자열을 파싱합니다.
        TcpStreamData data = TcpStreamConverter.DeserializeJson<TcpStreamData>(read_data);
        Debug.Log($"Received data: {data.send_data}");
        // 응답 데이터를 반환합니다.
        if (data.send_type == "ok")
        {
            return data;
        }
        else
        {
            Debug.LogError($"Failed to {request_data}. type: {data.send_type}, error_level: {data.command}");
            return default;
        }
    }

    public void ServerWrite(string msg)
    {
        _ = AsyncWriteData(msg).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Error: {task.Exception}");
            }
            else
            {
                Debug.Log($"Message sent: {msg}");
            }
        });
    }

    public string ServerRead(int buffer_size)
    {
        string result = null;
        _ = AsyncReadData(buffer_size).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Error: {task.Exception}");
            }
            else
            {
                result = task.Result;
            }
        });
        return result;
    }

    private void InitializedTcpStream()
    {
        _ = InitializedConnectServer().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Error: {task.Exception}");
            }
            else
            {
                Debug.Log($"connect to server");
            }
        });
    }

    private async Task InitializedConnectServer()
    {
        client = new TcpClient();
        Debug.Log("Created TcpClient");

        serverAddress = IPAddress.Parse(serverIpv4);

        await client.ConnectAsync(serverAddress, serverPort);
        Debug.Log("ConnectAsync completed");

        stream = client.GetStream();
        Debug.Log("Got NetworkStream from TcpClient");
    }

    private async Task AsyncWriteData(string msg)
    {
        byte[] request = Encoding.UTF8.GetBytes(msg);
        await stream.WriteAsync(request, 0, request.Length);
    }

    private async Task<string> AsyncReadData(int buffer_size)
    {
        byte[] buffer = new byte[buffer_size];
        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
        return Encoding.UTF8.GetString(buffer, 0, bytesRead);
    }
}
