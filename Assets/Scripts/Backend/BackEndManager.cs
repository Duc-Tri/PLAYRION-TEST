using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

public class BackEndManager : MonoBehaviour
{
    //public const string SERVER = "http://localhost/request.php";
    public const string SERVER = "https://pyxxym-studio.com/playrion/request.php";

    public static Action<List<float>> OnServerGetMachinesSpeed;
    public static Action<float> OnServerGetPassengerSpeed;
    public static Action<bool> OnServerGetPassengerBehavior;

    private static bool RequestMachinesSpeed_ongoing, RequestPassengerBehavior_ongoing, RequestPassengerFreeze_ongoing;

    private void Start()
    {
        //StartCoroutine(SendNewGET());
        RequestMachinesSpeed_ongoing = RequestPassengerBehavior_ongoing = RequestPassengerFreeze_ongoing = false;
        StartCoroutine(LaunchRequests());
    }

    readonly WaitForSeconds oneSec = new WaitForSeconds(1);
    private IEnumerator LaunchRequests()
    {
        while (true)
        {
            if (!RequestMachinesSpeed_ongoing)
                StartCoroutine(RequestMachinesSpeed());

            if (!RequestPassengerBehavior_ongoing)
                StartCoroutine(RequestPassengerBehavior());

            if (!RequestPassengerFreeze_ongoing)
                StartCoroutine(RequestPassengerFreeze());

            yield return oneSec;
        }
    }

    private IEnumerator RequestMachinesSpeed()
    {
        RequestMachinesSpeed_ongoing = true;
        using (UnityWebRequest www = UnityWebRequest.Get(SERVER + "?request=machines_speed"))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                string responseText = www.downloadHandler.text;
                // Show results as text
                Debug.Log(SERVER + " * RequestMachinesSpeed____________________GET from server: " + responseText);

                string[] values = responseText.Split(',');
                List<float> speeds = new List<float>();
                foreach (string value in values)
                {
                    float speed = 0;
                    // (value.EndsWith('f') ? value : value + 'f')
                    //if (float.TryParse(value, out speed))
                    speed = float.Parse(value, CultureInfo.InvariantCulture.NumberFormat);

                    {
                        speeds.Add(speed);
                    }
                }
                Debug.Log(SERVER + " * RequestMachinesSpeed____________________RequestMachinesSpeed >>>>>>>>>>>> " + string.Join("_", speeds) + " /// " + string.Join("_", values));

                OnServerGetMachinesSpeed?.Invoke(speeds);
            }
        }
        RequestMachinesSpeed_ongoing = false;
    }

    static readonly Dictionary<string, float> speedFactors = new Dictionary<string, float>() { { "hurried", 1.5f }, { "normal", 1f }, { "lazy", 0.5f } };

    private IEnumerator RequestPassengerBehavior()
    {
        RequestPassengerBehavior_ongoing = true;
        using (UnityWebRequest www = UnityWebRequest.Get(SERVER + "?request=passenger_speed"))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                string responseText = www.downloadHandler.text.Trim().ToLower();

                Debug.Log(SERVER + " * RequestPassengerBehavior____________________GET from server: " + responseText);

                if (speedFactors.ContainsKey(responseText))
                    OnServerGetPassengerSpeed?.Invoke(speedFactors[responseText]);
            }
        }
        RequestPassengerBehavior_ongoing = false;
    }

    private IEnumerator RequestPassengerFreeze()
    {
        RequestPassengerFreeze_ongoing = true;
        using (UnityWebRequest www = UnityWebRequest.Get(SERVER + "?request=passenger_behavior"))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                string responseText = www.downloadHandler.text.Trim().ToLower();

                Debug.Log(SERVER + " * RequestPassengerFreeze____________________GET from server: " + responseText);

                if (responseText.Equals("freeze")) OnServerGetPassengerBehavior?.Invoke(true);
                else if (responseText.Equals("unfreeze")) OnServerGetPassengerBehavior?.Invoke(false);

                //if (bool.TryParse(responseText, out freeze))
            }

        }
        RequestPassengerFreeze_ongoing = false;
    }

    IEnumerator SendNewGET()
    {
        using (UnityWebRequest www = UnityWebRequest.Get("http://localhost?getdata=Coucou"))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                // Show results as text
                Debug.Log("GET from server: " + www.downloadHandler.text);

                // Or retrieve results as binary data
                byte[] results = www.downloadHandler.data;
            }
        }
    }

}
