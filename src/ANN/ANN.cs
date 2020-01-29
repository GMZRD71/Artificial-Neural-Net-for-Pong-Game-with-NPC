using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CLASS
// -----
// This class essentially provides the structure for an artificial neural network
// This network can be used without training; it does not need to train the whole time.
// So, this only trains at the begining.  Once the network is being used, the weights
// does not need to be adjusted.
public class ANN{

	public int numInputs;
	public int numOutputs;
	public int numHidden;
	public int numNPerHidden;
	public double alpha;
	List<Layer> layers = new List<Layer>();

	// Artificial Neural Network Class (ANN)
	// Basic structure of the network
	public ANN(int nI, int nO, int nH, int nPH, double a)
	{
		numInputs = nI;
		numOutputs = nO;
		numHidden = nH;
		numNPerHidden = nPH;
		alpha = a;

		if(numHidden > 0)
		{
			layers.Add(new Layer(numNPerHidden, numInputs));

			for(int i = 0; i < numHidden-1; i++)
			{
				layers.Add(new Layer(numNPerHidden, numNPerHidden));
			}

			layers.Add(new Layer(numOutputs, numNPerHidden));
		}
		else
		{
			layers.Add(new Layer(numOutputs, numInputs));
		}
	}

	// Method 'Train' which consists of type List<double>
	// This actually trains
	public List<double> Train(List<double> inputValues, List<double> desiredOutput)
    // INPUTS
	//        List<double> inputValues --- dynamic-size array that holds a variable amount
	//                                     of variables in the input array
	//        List<double> desiredOutput - dynamic-size array that holds a variable amount
	//                                     of variables for the desired output
	// OUTPUTS
	//        List<double> Train(...) ---- dynamic-size array that holds a variable amount
	//                                     of training weights
	{
		List<double> outputValues = new List<double>();
		outputValues = CalcOutput(inputValues, desiredOutput);
		UpdateWeights(outputValues, desiredOutput);
		return outputValues;
	}

	// Method 'CalcOutput which consist of a type List<double>
	// This calculates the outputs without training
	public List<double> CalcOutput(List<double> inputValues, List<double> desiredOutput)
    // INPUTS
	//        Very similar to Train
	// OUTPUTS
	//        Very similar to Train
	{
		List<double> inputs = new List<double>();
		List<double> outputValues = new List<double>();
		int currentInput = 0;

		if(inputValues.Count != numInputs)
		{
			Debug.Log("ERROR: Number of Inputs must be " + numInputs);
			return outputValues;
		}

		inputs = new List<double>(inputValues);
		for(int i = 0; i < numHidden + 1; i++)
		{
				if(i > 0)
				{
					inputs = new List<double>(outputValues);
				}
				outputValues.Clear();

				for(int j = 0; j < layers[i].numNeurons; j++)
				{
					double N = 0;
					layers[i].neurons[j].inputs.Clear();

					for(int k = 0; k < layers[i].neurons[j].numInputs; k++)
					{
					    layers[i].neurons[j].inputs.Add(inputs[currentInput]);
						N += layers[i].neurons[j].weights[k] * inputs[currentInput];
						currentInput++;
					}

					N -= layers[i].neurons[j].bias;

					if(i == numHidden)
						layers[i].neurons[j].output = ActivationFunctionO(N);
					else
						layers[i].neurons[j].output = ActivationFunction(N);
					
					outputValues.Add(layers[i].neurons[j].output);
					currentInput = 0;
				}
		}
		return outputValues;
	}

	// Method 'PrintWeights' which provides the results 
	public string PrintWeights()
	{
		string weightStr = "";
		foreach(Layer l in layers)
		{
			foreach(Neuron n in l.neurons)
			{
				foreach(double w in n.weights)
				{
					weightStr += w + ",";
				}
			}
		}
		return weightStr;
	}

	// Method 'LoadWeights' which loads the weights previously calculated
	// instead of having to retrain every time
	public void LoadWeights(string weightStr)
	{
		if(weightStr == "") return;
		string[] weightValues = weightStr.Split(',');
		int w = 0;
		foreach(Layer l in layers)
		{
			foreach(Neuron n in l.neurons)
			{
				for(int i = 0; i < n.weights.Count; i++)
				{
					n.weights[i] = System.Convert.ToDouble(weightValues[w]);
					w++;
				}
			}
		}
	}
	
	// Method 'UpdateWeights' which is private and essentially does the back-propagation
	// through the layers
	void UpdateWeights(List<double> outputs, List<double> desiredOutput)
	{
		double error;
		for(int i = numHidden; i >= 0; i--)
		{
			for(int j = 0; j < layers[i].numNeurons; j++)
			{
				if(i == numHidden)
				{
					error = desiredOutput[j] - outputs[j];
					layers[i].neurons[j].errorGradient = outputs[j] * (1-outputs[j]) * error;
				}
				else
				{
					layers[i].neurons[j].errorGradient = layers[i].neurons[j].output * (1-layers[i].neurons[j].output);
					double errorGradSum = 0;
					for(int p = 0; p < layers[i+1].numNeurons; p++)
					{
						errorGradSum += layers[i+1].neurons[p].errorGradient * layers[i+1].neurons[p].weights[j];
					}
					layers[i].neurons[j].errorGradient *= errorGradSum;
				}	
				for(int k = 0; k < layers[i].neurons[j].numInputs; k++)
				{
					if(i == numHidden)
					{
						error = desiredOutput[j] - outputs[j];
						layers[i].neurons[j].weights[k] += alpha * layers[i].neurons[j].inputs[k] * error;
					}
					else
					{
						layers[i].neurons[j].weights[k] += alpha * layers[i].neurons[j].inputs[k] * layers[i].neurons[j].errorGradient;
					}
				}
				layers[i].neurons[j].bias += alpha * -1 * layers[i].neurons[j].errorGradient;
			}

		}

	}


	// VARIOUS ACTIVATION FUNCTIONS *************************************************************************************************
	// ******************************************************************************************************************************
	// For now, use TanH for both the hidden layers and the output layer

	double ActivationFunction(double value)
	{
		return TanH(value);
	}

	double ActivationFunctionO(double value)
	{
		return TanH(value);
	}

	double TanH(double value)
	{
		double k = (double) System.Math.Exp(-2*value);
    	return 2 / (1.0f + k) - 1;
	}

	double Sigmoid(double value) 
	{
    	double k = (double) System.Math.Exp(value);
    	return k / (1.0f + k);
	}
}
