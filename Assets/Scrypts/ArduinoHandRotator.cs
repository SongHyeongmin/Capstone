using UnityEngine;

public class ArduinoHandRotator : MonoBehaviour
{
    [Header("Arduino Receiver")]
    public ArduinoReceiver arduinoReceiver;

    [Header("Pivot Settings")]
    public Transform wristPivot; // R_Wrist ¿¬°á

    [Header("Rotation Settings")]
    public float rotationSensitivity = 0.005f;
    public float smoothSpeed = 5f;
    public float maxAngle = 90f;

    private float offsetAX = 0f;
    private bool isCalibrated = false;
    private int calibSampleCount = 0;
    private float calibSumAX = 0f;
    private const int CALIB_SAMPLES = 50;

    private float currentAngle = 0f;
    private float baseX;
    private float baseY;
    private float baseZ;

    void Start()
    {
        if (wristPivot != null)
        {
            baseX = wristPivot.localEulerAngles.x;
            baseY = wristPivot.localEulerAngles.y;
            baseZ = wristPivot.localEulerAngles.z;
        }
    }

    void Update()
    {
        if (arduinoReceiver == null || !arduinoReceiver.isConnected) return;
        if (wristPivot == null)
        {
            Debug.LogError("Wrist Pivot not assigned!");
            return;
        }

        // Calibration
        if (!isCalibrated)
        {
            calibSumAX += arduinoReceiver.AX;
            calibSampleCount++;

            if (calibSampleCount >= CALIB_SAMPLES)
            {
                offsetAX = calibSumAX / CALIB_SAMPLES;
                isCalibrated = true;
                Debug.Log($"Calibration complete! offsetAX:{offsetAX}");
            }
            return;
        }

        // Calculate rotation from AX
        float correctedAX = arduinoReceiver.AX - offsetAX;
        float targetAngle = correctedAX * rotationSensitivity;
        targetAngle = Mathf.Clamp(targetAngle, -maxAngle, maxAngle);

        // Smooth angle
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * smoothSpeed);

        // Rotate around Z axis (blue axis)
        wristPivot.localEulerAngles = new Vector3(
            baseX,
            baseY,
            baseZ + currentAngle
        );

        Debug.Log($"correctedAX:{correctedAX} angle:{currentAngle}");
    }
}