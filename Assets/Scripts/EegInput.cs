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

	public delegate void SignalQualityChangedEvent(bool isGood);
	public static event SignalQualityChangedEvent OnSignalQualityChanged;

	public delegate void ConcentrationLevelChangedEvent(ConcentrationLevel level);
	public static event ConcentrationLevelChangedEvent OnConcentrationLevelChanged;

	private UDPPacketIO udp;
	private Osc handler;

	private Single beta;
	private Single[] betaArray = new Single[20]; 
	private Single smoothedBeta;
	private int betaCounter = 0;
	private Single concentrationBase;
	private Single[] concArray = new Single[100];
	private int concCounter = 0;
	private ConcentrationLevel concentrationLevel;
	private bool alwaysUpdateConcentrationBase = true;

	private bool signalQualityGood = false;
	private bool signalQualityChanged = false;

	private bool blinked = false;
	private bool doubleBlinked = false;
	private DateTime prevBlinkTime = DateTime.UtcNow;
	private TimeSpan doubleBlinkRate = TimeSpan.FromMilliseconds (200);

	void Start () {
		doubleBlinkRate = TimeSpan.FromMilliseconds (doubleBlinkRateInMilliseconds);

		udp = GetComponent<UDPPacketIO> ();
		udp.init ("127.0.0.1", 3001, 3000);
		
		handler = GetComponent<Osc> ();
		handler.init (udp);
		handler.SetAddressHandler ("/muse/dsp/elements/beta", BetaMessage);
		handler.SetAddressHandler ("/muse/dsp/blink", BlinkMessage);
		handler.SetAddressHandler ("/muse/dsp/elements/is_good", IsGoodMessage);

		InvokeRepeating ("Log", 3, 2);
		InvokeRepeating ("UpdateSmoothedBeta", 3, 0.1f);
		InvokeRepeating ("UpdateBaseLevel", 3, 0.1f);
	}

	void Update () {
		if (signalQualityChanged && OnSignalQualityChanged != null) {
			OnSignalQualityChanged.Invoke(signalQualityGood);
			signalQualityChanged = false;
		}
		if (blinked && OnBlink != null) {
			Debug.Log ("Blinked!");
			OnBlink.Invoke ();
			blinked = false;
		}
		if (doubleBlinked && OnDoubleBlink != null) {
			Debug.Log ("Double blinked!");
			OnDoubleBlink.Invoke ();
			doubleBlinked = false;
		}

		ConcentrationLevel previousConcentrationLevel = concentrationLevel;
		concentrationLevel = GetConcentrationLevel ();
		bool concentrationLevelChanged = previousConcentrationLevel != concentrationLevel;
		if (concentrationLevelChanged && OnConcentrationLevelChanged != null) {
			Debug.Log ("Concentation changed to: " + concentrationLevel);
			OnConcentrationLevelChanged.Invoke(concentrationLevel);
		}
	}

	void BetaMessage(OscMessage message) {
		// messages for dsp elements are of the Single type and can be NaN
		Single avgFP = AverageFP (message);
		if (!Single.IsNaN (avgFP)) {
			beta = avgFP;
		}
	}

	void BlinkMessage(OscMessage message) {
		blinked = message.Values.Count == 1 && (int)message.Values [0] == 1;
		if (blinked) {
			DateTime now = DateTime.UtcNow;
			TimeSpan doubleBlinkDuration = now - prevBlinkTime;
			doubleBlinked = doubleBlinkDuration < doubleBlinkRate;
			prevBlinkTime = now;
		}
	}
	
	public bool IsConcentrated() {
		return concentrationLevel == ConcentrationLevel.Orange ||
			   concentrationLevel == ConcentrationLevel.Green;
	}

	public ConcentrationLevel GetConcentrationLevel() {
		if (smoothedBeta == Single.NaN || concentrationBase == Single.NaN) {
			return ConcentrationLevel.Green;
		}

		Single levelGreen = concentrationBase * (Single)1.10; // +10%
		Single levelOrange = concentrationBase * (Single)0.80; // -20%
		if (smoothedBeta > levelGreen) return ConcentrationLevel.Green;
		if (smoothedBeta > levelOrange) return ConcentrationLevel.Orange;
		return ConcentrationLevel.Red;
	}

	void IsGoodMessage(OscMessage message) {
		if (message.Values.Count == 4) {
			bool isGood = message.Values[1].Equals(1) && message.Values[2].Equals(1);
			if (isGood != signalQualityGood) {
				Debug.Log("Signal quality good changed to: " + isGood);
				signalQualityChanged = true;
			}
			signalQualityGood = isGood;
		}
	}

	public bool IsSignalQualityGood() {
		return signalQualityGood;
	}

	/**
	 * Calculate average of electrode FP1 and FP2
	 */ 
	Single AverageFP(OscMessage message) {
		if (message.Values.Count <= 2)
			return Single.NaN;
		Single fp1 = (Single)message.Values [1];
		Single fp2 = (Single)message.Values [2];
		if (Single.IsNaN (fp1) || Single.IsNaN (fp2))
			return Single.NaN;
		else
			return (fp1 + fp2) / 2;
	}

	/**
	 * Smooth samples over all betaArray samples
	 */
	void UpdateSmoothedBeta(){
		if (!signalQualityGood) {
			return;
		}

		betaArray [betaCounter] = beta;
		Single result = 0;
		foreach (Single value in betaArray) {
			if (Single.IsNaN(value))
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
		if (!signalQualityGood) {
			return;
		}

		concArray [concCounter] = smoothedBeta;
		Single result = 0;
		foreach (Single value in concArray) {
			if (Single.IsNaN(value))
				return;
			result += value;
		}
		result /= concArray.Length;

		bool concentrationIncreased = result > concentrationBase;
		if (concentrationIncreased || alwaysUpdateConcentrationBase) {
			concentrationBase = result;
			if (!alwaysUpdateConcentrationBase) {
				Debug.LogWarning("Difficulty increased to: " + result);
			}
		}

		concCounter++;
		if (concCounter >= concArray.Length)
			concCounter = 0;
	}

	public void Reset() {
		beta = Single.NaN;
		betaArray = new Single[20]; 
		smoothedBeta = Single.NaN;
		betaCounter = 0;
		concentrationBase = Single.NaN;
		concArray = new Single[100];
		concCounter = 0;
		alwaysUpdateConcentrationBase = true;
	}

	public void StartTrainingConcentration() {
		alwaysUpdateConcentrationBase = false;
		for (int i = 0; i < concArray.Length; i++) {
			concArray[i] *= 0.75f;
		}
		UpdateBaseLevel ();
	}

	void Log() {
		Debug.Log ("Base: " + concentrationBase + " Beta:" + smoothedBeta + " (" + (smoothedBeta - concentrationBase) + ")");
		Debug.Log ("Good signal: " + IsSignalQualityGood ()); 
	}
}

public enum ConcentrationLevel { Red, Orange, Green };