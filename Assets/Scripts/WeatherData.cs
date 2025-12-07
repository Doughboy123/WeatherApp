using System;

[Serializable]
public class WeatherResponse
{
    public float latitude;
    public float longitude;
    public string timezone;
    public DailyData daily;
}

[Serializable]
public class DailyData
{
    public string[] time;
    public float[] temperature_2m_max;
}

public class WeatherInfo
{
    public float Latitude;
    public float Longitude;
    public float Temperature;
    public string Date;
    public string Timezone;

    public override string ToString()
    {
        return $"Temperature: {Temperature}°C\nLocation: ({Latitude}, {Longitude})\nDate: {Date}";
    }
}
