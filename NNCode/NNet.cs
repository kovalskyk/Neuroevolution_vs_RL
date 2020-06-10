using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MathNet.Numerics.LinearAlgebra;
using System;

using Newtonsoft.Json;
using Random = UnityEngine.Random;

public class NNet : MonoBehaviour
{
    public Matrix<float> inputLayer = Matrix<float>.Build.Dense(1, 5);

    public List<Matrix<float>> hiddenLayers = new List<Matrix<float>>();

    public Matrix<float> outputLayer = Matrix<float>.Build.Dense(1, 2);

    public List<Matrix<float>> weights = new List<Matrix<float>>();

    public List<float> biases = new List<float>();

    public float fitness;

    public void Initialise (int hiddenLayerCount, int hiddenNeuronCount)
    {

        inputLayer.Clear();
        hiddenLayers.Clear();
        outputLayer.Clear();
        weights.Clear();
        biases.Clear();

        for (int i = 0; i < hiddenLayerCount + 1; i++)
        {

            Matrix<float> f = Matrix<float>.Build.Dense(1, hiddenNeuronCount);

            hiddenLayers.Add(f);

            biases.Add(Random.Range(-1f, 1f));

            //WEIGHTS
            if (i == 0)
            {
                Matrix<float> inputToH1 = Matrix<float>.Build.Dense(5, hiddenNeuronCount);
                weights.Add(inputToH1);
            }

            Matrix<float> HiddenToHidden = Matrix<float>.Build.Dense(hiddenNeuronCount, hiddenNeuronCount);
            weights.Add(HiddenToHidden);

        }

        Matrix<float> OutputWeight = Matrix<float>.Build.Dense(hiddenNeuronCount, 2);
        weights.Add(OutputWeight);
        biases.Add(Random.Range(-1f, 1f));

        RandomiseWeights();

    }

    public NNet InitialiseCopy (int hiddenLayerCount, int hiddenNeuronCount)
    {
        NNet n = gameObject.AddComponent<NNet>();

        List<Matrix<float>> newWeights = new List<Matrix<float>>();

        for (int i = 0; i < this.weights.Count; i++)
        {
            Matrix<float> currentWeight = Matrix<float>.Build.Dense(weights[i].RowCount, weights[i].ColumnCount);

            for (int x = 0; x < currentWeight.RowCount; x++)
            {
                for (int y = 0; y < currentWeight.ColumnCount; y++)
                {
                    currentWeight[x, y] = weights[i][x, y];
                }
            }

            newWeights.Add(currentWeight);
        }

        List<float> newBiases = new List<float>();

        newBiases.AddRange(biases);

        n.weights = newWeights;
        n.biases = newBiases;

        n.InitialiseHidden(hiddenLayerCount, hiddenNeuronCount);

        return n;
    }

    public void InitialiseHidden (int hiddenLayerCount, int hiddenNeuronCount)
    {
        inputLayer.Clear();
        hiddenLayers.Clear();
        outputLayer.Clear();

        for (int i = 0; i < hiddenLayerCount + 1; i ++)
        {
            Matrix<float> newHiddenLayer = Matrix<float>.Build.Dense(1, hiddenNeuronCount);
            hiddenLayers.Add(newHiddenLayer);
        }

    }

    public void RandomiseWeights()
    {

        for (int i = 0; i < weights.Count; i++)
        {

            for (int x = 0; x < weights[i].RowCount; x++)
            {

                for (int y = 0; y < weights[i].ColumnCount; y++)
                {

                    weights[i][x, y] = Random.Range(-1f, 1f);

                }

            }

        }

    }

    public (float, float) RunNetwork (float a, float b, float c, float d, float e)
    {
        inputLayer[0, 0] = a;
        inputLayer[0, 1] = b;
        inputLayer[0, 2] = c;
        inputLayer[0, 3] = d;
        inputLayer[0, 4] = e;

        inputLayer = inputLayer.PointwiseTanh();

        hiddenLayers[0] = ((inputLayer * weights[0]) + biases[0]).PointwiseTanh();

        for (int i = 1; i < hiddenLayers.Count; i++)
        {
            hiddenLayers[i] = ((hiddenLayers[i - 1] * weights[i]) + biases[i]).PointwiseTanh();
        }

        outputLayer = ((hiddenLayers[hiddenLayers.Count-1]*weights[weights.Count-1])+biases[biases.Count-1]).PointwiseTanh();

        //First output is acceleration and second output is steering
        return (Sigmoid(outputLayer[0,0]), (float)Math.Tanh(outputLayer[0,1]));
        //return (Sigmoid(outputLayer[0, 0]), Sigmoid(outputLayer[0, 1]));
    }

    private float Sigmoid (float s)
    {
        return (1 / (1 + Mathf.Exp(-s)));
    }
    
    /*
    public static NNet CreateFromSave(string json) // takes a saved json string and returns new NNet loaded from json
    {
        SavedNetwork save = JsonConvert.DeserializeObject<SavedNetwork>(json);

        NNet nn = new NNet(save.inputCount, save.outputCount, save.hiddenLayerCount, save.hiddenNeuronCount);
        nn.weights = WeightArrayToMatrix(save.weights);
        nn.biases = save.biases;

        return nn;
    }

    public void Load(string json) // loads a json string into the current NNet
    {
        SavedNetwork save = JsonConvert.DeserializeObject<SavedNetwork>(json);

        if (save.inputCount == inputCount && save.outputCount == outputCount && save.hiddenLayerCount == hiddenLayerCount && save.hiddenNeuronCount == hiddenNeuronCount)
        {
            this.weights = WeightArrayToMatrix(save.weights);
            this.biases = save.biases;
        }
        else
        {
            Debug.LogError("Saved NNet is incompatible with current NNet");
        }
    }

    public string Save() // returns biases + weights formated to json
    {
        SavedNetwork save = new SavedNetwork();

        save.inputCount = inputCount;
        save.outputCount = outputCount;
        save.hiddenLayerCount = hiddenLayerCount;
        save.hiddenNeuronCount = hiddenNeuronCount;

        save.weights = WeightMatrixToArray(weights);
        save.biases = biases;

        string json = JsonConvert.SerializeObject(save);
        return json;
    }

    private static List<float[,]> WeightMatrixToArray(List<Matrix<float>> weights) // converts list of matrix to list of float array to be serialised
    {
        List<float[,]> newWeights = new List<float[,]>();
        foreach (Matrix<float> w in weights)
        {
            newWeights.Add(w.ToArray());
        }
        return newWeights;
    }

    private static List<Matrix<float>> WeightArrayToMatrix(List<float[,]> weights) // converts de serialised float array to list of matrices 
    {
        List<Matrix<float>> newWeights = new List<Matrix<float>>();
        foreach (float[,] w in weights)
        {
            newWeights.Add(Matrix<float>.Build.DenseOfArray(w));
        }
        return newWeights;
    }

    private class SavedNetwork // structure to save data
    {
        public int inputCount;
        public int outputCount;
        public int hiddenLayerCount;
        public int hiddenNeuronCount;

        public List<float[,]> weights;
        public List<float> biases;
    }
   */

}
