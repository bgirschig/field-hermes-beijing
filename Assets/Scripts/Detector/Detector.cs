// Detects the horizontal position of a bright(er) object in a webcam input

using UnityEngine;

using System;
using System.IO;
using UnityEngine.Events;

public class Detector : MonoBehaviour
{
    [NonSerialized]
    public DetectorCore detectorCore = new DetectorCore();

    // Inspector settings
    public SharedWebcam webcam;
    public bool invert {
        get { return _inverted; }
        set {
            _inverted = value;
            initState();
        }
    }

    // output
    [NonSerialized]
    public float speed = 0;
    [NonSerialized]
    public float smoothedSpeed = 0;
    [NonSerialized]
    public UnityEvent onMaskChange = new UnityEvent();
    public float smoothedPosition;

    // Textures
    [NonSerialized]
    public Texture2D debugTexture; // In order to access the debug image from other components, it
                                 // needs to be in the 'Texture' format we'll do the conversion
                                 // after the detector is done with a frame and before starting the
                                 // next one
    Color32[] webcamPixels32;

    // Smoothing
    float lastDetectionTime = 0;
    //   position
    float lastRawPosition = 0;
    float positionSmoothSpeed = 0;
    float positionSmoothDuration = 0.01f;
    bool _inverted;

    // Start is called before the first frame update
    void Start()
    {
        webcam.onCameraChange.AddListener(OnCameraChange);
        if (webcam.ready) OnCameraChange();

        LoadMaskFromDisk();
    }

    void initState() {
        lastRawPosition = 0;
        lastDetectionTime = 0;
        smoothedSpeed = 0;
        smoothedPosition = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (!enabled) return;
        
        if (detectorCore.debugImg != null) {
            debugTexture = OpenCvSharp.Unity.MatToTexture(detectorCore.debugImg, debugTexture);
        }
        if (webcam.didUpdateThisFrame) {
            float deltaTime = Time.time - lastDetectionTime;
            lastDetectionTime = Time.time;
            float position = detectorCore.detect(getWebcamMat(webcam.capture));
            if (invert) position = 1 - position;

            if (deltaTime > 0) {
                float rawSpeed = (position - lastRawPosition) / deltaTime;
                speed = rawSpeed;
            }
            lastRawPosition = position;
        }

        smoothedPosition = Mathf.SmoothDamp(smoothedPosition, lastRawPosition, ref positionSmoothSpeed, positionSmoothDuration);
    }

    /** A faster version of unity opencv's TextureToMat (avoid memory allocation) */
    OpenCvSharp.Mat getWebcamMat(WebCamTexture texture, OpenCvSharp.Unity.TextureConversionParams parameters = null) {
        if (null == parameters)
            parameters = OpenCvSharp.Unity.TextureConversionParams.Default;

        if (webcamPixels32 == null || webcamPixels32.Length != texture.width*texture.height) {
            webcamPixels32 = texture.GetPixels32();
        } else {
            texture.GetPixels32(webcamPixels32);
        }
        return OpenCvSharp.Unity.PixelsToMat(
            webcamPixels32,
            texture.width,
            texture.height,
            parameters.FlipVertically,
            parameters.FlipHorizontally,
            parameters.RotationAngle);
    }

    void OnCameraChange() {
        detectorCore.SetSize(webcam.capture.width, webcam.capture.height);
        onMaskChange.Invoke();
    }

    public void UpdateMask() {
        detectorCore.UpdateMask();
        onMaskChange.Invoke();
    }

    public void SaveMask() {
        Texture2D texture = OpenCvSharp.Unity.MatToTexture(detectorCore.srcMask);
        var pngBytes = texture.EncodeToPNG();
        string destination = Path.Combine(Application.persistentDataPath, "mask.png");
        File.WriteAllBytes(destination, pngBytes);
    }

    void LoadMaskFromDisk() {
        string maskPath = Path.Combine(Application.persistentDataPath, "mask.png");
        if (File.Exists(maskPath)) {            
            var pngBytes = File.ReadAllBytes(maskPath);
            var loaderTexture = new Texture2D(2,2);
            loaderTexture.LoadImage(pngBytes);
            
            detectorCore.setMask(OpenCvSharp.Unity.TextureToMat(loaderTexture));
            onMaskChange.Invoke();
        } else {
            if (webcam.ready) {
                detectorCore.setMask(new OpenCvSharp.Mat(webcam.width, webcam.height, OpenCvSharp.MatType.CV_8UC3));
            } else {
                detectorCore.setMask(new OpenCvSharp.Mat(512, 512, OpenCvSharp.MatType.CV_8UC3));
            }
        }
    }
}