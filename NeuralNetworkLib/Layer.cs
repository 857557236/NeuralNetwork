﻿using System;

namespace NeuralNetworkLib
{
    public enum LayerType
    {
        InputLayer,
        HiddenLayer,
        OutputLayer
    }

    public class Layer
    {
        #region public Members

        public Neuron[] Neurons { get; private set; }
        public long OutputsCount { get; private set; }
        public long InputsCount { get; private set; }
        public LayerType Type { get; private set; }
        public double[][] Weights { get; private set; }

        #endregion


        public Layer(long NeuronsCount, long InputsCount, LayerType layerType, Function<double, double> activationFunction)
        {
            Neurons = new Neuron[NeuronsCount];
            Type = layerType;

            for (long i = 0; i < Neurons.Length; i++)
                Neurons[i] = new Neuron(InputsCount, layerType, activationFunction);

            OutputsCount = NeuronsCount;
            this.InputsCount = InputsCount;
            setLayerWeights();
        }

        #region public Methods

        public double[] FeedForward(double[] inputs)
        {
            if (InputsCount != inputs.Length)
                throw new ArgumentException("Neurons of the layer don't accept the number of given inputs");

            if (Type == LayerType.InputLayer)
                return inputs;

            double[] outputs = new double[OutputsCount];

            for (long i = 0; i < Neurons.Length; i++)
                outputs[i] = Neurons[i].FeedForward(inputs);

            return outputs;
        }

        public double[] BackPropagation(double[] errors, double learningRate)
        {
            if (OutputsCount != errors.Length)
                throw new ArgumentException("Errors vector doesn't match the layer neurons count");

            double[] deltas = new double[errors.Length];

            for (long i = 0; i < Neurons.Length; i++)
                deltas[i] = Neurons[i].BackPropagation(errors[i], learningRate);

            return deltas;
        }

        public double[] BackPropagation(double[] nextLayerDeltas, double[][] nextLayerWeights, double learningRate)
        {
            if (Type == LayerType.InputLayer)
                return nextLayerDeltas;

            if (nextLayerDeltas.Length != nextLayerWeights.Length)
                throw new ArgumentException("Number of errors and neuron's weights doen't match");

            double[] thisLayerErrors = propagateErrors(nextLayerDeltas, nextLayerWeights);
            double[] thisLaterDeltas = new double[thisLayerErrors.Length];

            for (long i = 0; i < Neurons.Length; i++)
                thisLaterDeltas[i] = Neurons[i].BackPropagation(thisLayerErrors[i], learningRate);

            return thisLaterDeltas;
        }

        public void UpdateDerivatives()
        {
            if (Type == LayerType.InputLayer)
                return;

            foreach (var neuron in Neurons)
                neuron.UpdateDerivatives();
        }

        public override string ToString()
        {
            string res = $"{Type}, inputs {InputsCount}, outputs {OutputsCount}:\n";

            foreach (var neuron in Neurons)
            {
                res += "\n    " + neuron.ToString();
            }

            return res + "\n\n";
        }


        #endregion

        void setLayerWeights()
        {
            double[][] weights = new double[Neurons.Length][];

            for (long neuron = 0; neuron < Neurons.Length; neuron++)
                weights[neuron] = Neurons[neuron].Weights;

            Weights = weights;
        }

        /// <summary>
        /// Back propagate the error
        /// </summary>
        /// <param name="nextLayerErrors">errors from the next layer</param>
        /// <param name="nextLayerWeights">weights from the next layer</param>
        /// <returns>appropriate errors for the layer</returns>
        double[] propagateErrors(double[] nextLayerErrors, double[][] nextLayerWeights)
        {
            double[] thisLayerErrors = new double[Neurons.Length];

            for (long neuron = 0; neuron < Neurons.Length; neuron++)
                for (long neuronNextLayer = 0; neuronNextLayer < nextLayerErrors.Length; neuronNextLayer++)
                    thisLayerErrors[neuron] += nextLayerErrors[neuronNextLayer] * nextLayerWeights[neuronNextLayer][neuron];

            return thisLayerErrors;
        }
    }
}