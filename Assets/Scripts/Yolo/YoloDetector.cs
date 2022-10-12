using UnityEngine;
using Unity.Barracuda;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.UI;
using System;

public class YoloDetector : MonoBehaviour
{
    const int IMAGE_SIZE = 640;
    const string INPUT_NAME = "images";
    const string OUTPUT_NAME = "output0";

    [Header("Model Stuff")] public NNModel modelFile;

    [Header("Scene Stuff")] public CameraView CameraView;
    public Preprocess preprocess;
    public YoloOutputParser yoloOutputParser = new YoloOutputParser();
    private YoloV5Prediction yoloV5Prediction = new YoloV5Prediction();
    string[] labels;
    IWorker worker;

    static readonly string[] classesNames = new string[] { "DISTO" };


    void Start()
    {
        var model = ModelLoader.Load(modelFile);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model);
    }

    void Update()
    {
        WebCamTexture webCamTexture = CameraView.GetCamImage();

        if (webCamTexture.didUpdateThisFrame && webCamTexture.width > 100)
        {
            preprocess.ScaleAndCropImage(webCamTexture, IMAGE_SIZE, RunModel);
        }
    }

    void RunModel(byte[] pixels)
    {
        StartCoroutine(RunModelRoutine(pixels));
    }

    IEnumerator RunModelRoutine(byte[] pixels)
    {
        Tensor tensor = TransformInput(pixels);

        var inputs = new Dictionary<string, Tensor>
        {
            { INPUT_NAME, tensor }
        };

        worker.Execute(inputs);
        var outputTensor = worker.PeekOutput("output0");
        var layer2 = worker.PeekOutput("onnx::Sigmoid_449");
        var layer3 = worker.PeekOutput("onnx::Sigmoid_487");
        var layer4 = worker.PeekOutput("onnx::Sigmoid_525");
        var boxes = yoloOutputParser.ParseOutputs(outputTensor.ToReadOnlyArray());


        var results = yoloV5Prediction.GetResults(layer2.ToReadOnlyArray(), layer3.ToReadOnlyArray(),
            layer4.ToReadOnlyArray(), classesNames, 0.3f, 0.7f);
        //get largest output
        List<float> temp = outputTensor.ToReadOnlyArray().ToList();
        float max = temp.Max();
        int index = temp.IndexOf(max);

        //set UI text

        //dispose tensors
        tensor.Dispose();
        outputTensor.Dispose();
        yield return null;
    }

    //transform from 0-255 to -1 to 1
    Tensor TransformInput(byte[] pixels)
    {
        float[] transformedPixels = new float[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            transformedPixels[i] = (pixels[i] - 127f) / 128f;
        }

        return new Tensor(1, IMAGE_SIZE, IMAGE_SIZE, 3, transformedPixels);
    }
}