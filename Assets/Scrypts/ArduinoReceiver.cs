using UnityEngine;
using System.IO.Ports;
using System.Threading;

public class ArduinoReceiver : MonoBehaviour
{
    [Header("Serial Settings")]
    public string portName = "COM4";
    public int baudRate = 115200;

    [Header("Sensor Values")]
    public float AX, AY, AZ;
    public float GX, GY, GZ;
    public int HALL1, HALL2, HALL3; // 홀센서 3개

    [Header("Status")]
    public bool isConnected = false;
    public bool isReady = false;

    private SerialPort serial;
    private Thread readThread;
    private bool isRunning = false;

    // Thread-safe buffer
    private string latestData = "";
    private readonly object dataLock = new object();

    // Complementary filter variables
    private float pitch = 0f;
    private float roll = 0f;
    private float lastTime = 0f;

    public float Pitch => pitch;
    public float Roll => roll;

    void Start()
    {
        try
        {
            serial = new SerialPort(portName, baudRate);
            serial.ReadTimeout = 100;
            serial.Open();
            isConnected = true;
            isReady = true;
            Debug.Log("Arduino connected!");

            // Start read thread
            isRunning = true;
            readThread = new Thread(ReadSerialData);
            readThread.IsBackground = true;
            readThread.Start();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Arduino connection failed: " + e.Message);
            isConnected = false;
        }

        lastTime = Time.time;
    }

    // Read data in separate thread
    void ReadSerialData()
    {
        while (isRunning && serial != null && serial.IsOpen)
        {
            try
            {
                string data = serial.ReadLine();
                lock (dataLock)
                {
                    latestData = data;
                }
            }
            catch (System.TimeoutException) { }
            catch (System.Exception e)
            {
                Debug.LogError("Read thread error: " + e.Message);
            }
        }
    }

    void Update()
    {
        if (!isConnected) return;

        string data = "";
        lock (dataLock)
        {
            data = latestData;
            latestData = "";
        }

        if (!string.IsNullOrEmpty(data))
        {
            if (data.StartsWith("ERROR:"))
            {
                Debug.LogWarning("Arduino error: " + data);
                return;
            }

            if (data.StartsWith("OK:"))
            {
                Debug.Log("Arduino: " + data);
                return;
            }

            ParseData(data);
            CalculateAngles();
        }
    }

    void ParseData(string data)
    {
        try
        {
            if (!data.Contains("AX:")) return;

            string[] parts = data.Split(',');
            foreach (string part in parts)
            {
                string[] kv = part.Split(':');
                if (kv.Length != 2) continue;

                string key = kv[0].Trim();
                string value = kv[1].Trim();

                float floatResult;
                int intResult;

                switch (key)
                {
                    case "AX":
                        if (float.TryParse(value, out floatResult)) AX = floatResult;
                        break;
                    case "AY":
                        if (float.TryParse(value, out floatResult)) AY = floatResult;
                        break;
                    case "AZ":
                        if (float.TryParse(value, out floatResult)) AZ = floatResult;
                        break;
                    case "GX":
                        if (float.TryParse(value, out floatResult)) GX = floatResult;
                        break;
                    case "GY":
                        if (float.TryParse(value, out floatResult)) GY = floatResult;
                        break;
                    case "GZ":
                        if (float.TryParse(value, out floatResult)) GZ = floatResult;
                        break;
                    case "HALL1":
                        if (int.TryParse(value, out intResult)) HALL1 = intResult;
                        break;
                    case "HALL2":
                        if (int.TryParse(value, out intResult)) HALL2 = intResult;
                        break;
                    case "HALL3":
                        if (int.TryParse(value, out intResult)) HALL3 = intResult;
                        break;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Parse error: " + e.Message);
        }
    }

    void CalculateAngles()
    {
        try
        {
            float dt = Time.time - lastTime;
            lastTime = Time.time;

            float accelPitch = AX / 16384f;
            float accelRoll = AY / 16384f;

            float gyroPitch = GX / 131f;
            float gyroRoll = GY / 131f;

            pitch = 0.98f * (pitch + gyroPitch * dt) + 0.02f * accelPitch;
            roll = 0.98f * (roll + gyroRoll * dt) + 0.02f * accelRoll;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Angle calculation error: " + e.Message);
        }
    }

    void OnDestroy()
    {
        try
        {
            isRunning = false;
            if (readThread != null && readThread.IsAlive)
                readThread.Join(1000);

            if (serial != null && serial.IsOpen)
            {
                serial.Close();
                Debug.Log("Arduino disconnected");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Serial close error: " + e.Message);
        }
    }
}