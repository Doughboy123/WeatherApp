using UnityEngine;
using UnityEngine.UI;
using CleverTap.ToastNotification;

public class WeatherController : MonoBehaviour
{
    public Button getWeatherButton;
    public Text statusText;
    public Text weatherInfoText;
    public GameObject permissionPanel;
    public Text permissionText;
    public Button openSettingsButton;

    private GetLocation getLocation;
    private GetWeather getWeather;

    private bool isFetching = false;
    private bool hasLocationPermission = false;
    private bool isWaitingForPermissionResponse = false;

    private void Awake()
    {
        getLocation = gameObject.AddComponent<GetLocation>();
        getWeather = gameObject.AddComponent<GetWeather>();
    }

    private void Start()
    {
        if (getWeatherButton != null)
        {
            getWeatherButton.onClick.AddListener(OnGetWeatherClicked);
            getWeatherButton.interactable = false;
        }

        if (openSettingsButton != null)
        {
            openSettingsButton.onClick.AddListener(OnOpenSettingsClicked);
        }

        if (permissionPanel != null)
        {
            permissionPanel.SetActive(false);
        }

        StartCoroutine(CheckLocationPermission());
    }

    private System.Collections.IEnumerator CheckLocationPermission()
    {
        UpdateStatus("Checking location permission...");

#if UNITY_EDITOR
        hasLocationPermission = true;
        ShowPermissionGrantedUI();
        yield break;
#endif

#if UNITY_ANDROID
        bool hasPermission = UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation);

        if (!hasPermission)
        {
            UpdateStatus("Requesting location permission...");
            
            isWaitingForPermissionResponse = true;
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.FineLocation);
            
            yield break;
        }
        else
        {
            hasLocationPermission = true;
            ShowPermissionGrantedUI();
        }
#elif UNITY_IOS
        if (!Input.location.isEnabledByUser)
        {
            ShowPermissionDeniedUI();
            yield break;
        }
        
        hasLocationPermission = true;
        ShowPermissionGrantedUI();
#endif
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && isWaitingForPermissionResponse)
        {
            isWaitingForPermissionResponse = false;

#if UNITY_ANDROID
            bool hasPermission = UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation);
            
            if (hasPermission)
            {
                hasLocationPermission = true;
                ShowPermissionGrantedUI();
            }
            else
            {
                hasLocationPermission = false;
                ShowPermissionDeniedUI();
            }
#endif
        }
        else if (hasFocus && !hasLocationPermission)
        {
#if UNITY_ANDROID
            bool hasPermission = UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation);
            
            if (hasPermission)
            {
                hasLocationPermission = true;
                ShowPermissionGrantedUI();
            }
#endif
        }
    }

    private void ShowPermissionDeniedUI()
    {
        hasLocationPermission = false;

        UpdateStatus("Location permission required");

        if (permissionPanel != null)
            permissionPanel.SetActive(true);

        if (permissionText != null)
            permissionText.text = "This app requires location permission to show weather at your location.\n\n" +
                                 "Please enable location services in your device settings.";

        if (getWeatherButton != null)
            getWeatherButton.interactable = false;
    }

    private void ShowPermissionGrantedUI()
    {
        hasLocationPermission = true;

        UpdateStatus("Ready. Click the button to get weather.");

        if (permissionPanel != null)
        {
            permissionPanel.SetActive(false);
        }

        if (getWeatherButton != null)
        {
            getWeatherButton.interactable = true;
        }
    }

    private void OnOpenSettingsClicked()
    {
#if UNITY_ANDROID
        try
        {
            using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var currentActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var intent = new AndroidJavaObject("android.content.Intent", "android.settings.APPLICATION_DETAILS_SETTINGS"))
            using (var uri = new AndroidJavaClass("android.net.Uri"))
            {
                var uriObject = uri.CallStatic<AndroidJavaObject>("fromParts", "package", Application.identifier, null);
                intent.Call<AndroidJavaObject>("setData", uriObject);
                currentActivity.Call("startActivity", intent);
            }

        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to open settings: {e.Message}");
        }
#elif UNITY_IOS
        Application.OpenURL("app-settings:");
#endif
    }

    public void OnGetWeatherClicked()
    {
        if (!hasLocationPermission)
        {
            ShowPermissionDeniedUI();
            return;
        }

        if (isFetching)
        {
            return;
        }

        StartCoroutine(FetchWeatherData());
    }

    private System.Collections.IEnumerator FetchWeatherData()
    {
        isFetching = true;
        UpdateStatus("Getting your location...");

        float latitude = 0f;
        float longitude = 0f;
        bool locationReceived = false;

        yield return getLocation.FetchtLocationWithFallback((lat, lon) =>
        {
            latitude = lat;
            longitude = lon;
            locationReceived = true;
        });

        if (!locationReceived)
        {
            UpdateStatus("Failed to get location");
            ToastManager.Show("Location unavailable", 3f);
            isFetching = false;
            yield break;
        }

        UpdateStatus($"Location: {latitude:F2}, {longitude:F2}");

        yield return new WaitForSeconds(1f);

        UpdateStatus("Fetching weather data...");

        bool weatherReceived = false;

        yield return getWeather.FetchWeather(
            latitude,
            longitude,
            (weatherInfo) =>
            {
                weatherReceived = true;
                OnWeatherReceived(weatherInfo);
            },
            (error) =>
            {
                weatherReceived = true;
                OnWeatherError(error);
            }
        );

        if (!weatherReceived)
        {
            UpdateStatus("Weather fetch timeout");
            ToastManager.Show("Failed to get weather", 3f);
        }

        isFetching = false;
    }

    private void OnWeatherReceived(WeatherInfo weather)
    {
        UpdateStatus("Weather data received!");

        if (weatherInfoText != null)
        {
            weatherInfoText.text = $"Location: {weather.Latitude:F2}, {weather.Longitude:F2}\n" +
                                  $"Date: {weather.Date}\n" +
                                  $"Timezone: {weather.Timezone}";
        }

        ToastManager.Show($"Temperature: {weather.Temperature}°C at your location", 4f);
    }

    private void OnWeatherError(string error)
    {
        UpdateStatus($"Error: {error}");
        ToastManager.Show($"Error: {error}", 4f);

        if (weatherInfoText != null)
            weatherInfoText.text = "Failed to get weather data";
    }

    private void UpdateStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
}