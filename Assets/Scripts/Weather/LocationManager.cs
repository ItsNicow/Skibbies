using UnityEngine;
using TMPro;
using UnityEngine.Android;
using System.Collections;

public class LocationManager : MonoBehaviour
{
    public TMP_Text testDisplayText;

    void Start()
    {
        if (SystemInfo.deviceType != DeviceType.Handheld)
        {
            Debug.Log("Running on non-handheld device. Location-related features are NOT expected to function.");
            return;
        }

        InitLocation();
    }

    void Update()
    {
        if (!Input.location.isEnabledByUser)
        {
            testDisplayText.text = "Localisation: OFF";
            return;
        }
        testDisplayText.text = "Localisation: ON";
    }

    void InitLocation()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation))
        {
            Permission.RequestUserPermission(Permission.CoarseLocation);
            Debug.Log("no permission");
        }
        else
        {
            StartLocationServices();
        }
    }

    void StartLocationServices()
    {
        if (!Input.location.isEnabledByUser)
        {
            testDisplayText.text = "Localisation: OFF";
            PromptEnableLocation();
            return;
        }
        testDisplayText.text = "Localisation: ON";

        StartCoroutine(StartLocation());
    }

    void PromptEnableLocation()
    {
        testDisplayText.text = "Please enable localisation.";

        #if UNITY_ANDROID
        OpenLocationSettings();
        #endif
    }

    void OpenLocationSettings()
    {
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", "android.settings.LOCATION_SOURCE_SETTINGS");
                currentActivity.Call("startActivity", intent);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to open location settings: " + e.Message);
        }
    }

    IEnumerator StartLocation()
    {
        Input.location.Start();

        int timeout = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && timeout > 0)
        {
            yield return new WaitForSeconds(1);
            timeout--;
        }

        if (timeout <= 0)
        {
            testDisplayText.text = "Timed out initializing location services.";
            yield break;
        }
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            testDisplayText.text = "Failed to determine device location.";
            yield break;
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            StartLocationServices();
        }
    }
}