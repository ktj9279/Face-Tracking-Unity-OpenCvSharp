
using Emgu.CV.CvEnum;
using UnityEngine;
using System;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Collections.Generic;
using Emgu.CV.Util;
using System.Runtime.InteropServices;

namespace FaceRecognition
{
    public class UsingComputeShader : MonoBehaviour
    {
        // Webcam.
        private VideoCapture webcamTexture;
        // image which is used in OpenCV manipulation.
        Mat img = new Mat();
        // List of available webcam devices.
        private WebCamDevice[] devices;
        public ComputeShader shader;

        RenderTexture tex;

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
            int kernalIndex = shader.FindKernel("FillWithRed");
            tex = new RenderTexture(img.Width, img.Height,24);
            tex.enableRandomWrite = true;
            tex.Create();



           // shader.SetVector(kernalIndex, new Vector4());
            shader.SetTexture(kernalIndex, "res",tex);
            shader.Dispatch(kernalIndex, tex.width / 8, tex.height / 8, 1);
            GetComponent<GUITexture>().texture = tex;
            Size s = img.Size;
            GetComponent<GUITexture>().pixelInset = new Rect(s.Width / 2, -s.Height / 2, s.Width, s.Height);
        }
        void OnDestroy()
        {
            webcamTexture.Stop();
        }
    }
}