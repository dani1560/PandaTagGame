using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Analytics;

public class ConversionTracker : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventAppOpen);
    }

}
