// Display the output from the first webcam on a rawImage

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp;
using System.Threading;

[RequireComponent(typeof(RawImage))]
public class Webcam : MonoBehaviour
{
    WebCamTexture camTexture;
    RawImage image;

    void Start() {
        image = GetComponent<RawImage>();
        camTexture = new WebCamTexture(WebCamTexture.devices[0].name);
        camTexture.Play();

        image.texture = camTexture;
        image.rectTransform.sizeDelta = new Vector2(camTexture.width, camTexture.height);
    }
}