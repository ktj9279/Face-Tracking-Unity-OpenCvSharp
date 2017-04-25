// The purpose of this script is to remove background image and try to only capture the face.

using Emgu.CV.CvEnum;
using UnityEngine;
using System;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Collections.Generic;
using Emgu.CV.Util;

public class GrabCut : MonoBehaviour
{
    // Webcam.
    private VideoCapture webcamTexture;
    // image which is used in OpenCV manipulation.
    Mat img = new Mat();
    // The end result which is displayed on GUITexture.
    private Texture2D resultTexture;
    // Cascade used for detecting faces.
    private CascadeClassifier faceCascade = new CascadeClassifier("C:\\Users\\conner\\Documents\\GitHub\\Face-Tracking-Unity-OpenCvSharp\\EmguProject\\Assets\\Resources\\haarcascade_frontalface_default.xml");

    private int[,] hierarchy;
    // Use this for initialization
    void Start()
    {
        // If there is a available webcam.
        webcamTexture = new VideoCapture(0);
        webcamTexture.Start();
    }


    // Update is called once per frame
    void Update()
    {

        webcamTexture.Read(img);
       
        
        CvInvoke.CvtColor(img, img, ColorConversion.Bgr2Rgb);
        // Use statement - improves performance.
        using (UMat gray = new UMat())
        {
            // Convert image to gray scale.
            CvInvoke.CvtColor(img, gray, ColorConversion.Bgr2Gray);
            // Equalise the lighting.
            CvInvoke.EqualizeHist(gray, gray);
            // Rectanlges which highlight where the face is in the image.
            Rectangle[] faces = null;

            // Detect faces in image.
            faces = faceCascade.DetectMultiScale(gray, 1.15, 5);
            if (faces.Length > 0)
            {
                Rectangle face = faces[0];
                int numberOfIterations = 15;
                Image<Bgr, byte> src = img.ToImage<Bgr, byte>();
                Image<Gray, byte> mask = src.GrabCut(face, numberOfIterations);
                mask = mask.ThresholdBinary(new Gray(2), new Gray(255));
                UMat newImg = src.Copy(mask).ToUMat();  
                // Update the result texture.
                resultTexture = TextureConvert.InputArrayToTexture2D(newImg, FlipType.Vertical);
                GetComponent<GUITexture>().texture = resultTexture;
                Size s = img.Size;
                GetComponent<GUITexture>().pixelInset = new Rect(s.Width / 2, -s.Height / 2, s.Width, s.Height);
            }
        }
    }
}
