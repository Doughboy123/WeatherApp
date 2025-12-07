using UnityEngine;
using System.Collections;
using System;

public class GetLocation : MonoBehaviour
{
    private bool isInitializing = false;

    public IEnumerator FetchtLocation(Action<float, float> onSuccess, Action<string> onError)
    {
        if (isInitializing)
        {
            onError?.Invoke("Location service is already initializing");
            yield break;
        }

        isInitializing = true;

        if (!Input.location.isEnabledByUser)
        {
            isInitializing = false;
            onError?.Invoke("Location services not enabled by user");
            yield break;
        }

        Input.location.Start(10f, 0.1f);

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1)
        {
            isInitializing = false;
            onError?.Invoke("Location service initialization timed out");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            isInitializing = false;
            onError?.Invoke("Unable to determine device location");
            yield break;
        }

        float latitude = Input.location.lastData.latitude;
        float longitude = Input.location.lastData.longitude;

        isInitializing = false;
        Input.location.Stop();

        onSuccess?.Invoke(latitude, longitude);
    }

    public IEnumerator FetchtLocationWithFallback(Action<float, float> onSuccess)
    {
        bool locationReceived = false;

        yield return FetchtLocation(
            (lat, lon) =>
            {
                locationReceived = true;
                onSuccess?.Invoke(lat, lon);
            },
            (error) =>
            {
                locationReceived = true;
                onSuccess?.Invoke(19.07f, 72.87f); //Mumbai coordinates
            }
        );

        float timeout = 25f;
        while (!locationReceived && timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (!locationReceived)
        {
            onSuccess?.Invoke(19.07f, 72.87f); //Mumbai coordinates
        }
    }
}
