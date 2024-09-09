using UnityEngine;
using Firebase.Analytics; 

public class AttributionReceiver : MonoBehaviour
{
    public void ReceiveAttributionToken(string token)
    {
        Debug.Log("Received Attribution Token: " + token);

        FirebaseAnalytics.LogEvent("apple_search_ads_attribution", new Parameter("attribution_token", token));

    }

}

































        // SendTokenToServer(token);
//    }

    //// Example method to send the token to your server
    //private void SendTokenToServer(string token)
    //{
    //    // Implement the logic to send the token to your server
    //    // You might use UnityWebRequest or another networking library
    //    Debug.Log("Sending token to server: " + token);

    //    // Example code (not functional, just a placeholder)
    //    /*
    //    UnityWebRequest request = new UnityWebRequest("https://your-server-endpoint.com/attribution", "POST");
    //    byte[] bodyRaw = Encoding.UTF8.GetBytes(token);
    //    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
    //    request.downloadHandler = new DownloadHandlerBuffer();
    //    request.SetRequestHeader("Content-Type", "application/json");
    //    yield return request.SendWebRequest();

    //    if (request.result != UnityWebRequest.Result.Success)
    //    {
    //        Debug.Log("Error: " + request.error);
    //    }
    //    else
    //    {
    //        Debug.Log("Success: " + request.downloadHandler.text);
    //    }
    //    */
    //}