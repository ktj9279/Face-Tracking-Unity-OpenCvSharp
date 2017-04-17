
using Emgu.CV.CvEnum;
using UnityEngine;
using System;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Collections.Generic;
using Emgu.CV.Util;

public class Test : MonoBehaviour
{
    // Webcam.
    private WebCamTexture webcamTexture;
    // Webcam image that is copied to a texure2D - Is there a faster method?
    private Texture2D copyTexture;
    // image which is used in OpenCV manipulation.
    UMat img = new UMat();
    // The end result which is displayed on GUITexture.
    private Texture2D resultTexture;
    // List of available webcam devices.
    private WebCamDevice[] devices;
    // Cascade used for detecting faces.
    private CascadeClassifier faceCascade = new CascadeClassifier("C:\\Users\\conner\\Documents\\GitHub\\Face-Tracking-Unity-OpenCvSharp\\EmguProject\\Assets\\Resources\\haarcascade_frontalface_default.xml");

    private VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();

    private int[,] hierarchy;
    // Use this for initialization
    void Start()
    {
        // If there is a available webcam.
        devices = WebCamTexture.devices;
        if (devices.Length > 0)
        {
            // Set webcam to first one found.
            webcamTexture = new WebCamTexture(WebCamTexture.devices[0].name);
            // Need to initalize texture2D in order to use it.
            copyTexture = new Texture2D(webcamTexture.width, webcamTexture.height);
            //resultTexture = new Texture2D(webcamTexture.width, webcamTexture.height);
            // Start the webcam.
            webcamTexture.Play();
            GetComponent<GUITexture>().texture = resultTexture;
        }
    }


    // Update is called once per frame
    void Update()
    {
        // Update texture with webcam image.
        copyTexture = new Texture2D(webcamTexture.width, webcamTexture.height);
        copyTexture.SetPixels32(webcamTexture.GetPixels32());
        copyTexture.Apply();
        // Convert to Image to be used in image manipulation.
        TextureConvert.Texture2dToOutputArray(copyTexture, img);
        // This will appear upside down, so flip it.
        CvInvoke.Flip(img, img, FlipType.Vertical);

        // Use statement - improves performance.
        using (UMat gray = new UMat())
        {
            // Convert image to gray scale.
            CvInvoke.CvtColor(img, gray, ColorConversion.Bgr2Gray);
            // Equalise the lighting.
            CvInvoke.EqualizeHist(gray, gray);
            //CvInvoke.CvtColor(img, img, ColorConversion.Bgr2Gray);

            // Rectanlges which highlight where the face is in the image.
            Rectangle[] faces = null;

            // Detect faces in image.
            faces = faceCascade.DetectMultiScale(gray);
            foreach (Rectangle face in faces)
            {
                using (UMat faceGray = new UMat(img, face))
                {
                    // Draw a green rectangle around the found area.
                    CvInvoke.Rectangle(img, face, new MCvScalar(0, 255, 0));
                    // Convert ROI to gray-scale.                     
                    CvInvoke.CvtColor(faceGray, faceGray, ColorConversion.Bgr2Gray);
                    // Convert image to canny to detect edges.
                    CvInvoke.Canny(faceGray, faceGray, 30, 128, 3, false);
                    // Hierarchy order of contours. 
                    //hierarchy = CvInvoke.FindContourTree(faceGray, contours, ChainApproxMethod.ChainApproxSimple);
                    // Find the contours in the ROI area.
                    CvInvoke.FindContours(faceGray, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple, face.Location);
                    for (int i = 0; i < contours.Size; ++i)
                        CvInvoke.DrawContours(img, contours,i, new MCvScalar(255, 255, 255));
                }
            }
        }

        // Update the result texture.
        //Texture2D texture 
        resultTexture = TextureConvert.InputArrayToTexture2D(img, FlipType.Vertical);
        GetComponent<GUITexture>().texture = resultTexture;
        Size s = img.Size;
        GetComponent<GUITexture>().pixelInset = new Rect(s.Width / 2, -s.Height / 2, s.Width, s.Height);
    }
}
