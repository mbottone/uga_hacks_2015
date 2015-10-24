using UnityEngine;
using System.Collections;
using WiimoteApi;

public class WiiMote : MonoBehaviour {

    float lastAngle = 0.0f;
    float updateInterval = 0.1f;

    int smooth = 5;
    float[] diffStorage = new float[5];
    float runningAverage = 0.0f;
    int currentUpdate = 0;
    int diffLength = 0;

    void InitWiimotes()
    {
        WiimoteManager.FindWiimotes(); // Poll native bluetooth drivers to find Wiimotes
        foreach (Wiimote remote in WiimoteManager.Wiimotes)
        {
            Debug.Log("Found Wiimote");
            remote.SendPlayerLED(true, false, false, false);
            remote.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL);
        }
    }
    void FinishedWithWiimotes()
    {
        foreach (Wiimote remote in WiimoteManager.Wiimotes)
        {
            WiimoteManager.Cleanup(remote);
        }
    }

    // Use this for initialization
    void Start () {
        InitWiimotes();

        for (int i = 0;i < smooth; i ++)
        {
            diffStorage[i] = 0.0f;
        }
	}
	
	// Update is called once per frame
	void Update () {
        Wiimote remote = WiimoteManager.Wiimotes[0];
        int ret;
        do
        {
            ret = remote.ReadWiimoteData();
        } while (ret > 0);

        float[] data = remote.Accel.GetCalibratedAccelData();
        float x = data[0];
        float y = data[1];
        float z = data[2];

        x -= 0.5f;
        y -= 0.5f;

        float angle = (Mathf.Atan2(y, x) * 57.2958f) + 180;
        float diff = angle - lastAngle;
        float smoothDiff = GetSmoothDiff(diff);
        if (smoothDiff < 0.5f)
        {
            smoothDiff = 0.0f;
        }

        float speed = smoothDiff / Time.smoothDeltaTime;

        Debug.Log(speed);

        lastAngle = angle;
	}

    float GetSmoothDiff(float diff)
    {
        runningAverage -= diffStorage[currentUpdate];
        diffStorage[currentUpdate] = diff;
        runningAverage += diff;

        if (diffLength != smooth)
        {
            diffLength++;
        }

        currentUpdate++;
        if (currentUpdate == smooth)
        {
            currentUpdate = 0;
        }

        return runningAverage / diffLength;
    }

    void OnApplicationClose ()
    {
        FinishedWithWiimotes();
    }
}
