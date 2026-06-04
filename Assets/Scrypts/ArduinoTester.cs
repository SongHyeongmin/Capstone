using UnityEngine;

public class ArduinoTester : MonoBehaviour
{
    [Header("Arduino Receiver")]
    public ArduinoReceiver arduinoReceiver;

    [Header("Test Results")]
    public bool isReceivingData = false;
    public float dataReceiveRate = 0f;

    private int dataCount = 0;
    private float timer = 0f;
    private float lastAX = 0f;

    void Update()
    {
        if (arduinoReceiver == null)
        {
            Debug.LogError("ArduinoReceiver not assigned!");
            return;
        }

        // Check connection
        if (!arduinoReceiver.isConnected)
        {
            Debug.LogWarning("Arduino not connected!");
            return;
        }

        // Check if data is changing
        if (arduinoReceiver.AX != lastAX)
        {
            isReceivingData = true;
            dataCount++;
            lastAX = arduinoReceiver.AX;
        }

        // Calculate data receive rate per second
        timer += Time.deltaTime;
        if (timer >= 1f)
        {
            dataReceiveRate = dataCount;
            dataCount = 0;
            timer = 0f;

            /* Print test results every second
            Debug.Log($"=== Arduino Test ===");
            Debug.Log($"Connected: {arduinoReceiver.isConnected}");
            Debug.Log($"Data Rate: {dataReceiveRate}/sec");
            Debug.Log($"AX:{arduinoReceiver.AX} AY:{arduinoReceiver.AY} AZ:{arduinoReceiver.AZ}");
            Debug.Log($"GX:{arduinoReceiver.GX} GY:{arduinoReceiver.GY} GZ:{arduinoReceiver.GZ}");
            Debug.Log($"Pitch:{arduinoReceiver.Pitch} Roll:{arduinoReceiver.Roll}");
            Debug.Log($"HALL:{arduinoReceiver.HALL}");*/
        }
    }

    // Draw GUI on screen for easy testing
    void OnGUI()
    {
        if (arduinoReceiver == null) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(10, 10, 400, 30), $"Connected: {arduinoReceiver.isConnected}", style);
        GUI.Label(new Rect(10, 40, 400, 30), $"Data Rate: {dataReceiveRate}/sec", style);
        GUI.Label(new Rect(10, 70, 400, 30), $"AX: {arduinoReceiver.AX:F0} AY: {arduinoReceiver.AY:F0}", style);
        GUI.Label(new Rect(10, 100, 400, 30), $"GX: {arduinoReceiver.GX:F0} GY: {arduinoReceiver.GY:F0}", style);
        GUI.Label(new Rect(10, 130, 400, 30), $"Pitch: {arduinoReceiver.Pitch:F3} Roll: {arduinoReceiver.Roll:F3}", style);
        GUI.Label(new Rect(10, 160, 400, 30), $"HALL: {arduinoReceiver.HALL1}", style);

        // Connection status color
        GUIStyle statusStyle = new GUIStyle();
        statusStyle.fontSize = 24;
        statusStyle.normal.textColor = arduinoReceiver.isConnected ? Color.green : Color.red;
        GUI.Label(new Rect(10, 190, 400, 30),
            arduinoReceiver.isConnected ? "�� CONNECTED" : "�� DISCONNECTED", statusStyle);
    }
}