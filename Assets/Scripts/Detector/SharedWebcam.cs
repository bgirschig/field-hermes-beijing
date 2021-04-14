/// Manage references to camera texture and unity camera management
// This wraps around unity's webcam texture and fixes a few issues with it:
// - Multiple components can't create a WebCamTexture from the same camera. This creates one that
//   other components can reference
// - The texture is not always ready immediately after the WebCamTexture is created. SharedWebcam
//   has a 'onCameraChange' event is invoked at the appropriate time

using UnityEngine;
using System;
using UnityEngine.Events;

public class SharedWebcam : MonoBehaviour
{
    public int defaultCameraIndex;

    [NonSerialized]
    public UnityEvent onCameraChange = new UnityEvent();

    [NonSerialized]
    public string currentCamera = null;
    [NonSerialized]
    public WebCamTexture capture;
    [NonSerialized]
    public float ratio;
    [NonSerialized]
    public int width;
    [NonSerialized]
    public int height;
    [NonSerialized]
    public bool ready = false;

    public bool didUpdateThisFrame {
        get {
            if (capture == null) return false;
            return capture.didUpdateThisFrame;
        }
    }
    
    private bool waitingCameraInit = false;

    // Start is called before the first frame update
    void Start()
    {
        if (currentCamera != null) setCamera(currentCamera);
        else setCamera(defaultCameraIndex);
    }

    void Update() {
        if (waitingCameraInit && capture.width > 100) {
            width = capture.width;
            height = capture.height;
            ratio = (float)width / height;
            onCameraChange.Invoke();
            
            waitingCameraInit = false;
            ready = true;
        }
    }

    void setCamera(string deviceName) {
        if (capture) capture.Stop();
        if (deviceName == currentCamera) return;
        if (deviceName == null) return;
        ready = false;
        capture = new WebCamTexture(deviceName);
        capture.Play();
        currentCamera = deviceName;
        print($"Starting camera: {currentCamera}");
        waitingCameraInit = true;
    }

    void setCamera(int deviceIndex) {
        int deviceCount = WebCamTexture.devices.Length;
        deviceIndex %= deviceCount;

        if (WebCamTexture.devices[deviceIndex].name == "OBS Virtual Camera") {
            deviceIndex = (deviceIndex + 1) % deviceCount;
        }

        string deviceName = WebCamTexture.devices[deviceIndex].name;
        setCamera(deviceName);
    }
}
