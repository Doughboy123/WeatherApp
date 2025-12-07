using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class GetWeather : MonoBehaviour
{
    private const string API_BASE_URL = "https://api.open-meteo.com/v1/forecast";

    public IEnumerator FetchWeather(float latitude, float longitude, Action<WeatherInfo> onSuccess, Action<string> onError)
    {
        string url = $"{API_BASE_URL}?latitude={latitude}&longitude={longitude}&timezone=auto&daily=temperature_2m_max";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Network error: {request.error}");
                yield break;
            }

            string jsonResponse = request.downloadHandler.text;

            try
            {
                WeatherResponse weatherResponse = JsonUtility.FromJson<WeatherResponse>(jsonResponse);

                if (weatherResponse.daily.temperature_2m_max != null && weatherResponse.daily.temperature_2m_max.Length > 0)
                {
                    WeatherInfo weatherInfo = new WeatherInfo
                    {
                        Latitude = weatherResponse.latitude,
                        Longitude = weatherResponse.longitude,
                        Temperature = weatherResponse.daily.temperature_2m_max[0],
                        Date = weatherResponse.daily.time[0],
                        Timezone = weatherResponse.timezone
                    };

                    onSuccess?.Invoke(weatherInfo);
                }
                else
                {
                    onError?.Invoke("No temperature data available");
                }
            }
            catch (Exception e)
            {
                onError?.Invoke($"Failed to parse weather data: {e.Message}");
            }
        }
    }
}