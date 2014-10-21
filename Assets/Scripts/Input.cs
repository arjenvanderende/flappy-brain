using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;

public class Input : MonoBehaviour {

	UDPPacketIO udp;
	Osc handler;
	Single beta;
	Single[] betaArray = new Single[20]; 
	Single smoothedBeta;
	int betaCounter = 0;
	Single concentrationBase;
	Single[] concArray = new Single[10];
	int concCounter = 0;

	void Start () {

		udp = GetComponent<UDPPacketIO>();
		udp.init("127.0.0.1", 3001, 3000);
		
		handler = GetComponent<Osc>();
		handler.init(udp);
		handler.SetAddressHandler("/muse/dsp/elements/beta", BetaMessage);
		handler.SetAddressHandler("/muse/dsp/blink", BlinkMessage);

		// Twice a second log beta value to console
		InvokeRepeating("LogBeta", 3, 1);
		InvokeRepeating("GetSmoothed", 3, (float)0.1);
		InvokeRepeating ("GetConcentrated", 3, 1);

		concentrationBase = (Single)0.10;
	}

	void BetaMessage(OscMessage message) {
		// messages for dsp elements are of the Single type and can be NaN
		Single avgFP = AverageFP (message);
		if (!Single.IsNaN (avgFP)) {
			beta = avgFP;
		}
	}

	void BlinkMessage(OscMessage message) {
		if (message.Values.Count == 1 && (int)message.Values [0] == 1) {
			BlinkEvent();
		}
	}

	void BlinkEvent() {
		Debug.Log ("Blinked!");
	}

	void Update() {
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
	void GetSmoothed(){
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
	void GetConcentrated() {
		concArray [concCounter] = smoothedBeta;
		Single result = 0;
		foreach (Single value in concArray) {
			if (Single.IsNaN(value))
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
