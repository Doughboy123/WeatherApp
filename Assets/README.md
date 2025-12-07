# Weather App

Unity application that fetches and displays current weather based on user location using the Open-Meteo API.

## Features

- Automatic location permission request on launch
- Gets user's GPS coordinates
- Fetches weather data from Open-Meteo API
- Displays location, date and timezone
- Uses Toast notifications for temperature
- Asks user to give location permission if denied

### Components:

**Services:**
- **GetLocation** - Handles GPS location fetching using Unity's `Input.location` API. Implements fallback mechanism for editor testing and permission denial scenarios.
- **GetWeather** - Makes HTTP requests to Open-Meteo weather API using `UnityWebRequest`. Parses JSON responses and handles errors gracefully.

- **WeatherController** - Main controller that orchestrates the app flow. Manages permission requests, coordinates between services, and updates UI based on results.

- **WeatherData** - Data models for API responses (`WeatherResponse`, `DailyData`) and display (`WeatherInfo`). Uses `[Serializable]` for JSON parsing.

### Flow:
1. App launches → Checks location permission
2. If denied → Shows permission panel with settings option
3. If granted → User clicks button to fetch weather
4. GetLocation service retrieves coordinates
5. GetWeather service calls API with coordinates
6. WeatherController updates UI with results

## Dependencies

- **Toast Notification Package** - Custom Unity package for displaying toast messages
  - Repository: `https://github.com/Doughboy123/Unity-Toast.git`


## Requirements

- Unity 6000.0 or later
- Internet connection for API calls
- Location permission (Android/iOS)
- Open-Meteo API (no API key required)

## Platform Notes

### Android
- Location permissions requested at runtime
- Requires `ACCESS_FINE_LOCATION` and `INTERNET` permissions in AndroidManifest.xml

### iOS
- Location permission requested on first location access
- Requires "Location Usage Description" in Player Settings

### Unity Editor
- Location services not available
- Automatically uses Mumbai coordinates (19.07, 72.87) for testing

## API Information

**Endpoint:** `https://api.open-meteo.com/v1/forecast`

**Parameters:**
- `latitude` - Location latitude
- `longitude` - Location longitude
- `timezone` - Timezone (auto-detected)
- `daily` - Weather parameters (temperature_2m_max)

## Unit Testing
- Ran out of time and had to skip it.
```