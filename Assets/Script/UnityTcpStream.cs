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
    public IPAddress serverAddress;
    public string serverIpv4; // ������ ���� IP
    public int serverPort; // ������ ���� ��Ʈ

    private void Awake()
    {
        Debug.Log("UnityTcpStream Awake called");
        
        // MonoBehaviour�� Awake �޼��忡�� TcpClient�� �ʱ�ȭ�մϴ�.
        if (string.IsNullOrEmpty(serverIpv4) || serverPort <= 0)
        {
            Debug.LogError("Server IP or Port is not set.");
            return;
        }
        InitializedTcpStream();
    }

    public static string GenerateJsonString(string send_type = "connect", string command = null, string uuid = "", Dictionary<string, object> send_data = null, List<string> request_data = null)
    {
        if (send_type != "uuid" && send_type != "name" && send_type != "connect") {
            return "{\"error\": \"Invalid send_type\"}";
        }

        if (command != "check" && command != "get" && command != "send" && command != "request" && command != "both") {
            return "{\"error\": \"Invalid command\"}";
        }

        TcpStreamData data = new TcpStreamData
        {
            send_type = send_type,
            command = command,
            uuid = uuid,
            send_data = send_data,
            request_data = request_data
        };

        Debug.Log($"Generated JSON: {JsonUtility.ToJson(data)}");

        string json = JsonConvert.SerializeObject(data);

        Debug.Log($"Generated JSON String: {json}");

        return json;
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

        string elcomeMessage = GenerateJsonString("uuid", "get", "");
        Debug.Log($"Welcome message: {elcomeMessage}");

        await AsyncWriteData(elcomeMessage);
        Debug.Log("AsyncWriteData completed");

        string read_data = await AsyncReadData(1024);
        Debug.Log($"Received data: {read_data}");
    }

    /*
    private async Task InitializedConnectServer()
    {
        try
        {
            client = new TcpClient();
            // ������ ������ �õ��մϴ�.
            await client.ConnectAsync(ServerIp, ServerPort);
            bool connect = await VerifyConnection();
            Debug.Log($"Connected to server: {connect}");
            await AsyncWriteData(
                GenerateJsonString("uuid", "get", "", new Dictionary<string, object>(), new List<string>())
            );
        }
        catch (SocketException ex)
        {
            Debug.LogError($"SocketException: {ex.Message}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception: {ex.Message}");
        }
    }
    */

    public string InitializedCheckUuid(string uuid)
    {
        // UUID�� �񵿱������� �ʱ�ȭ�մϴ�.
        // �� �޼���� Unity�� MonoBehaviour���� �񵿱� �۾��� ó���ϱ� ���� ���˴ϴ�.
        string response = null;
        _ = InitializedAsyncCheckUuid(uuid).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Error: {task.Exception}");
            }
            else
            {
                response = task.Result;
            }
        });
        return response;
    }

    private async Task<string> InitializedAsyncCheckUuid(string uuid)
    {
        // UUID�� �񵿱������� �ʱ�ȭ�մϴ�.
        string jsonString = GenerateJsonString("uuid", "check", uuid);
        await AsyncWriteData(jsonString);
        // �����κ��� UUID Ȯ�� ������ �޽��ϴ�.
        string response = await AsyncReadData(1024);
        return response;
    }

    public string InitializedGetUuid()
    {
        // UUID�� �񵿱������� �ʱ�ȭ�մϴ�.
        // �� �޼���� Unity�� MonoBehaviour���� �񵿱� �۾��� ó���ϱ� ���� ���˴ϴ�.
        string uuid = null;
        _ = InitializedAsyncGetUuid().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Error: {task.Exception}");
            }
            else
            {
                uuid = task.Result;
            }
        });
        return uuid;
    }

    private async Task<string> InitializedAsyncGetUuid()
    {
        // UUID ���� ��û
        string jsonString = GenerateJsonString("uuid", "get", "", new Dictionary<string, object>(), new List<string>());
        await AsyncWriteData(jsonString);
        // �����κ��� UUID�� �޽��ϴ�.
        string uuid = await AsyncReadData(1024);
        return uuid;
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
