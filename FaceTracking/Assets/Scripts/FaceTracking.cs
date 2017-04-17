
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OpenCvSharp;

// Parallel computation support
using Uk.Org.Adcock.Parallel;
using System;
using System.Runtime.InteropServices;



public class FaceTracking : MonoBehaviour
{

    // Quad that displays original camera. // Only need for debug.
    public MeshRenderer WebCamTextureRenderer;
    // Quad that shows modification to captured image.
    public MeshRenderer ProcessedTextureRenderer;
    // What camera to use.
    public int deviceNumber;
    private WebCamTexture webcamTexture;
    // This is displayed on the processedRenderer.
    private Texture2D processedTexture;
    // Webcam caputre device.
    VideoCapture capture;
    // Current frame.
    Mat frame;
    // Current frame texture.
    Texture2D snapFrame;
    Point[][] contours;
    HierarchyIndex[] hierarchy;
    // Video size
    private const int imWidth = 1280;
    private const int imHeight = 720;
    private int imFrameRate = 180;

    // Mat used to hold grayScaled image.
    Mat gray = new Mat();
    Mat cannyOutput = new Mat();

    CascadeClassifier haarCascade = new CascadeClassifier("..\\FaceTracking\\Assets\\HaarCascades\\haarcascades\\haarcascade_frontalface_default.xml");
    CascadeClassifier eyeCascade = new CascadeClassifier("..\\FaceTracking\\Assets\\HaarCascades\\haarcascades\\haarcascade_eye.xml");

    void Start()
    {
        snapFrame  = new Texture2D(imWidth, imHeight);
        capture = new VideoCapture();
        frame = new Mat();
        capture.Open(deviceNumber);
        capture.Read(frame);
        //frame.SaveImage("image.jpg");
        // snapFrame = (Texture2D) Resources.Load("image.jpg");

        //  WebCamTextureRenderer.material.mainTexture = snapFrame;
    }

    // Rotates and returns a mat that has been rotated by a certain angle.
    Mat RotateFrame(Mat src, double angle)
    {
        Mat dst = new Mat();
        Point2f pt = new Point2f(src.Cols / 2, src.Rows / 2);
        Mat r = Cv2.GetRotationMatrix2D(pt, angle, 1.0);
        Cv2.WarpAffine(src, dst, r, new Size(src.Cols, src.Rows));
        return dst;
    }


    void Update()
    {
        // If the webcam has been updated, update the frame.
        if (capture.Read(frame))
        {
            // Convert the frame into grayscale.
            Cv2.CvtColor(frame, gray, ColorConversionCodes.RGB2GRAY);
          
            // Equalise lighting to make landmark detection more accurate. - detecting points.
            Cv2.EqualizeHist(gray, gray);
        }


        //// As faces is easiest to detect, check for these first.
        OpenCvSharp.Rect[] faces = new OpenCvSharp.Rect[0];
        faces = haarCascade.DetectMultiScale(
                             gray, 1.15, 5, HaarDetectionType.DoRoughSearch | HaarDetectionType.ScaleImage, new Size(60, 60));

        //// Detect if any eyes are in the image.
        //OpenCvSharp.Rect[] eyes = new OpenCvSharp.Rect[2];
        //eyes = eyeCascade.DetectMultiScale(
        //           gray, 1.15, 6, HaarDetectionType.DoRoughSearch | HaarDetectionType.ScaleImage, new Size(40, 40));


        //// If eyes are equal = 2 then there must be a face. - assumption is that the two eyes are from the same person. 
        //if (eyes.Length == 2 && faces.Length == 0)
        //{
        //    Point averagePoint = new Point();
        //    averagePoint.X = (eyes[0].X + eyes[1].X) / 2;
        //    averagePoint.Y = (eyes[0].Y + eyes[1].Y) / 2;
        //    faces = new OpenCvSharp.Rect[1];
        //    faces[0] = new OpenCvSharp.Rect(averagePoint, new Size(150, 150));
        //}
        //// If there is an eye, there must be a face. Not as accurate as there is no other landmark to triangulate point.
        //else if (eyes.Length == 1 && faces.Length == 0)
        //{
        //    Point averagePoint = new Point();
        //    averagePoint.X = eyes[0].X;
        //    averagePoint.Y = eyes[0].Y - 2;
        //    faces = new OpenCvSharp.Rect[1];
        //    faces[0] = new OpenCvSharp.Rect(averagePoint, new Size(80, 80));
        //}


        //// due to cascade method not being rotation invarient, rotate frame and check if face is there.
        //Mat clockRot = new Mat();
        //// If no faces were detected, rotate image.
        //if (faces.Length == 0)
        //{
        //    clockRot = RotateFrame(gray, 40);
        //    faces = haarCascade.DetectMultiScale(
        //                    clockRot, 1.15, 5, HaarDetectionType.DoRoughSearch | HaarDetectionType.ScaleImage, new Size(60, 60));
        //}
        //if (faces.Length == 0)
        //{
        //    clockRot = RotateFrame(gray, -40);
        //    faces = haarCascade.DetectMultiScale(
        //                clockRot, 1.15, 5, HaarDetectionType.DoRoughSearch | HaarDetectionType.ScaleImage, new Size(60, 60));

        //}

        //// For each face detected, add square around feature, and check for eyes.
        foreach (var faceRect in faces)
        {
            // Obain area face is contained.
            var faceMat = new Mat(frame, faceRect);
            // Set color of rectangle for face.
            var color = Scalar.FromRgb(0, 0, 255);
            // Draw rectangle around face. - remove once edge is found.
            Cv2.Rectangle(frame, faceRect, color, 3);
        }

        //foreach (var eyeRect in eyes)
        //{
        //    var eyeMat = new Mat(gray, eyeRect);
        //    var color = Scalar.FromRgb(0, 255, 0);
        //    Cv2.Rectangle(frame, eyeRect, color, 3);
        //}

        // This segment is to draw contours around an image and display it on the OpenCv window.

        // Blur the image.
        Cv2.Blur(gray, gray, new Size(3, 3));
        // Detect edges using canny - Lower number = more contours.
        Cv2.Canny(gray, cannyOutput, 100, 255);
       // Cv2.Threshold(gray, cannyOutput,100,255,ThresholdTypes.BinaryInv);
        // Find contours
        
        Cv2.FindContours(cannyOutput, out contours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxTC89L1, new Point(0, 0));
        // Draw contours
       
        //Parallel.For(0, contours.Length, i =>
        //{
            // Cv2.DrawContours(frame, contours, i, Scalar.White, 2, LineTypes.Filled, hierarchy);
          
        //});
        if(contours != null)
            for (int i = 0; i < contours.Length;i++)
            {
                Cv2.DrawContours(frame, contours, i, Scalar.White, 2, LineTypes.Link8, hierarchy);
                if (Input.GetKeyDown("space"))
                    Application.Quit();
            }
            //Debug.Log("Drawn contour.");
        // Update video.
        Cv2.ImShow("Contours", frame);



        //    frame.SaveImage("image",);
        //     Cv2.ImWrite("image.jpg", frame);

        //    snapFrame =  Resources.Load("image.jpg") as Texture2D;

        //Debug.Log(snapFrame.dimension);
    }



    // Trying to convert OpencV texture to a Unity texture2D. 
    void MatToTexture(Mat m, Texture2D tex)
    {
 
        //// create Color32 array that can be assigned to Texture2D directly
        //Color32[] c = new Color32[imHeight * imWidth];

       
        //// parallel for loop
        //Parallel.For(0, imHeight, i =>
        //{
        //    for (var j = 0; j < imWidth; j++)
        //    {
        //        byte vec = m.GetArray[j + i * imWidth];
        //        var color32 = new Color32
        //        {
        //            r = vec,
        //            g = vec,
        //            b = vec,
        //            a = 0
        //        };
        //        c[j + i * imWidth] = color32;
        //    }
        //});

        //processedTexture.SetPixels32(c);
        //// to update the texture, OpenGL manner
        //processedTexture.Apply();
    }

    void OnDestroy()
    {
        Cv2.DestroyAllWindows();
 
    }
}