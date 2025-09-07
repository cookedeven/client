using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class TcpStreamConverter : MonoBehaviour
{
    // JSON 문자열을 직렬화하는 메서드
    public static T DeserializeJson<T>(string json)
    {
        try
        {
            T data = JsonConvert.DeserializeObject<T>(json);
            Debug.Log($"Deserialized JSON: {JsonUtility.ToJson(data)}");
            return data;
        }
        catch (JsonException e)
        {
            Debug.LogError($"Error deserializing JSON: {e.Message}");
            return default;
        }
    }

    // 전체 JSON 문자열을 생성하는 메서드
    public static string GenerateJsonString(string send_type, string command, string uuid, Dictionary<string, object> send_data, Dictionary<string, object> request_data)
    {
        if (send_type != "uuid" && send_type != "name" && send_type != "connect")
        {
            return "{\"error\": \"Invalid send_type\"}";
        }

        if (command != "check" && command != "get")
        {
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
}
