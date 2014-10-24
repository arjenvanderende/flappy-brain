using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;

public class EegInput : MonoBehaviour {

	public double doubleBlinkRateInMilliseconds;
	public delegate void BlinkAction ();
	public static event BlinkAction OnBlink;
	public static event BlinkAction OnDoubleBlink;

	private UDPPacketIO udp;
	private Osc handler;

	private float beta;
	private float[] betaArray = new float[20]; 
	private float smoothedBeta;
	private int betaCounter = 0;
	private float concentrationBase;
	private float[] concArray = new float[100];
	private int concCounter = 0;
	private float acceptError = 0.2f;

	private DateTime prevBlinkTime = DateTime.UtcNow;
	private TimeSpan doubleBlinkRate = TimeSpan.FromMilliseconds (200);

	public enum ConcentrationLevel { Red, Orange, Green };

	void Start () {
		concentrationBase = 0.10f;
		doubleBlinkRate = TimeSpan.FromMilliseconds (doubleBlinkRateInMilliseconds);

		udp = GetComponent<UDPPacketIO> ();
		udp.init ("127.0.0.1", 3001, 3000);
		
		handler = GetComponent<Osc> ();
		handler.init (udp);
		handler.SetAddressHandler ("/muse/dsp/elements/beta", BetaMessage);
		handler.SetAddressHandler ("/muse/dsp/blink", BlinkMessage);
		handler.SetAddressHandler ("/muse/dsp/status_indicator", StatusIndicator);

		InvokeRepeating ("LogBeta", 3, 2);
		InvokeRepeating ("UpdateSmoothedBeta", 3, 0.1f);
		InvokeRepeating ("UpdateBaseLevel", 3, 0.1f);
	}

	void BetaMessage(OscMessage message) {
		// messages for dsp elements are of the float type and can be NaN
		float avgFP = AverageFP (message);
		if (!float.IsNaN (avgFP)) {
			beta = avgFP;
		}
	}

	void BlinkMessage(OscMessage message) {
		bool blinked = message.Values.Count == 1 && (int)message.Values [0] == 1;
		if (blinked) {
			Debug.Log ("Blinked!");
			if (OnBlink != null)
				OnBlink.Invoke ();

			DateTime now = DateTime.UtcNow;
			TimeSpan doubleBlinkDuration = now - prevBlinkTime;
			bool doubleBlinked = doubleBlinkDuration < doubleBlinkRate;
			prevBlinkTime = now;

			if (doubleBlinked) {
				Debug.Log ("Double blinked!");
				if (OnDoubleBlink != null)
					OnDoubleBlink.Invoke ();
			}
		}
	}

	void StatusIndicator(OscMessage message) {
		Debug.Log (Osc.OscMessageToString (message));
	}
	
	public bool IsConcentrated() {
		return smoothedBeta > (concentrationBase - concentrationBase * acceptError);
	}

	public ConcentrationLevel GetConcentrationLevel() {
		Single levelGreen = concentrationBase * (Single)1.10; // +10%
		Single levelOrange = concentrationBase * (Single)0.90; // -10%
		if (smoothedBeta > levelGreen) return ConcentrationLevel.Green;
		if (smoothedBeta > levelOrange) return ConcentrationLevel.Orange;
		return ConcentrationLevel.Red;
	}

	/**
	 * Calculate average of electrode FP1 and FP2
	 */ 
	float AverageFP(OscMessage message) {
		if (message.Values.Count <= 2)
			return float.NaN;
		float fp1 = (float)message.Values [1];
		float fp2 = (float)message.Values [2];
		if (float.IsNaN (fp1) || float.IsNaN (fp2))
			return float.NaN;
		else
			return (fp1 + fp2) / 2;
	}

	/**
	 * Smooth samples over all betaArray samples
	 */
	void UpdateSmoothedBeta(){
		betaArray [betaCounter] = beta;
		float result = 0;
		foreach (float value in betaArray) {
			if (float.IsNaN(value))
				return;
			result += value;
		}
		smoothedBeta = result / betaArray.Length;
		betaCounter++;
		if (betaCounter >= betaArray.Length)
			betaCounter = 0;
	}

	/**
	 * If average smoothedBeta over last X seconds is higher then 
	 * concentrationBase, then update with higher value
	 */
	void UpdateBaseLevel() {
		concArray [concCounter] = smoothedBeta;
		float result = 0;
		foreach (float value in concArray) {
			if (float.IsNaN(value))
				return;
			result += value;
		}
		result /= concArray.Length;

		if (result > concentrationBase)
			concentrationBase = result;

		concCounter++;
		if (concCounter >= concArray.Length)
			concCounter = 0;
	}

	void LogBeta() {
		Debug.Log ("Base: " + concentrationBase + " Beta:" + smoothedBeta + " (" + (smoothedBeta - concentrationBase) + ")");
	}
}
