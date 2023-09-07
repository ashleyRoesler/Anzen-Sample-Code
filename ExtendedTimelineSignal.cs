using UnityEngine;

public class ExtendedTimelineSignal : MonoBehaviour {

    public static event System.Action<string> SignalSent;

    public void SendSignal(string signal) {
        SignalSent?.Invoke(signal);
    }
}