using UnityEngine;
using Sirenix.OdinInspector;

public class TimelineReceiver : MonoBehaviour {

    public string Signal = "";

    [LabelText("When Signal Received...")]
    public BetterEvent BEOnSignalReceived = new BetterEvent();

    private void Awake() {
        ExtendedTimelineSignal.SignalSent += TimelineSignal_Sent;
    }

    private void OnDisable() {
        ExtendedTimelineSignal.SignalSent -= TimelineSignal_Sent;
    }

    private void TimelineSignal_Sent(string signal) {
        if (signal == Signal)
            BEOnSignalReceived.Invoke();
    }
}