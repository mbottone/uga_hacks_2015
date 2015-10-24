using UnityEngine;
using System.Collections;
using WiimoteApi;
using UnityStandardAssets.Vehicles.Car;

public class WiiMote : MonoBehaviour {

    float lastAngle = 0.0f;
    float updateInterval = 0.1f;

    int smooth = 5;
    float[] diffStorage = new float[5];
    float runningAverage = 0.0f;
    int currentUpdate = 0;
    int diffLength = 0;

    int playerCount = 0;
    bool activated = false;

    Wiimote steering;
    Wiimote speedMote;
    bool resetData = false;
    float currentAngle = 0.0f;

    void InitWiimotes()
    {
        WiimoteManager.FindWiimotes(); // Poll native bluetooth drivers to find Wiimotes
        foreach (Wiimote remote in WiimoteManager.Wiimotes)
        {
            Debug.Log("Found Wiimote");
            playerCount++;
            if (playerCount == 1)
            {
                speedMote = remote;
                speedMote.SendPlayerLED(true, false, false, false);
                speedMote.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL);
            }
            else
            {
                steering = remote;
                steering.SendPlayerLED(false, true, false, false);
                steering.SendDataReportMode(InputDataType.REPORT_BUTTONS_EXT19);
                steering.RequestIdentifyWiiMotionPlus();
            }
            
        }
    }
    void FinishedWithWiimotes()
    {
        WiimoteManager.Cleanup(steering);
        WiimoteManager.Cleanup(speedMote);
    }

    // Use this for initialization
    void Start () {
        InitWiimotes();

        for (int i = 0; i < smooth; i++)
        {
            diffStorage[i] = 0.0f;
        }
    }

    void Awake ()
    {
        
    }
	
	// Update is called once per frame
	void Update () {

        int ret;
        do
        {
            ret = speedMote.ReadWiimoteData();
        } while (ret > 0);

        float[] data = speedMote.Accel.GetCalibratedAccelData();
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

        //Debug.Log(speed);

        lastAngle = angle;

        do
        {
            ret = steering.ReadWiimoteData();
        } while (ret > 0);

        if (!activated && steering.wmp_attached)
        {
            Debug.Log("Activating WiiMotion Plus...");
            steering.ActivateWiiMotionPlus();
            activated = true;
        }
        if (activated && steering.current_ext == ExtensionController.MOTIONPLUS)
        {
            MotionPlusData motionData = steering.MotionPlus; // data!

            float dPitch = motionData.YawSpeed - 6.0f;

            if (Mathf.Abs(dPitch) < 1.0f)
            {
                dPitch = 0.0f;
            }

            float dist = dPitch * Time.deltaTime * -0.5f;
            currentAngle -= dist;

            GetComponent<CarController>().Move(dist, speed, 0, 0);
            HandlebarRotate[] comps = GetComponentsInChildren<HandlebarRotate>();
            foreach (HandlebarRotate comp in comps)
            {
                comp.speed = dist;
            }

            // Use the data...
        }

        
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

    void OnApplicationQuit()
    {
        FinishedWithWiimotes();
    }
}
