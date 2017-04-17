using OpenCvSharp;
using System.Collections;
using System.Collections.Generic;
using Uk.Org.Adcock.Parallel;
using UnityEngine;

public class Contours : MonoBehaviour
{
    private Texture2D currentFrame;
    //private WebCamTexture webcam;
    private VideoCapture webcam = new VideoCapture();
    private Mat frameMat;
    private Mat resultMat;

    private Vec3b[] videoSourceImageData = new Vec3b[0];
    private byte[] cannyImageData;
    
    
    // Use this for initialization
    void Start ()
    {
        //WebCamDevice[] devices = WebCamTexture.devices;
        // Set up webcam.
        //if (devices.Length > 0)
        //{
            Debug.Log("Started application");
            webcam = new VideoCapture(0);
            cannyImageData = new byte[webcam.FrameHeight * webcam.FrameWidth];
        currentFrame = new Texture2D(webcam.FrameWidth, webcam.FrameHeight);
        frameMat = new Mat(webcam.FrameWidth, webcam.FrameHeight, MatType.CV_8UC1);
        //    webcam = new WebCamTexture(WebCamTexture.devices[0].name);
        //    webcam.Play();

        //}
    }
	
	// Update is called once per frame
	void Update ()
    {
        
        webcam.Read(frameMat);

        

        // Convert webcam texture to mat object.
        //WebCamToMat(frameMat);
        using (Mat gray = new Mat())
        {
            Cv2.CvtColor(frameMat, gray, ColorConversionCodes.RGB2GRAY);
            // Blur the image.
            Cv2.Blur(gray, gray, new Size(3, 3));
            // Detect edges using canny - Lower number = more contours.
            Cv2.Canny(gray, gray, 85, 255);
            // Cv2.Threshold(gray, cannyOutput,100,255,ThresholdTypes.BinaryInv);
            Cv2.ImShow("Contours", gray);
            currentFrame = MatToTexture(frameMat);
            GetComponent<Renderer>().material.mainTexture = currentFrame;
        }
    }



    // Trying to convert OpencV texture to a Unity texture2D. 
    void WebCamToMat(Mat m)
    {
        //Color32[] c = webcam.GetPixels32();
        //// Parallel for loop
        //// convert Color32 object to Vec3b object
        //// Vec3b is the representation of pixel for Mat
        //Parallel.For(0, webcam.height, i => {
        //    for (var j = 0; j < webcam.width; j++)
        //    {
        //        var col = c[j + i * webcam.width];
        //        var vec3 = new Vec3b
        //        {
        //            Item0 = col.b,
        //            Item1 = col.g,
        //            Item2 = col.r
        //        };
        //        // set pixel to an array
        //        videoSourceImageData[j + i * webcam.width] = vec3;
        //    }
        //});
        //// assign the Vec3b array to Mat
        //m.SetArray(0, 0, videoSourceImageData);
    }

    Texture2D MatToTexture(Mat m)
    {
        Texture2D tex = new Texture2D(webcam.FrameWidth, webcam.FrameHeight);
        // cannyImageData is byte array, because canny image is grayscale
        cannyImageData = m.ToBytes(".png");
        //m.GetArray(0, 0, cannyImageData);
   
        // create Color32 array that can be assigned to Texture2D directly
        Color32[] c = new Color32[webcam.FrameHeight * webcam.FrameWidth];

        // parallel for loop
        Parallel.For(0, webcam.FrameHeight, i =>
        {
            for (var j = 0; j < webcam.FrameWidth; j++)
            {
                byte vec = cannyImageData[j + i * webcam.FrameWidth];
                var color32 = new Color32
                {
                    r = vec,
                    g = vec,
                    b = vec,
                    a = 0
                };
                c[j + i * webcam.FrameWidth] = color32;
            }
        });

        tex.SetPixels32(c);
        // to update the texture, OpenGL manner
        tex.Apply();

        return tex;
    }


    void OnDestroy()
    {
        webcam.Dispose();

    }
}
