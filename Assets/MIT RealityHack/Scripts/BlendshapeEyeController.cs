﻿using UnityEngine;
using System.Collections;
//https://forum.unity.com/threads/blendshape-eye-controller-look-at-random-eyes-and-random-blink.322781/
public class BlendshapeEyeController : MonoBehaviour {

	private SkinnedMeshRenderer blendshapeEyeMesh;
	public SkinnedMeshRenderer _blendshapeEyeMesh{get{return blendshapeEyeMesh;}}
	public int positiveXAxis;
	public int negativeXAxis;
	public int positiveYAxis;
	public int negativeYAxis;
	public int blinkerEyes;

	[Header("Blink Resetter")]
	public bool resetWhenBlink;
	public int[] resetBlinkIndex;

	[Header("Eye Movement")]
	public bool moveableEyes = true;
	public Transform target;
	public float targetMultiplier = 10.0f;
	[Range(-1.0f, 1.0f)]
	public float _2DTargetX;
	[Range(-1.0f,1.0f)]
	public float _2DTargetY;

	public bool randomEyes = false;
	public bool random2DEyes = false;
	public float[] random2DEyesTime = new float[2];
	public float randomCompressor = 10.0f;
	public bool blinkable = true;
	public float blinkInterval = 3.0f;
	public float blinkSpeed = 0.3f;

	// DEBUGGER
	[Header("Debugger")]
	public bool isDebugging = false;
	public float gizmoSize = 0.02f;

	public string pxName;
	public string nxName;
	public string pyName;
	public string nyName;
	public string blinkerName;

	private Vector4 twoDeePos;
	private Vector4 triDeePos;
	private GameObject randomizerTarget;
	private bool isBlinking = false;
	private bool isBlinkedFull = false;
	private bool isBlinkinged = false;
	private float minz = -1.0f;

	private float[] beforeBlinkVal;

	private float m_2DBlinkIntervaler = -1.0f;
	private float[] m_2DBlinkTarget = new float[2];
	//private float[] m_2DBlinkBeforeVal = new float[2];
	private float[] m_2DBlinkVel = new float[2];
	private float m_2DBlinkSmoothTime  = 0.3f;
	private bool m_2DLerpNow = false;

	/*
	 * 
	 * vx = px
	 * vy = py
	 * vz = nx
	 * vw = ny
	 * 
	*/

	// Use this for initialization
	void Awake () {
		blendshapeEyeMesh = GetComponent<SkinnedMeshRenderer>();

		if(!blendshapeEyeMesh) enabled = false;

		if(isDebugging){
			print(blendshapeEyeMesh.sharedMesh.blendShapeCount);
			pxName = blendshapeEyeMesh.sharedMesh.GetBlendShapeName(positiveXAxis);
			nxName = blendshapeEyeMesh.sharedMesh.GetBlendShapeName(negativeXAxis);
			pyName = blendshapeEyeMesh.sharedMesh.GetBlendShapeName(positiveYAxis);
			nyName = blendshapeEyeMesh.sharedMesh.GetBlendShapeName(negativeYAxis);
			blinkerName = blendshapeEyeMesh.sharedMesh.GetBlendShapeName(blinkerEyes);
		}

		//if(resetWhenBlink) beforeBlinkVal = new float[resetBlinkIndex[1]-resetBlinkIndex[0]];
		if(resetWhenBlink){ 
			beforeBlinkVal = new float[blendshapeEyeMesh.sharedMesh.blendShapeCount];
			for(int i = resetBlinkIndex[0]; i < resetBlinkIndex[1]; i++){
				beforeBlinkVal[i] = blendshapeEyeMesh.GetBlendShapeWeight(i);
			}
		}
	}
	
	// Update is called once per frame
	void LateUpdate () {
		if(!Camera.main) return;
		if(Vector3.Distance(transform.position,Camera.main.transform.position) > 5.0f) return;

		if(isDebugging) return;

		if(moveableEyes){ // MOVEABLE EYES START
			if(target){
				Vector3 newTgt = new Vector3(0,0,0);
				newTgt = target.transform.InverseTransformPoint(blendshapeEyeMesh.transform.position);

				triDeePos.x = newTgt.x * targetMultiplier;
				triDeePos.y = newTgt.y * targetMultiplier;
				triDeePos.z = -(newTgt.x) * targetMultiplier;
				triDeePos.w = -(newTgt.y) * targetMultiplier;

				blendshapeEyeMesh.SetBlendShapeWeight(positiveXAxis,triDeePos.x); // +X
				blendshapeEyeMesh.SetBlendShapeWeight(positiveYAxis,triDeePos.y); // +Y
				blendshapeEyeMesh.SetBlendShapeWeight(negativeXAxis,triDeePos.z); // -X
				blendshapeEyeMesh.SetBlendShapeWeight(negativeYAxis,triDeePos.w); // -Y
			}else{
				if(random2DEyes){
					if(Time.time - Random.Range(random2DEyesTime[0],random2DEyesTime[1]) > m_2DBlinkIntervaler){
						m_2DBlinkIntervaler = Time.time - Time.deltaTime;

						m_2DBlinkTarget[0] = Random.Range(-1.0f,1.0f);
						m_2DBlinkTarget[1] = Random.Range(-0.5f,0.5f);

						m_2DBlinkVel[0] = 0f;
						m_2DBlinkVel[1] = 0f;

						m_2DLerpNow = true;

						//m_2DBlinkBeforeVal[0] = _2DTargetX;
						//m_2DBlinkBeforeVal[1] = _2DTargetY;
						//m_2DLerpNow = true;
					}

					if(m_2DLerpNow){
						if(m_2DBlinkVel[0] < 1.0f) m_2DBlinkVel[0] += Time.deltaTime;
						if(m_2DBlinkVel[1] < 1.0f) m_2DBlinkVel[1] += Time.deltaTime;

						_2DTargetX = Mathf.Lerp(_2DTargetX, m_2DBlinkTarget[0],m_2DBlinkVel[0]);
						_2DTargetY = Mathf.Lerp(_2DTargetY, m_2DBlinkTarget[1],m_2DBlinkVel[1]);

						//print(m_2DBlinkVel[0]);

						if(m_2DBlinkVel[0] > 1.0f && m_2DBlinkVel[1] > 1.0f) m_2DLerpNow = false;
					}

					/*if(m_2DLerpNow){
						//if(_2DTargetX != m_2DBlinkBeforeVal[0] || _2DTargetY != m_2DBlinkBeforeVal[1]){
							_2DTargetX = Mathf.SmoothDamp(_2DTargetX, m_2DBlinkTarget[0],ref m_2DBlinkVel[0], m_2DBlinkSmoothTime);
							_2DTargetY = Mathf.SmoothDamp(_2DTargetY,m_2DBlinkTarget[1], ref m_2DBlinkVel[1], m_2DBlinkSmoothTime);
						}else{
							m_2DLerpNow = false;
						}
					}*/
				}

				if(_2DTargetX>0 || _2DTargetY>0){ //Positive Converter
					twoDeePos.x = _2DTargetX;
					twoDeePos.y = _2DTargetY;
				}

				if(_2DTargetX < 0 || _2DTargetY < 0){
					twoDeePos.z = -_2DTargetX;
					twoDeePos.w = -_2DTargetY;
				}

				blendshapeEyeMesh.SetBlendShapeWeight(positiveXAxis,twoDeePos.x*targetMultiplier); // +X
				blendshapeEyeMesh.SetBlendShapeWeight(positiveYAxis,twoDeePos.y*targetMultiplier);// +Y
				blendshapeEyeMesh.SetBlendShapeWeight(negativeXAxis,twoDeePos.z*targetMultiplier);// -X
				blendshapeEyeMesh.SetBlendShapeWeight(negativeYAxis,twoDeePos.w*targetMultiplier);// -Y
			}

			if(randomEyes){
				if(!randomizerTarget){
					randomizerTarget = new GameObject("RNDTRG");
					randomizerTarget.transform.position = blendshapeEyeMesh.transform.position;
					target = randomizerTarget.transform;
				}else{
					target.position += Random.onUnitSphere * Time.fixedDeltaTime/randomCompressor;
				}
			}
		}

		// MOVEABLE EYES END

		if(blinkable){
			float blinkVal = blendshapeEyeMesh.GetBlendShapeWeight(blinkerEyes);
			//print(blinkVal + transform.root.name);
			if(Time.time - blinkInterval > minz && !isBlinking && !isBlinkedFull && !isBlinkinged){
				minz = Time.time - Time.deltaTime;
				/*if(resetWhenBlink){
					for(int i = 0; i < beforeBlinkVal.Length; i++){
						beforeBlinkVal[i] = blendshapeEyeMesh.GetBlendShapeWeight(i);
					}
				}*/
				isBlinking = true;
			}

			if(isBlinking){
				if(!isBlinkinged){
					if(blinkVal < 99.0f && !isBlinkedFull){
						
						/*if(resetWhenBlink){
							for(int i = resetBlinkIndex[0]; i < resetBlinkIndex[1]; i++){
								float iz = Mathf.Lerp(beforeBlinkVal[i],0.0f,blinkSpeed*1.5f);
								blendshapeEyeMesh.SetBlendShapeWeight(i, iz);
								//print(iz + " | "+ beforeBlinkVal[i]);
							}
						}*/

						if(resetWhenBlink){
							for(int i = resetBlinkIndex[0]; i < resetBlinkIndex[1]; i++){
								blendshapeEyeMesh.SetBlendShapeWeight(i, 0.0f);
								//print(iz + " | "+ beforeBlinkVal[i]);
							}
						}

						blinkVal = Mathf.Lerp(blinkVal, 100.0f, blinkSpeed);
						if(blinkVal >= 99.0f) isBlinkedFull = true;
					}

					if(isBlinkedFull){
						
						/*if(resetWhenBlink){
							for(int i = resetBlinkIndex[0]; i < resetBlinkIndex[1]; i++){
								float iz = Mathf.Lerp(0f,beforeBlinkVal[i],blinkSpeed*1.5f);
								blendshapeEyeMesh.SetBlendShapeWeight(i, iz);
							}
						}*/

						if(resetWhenBlink){
							for(int i = resetBlinkIndex[0]; i < resetBlinkIndex[1]; i++){
								blendshapeEyeMesh.SetBlendShapeWeight(i, beforeBlinkVal[i]);
								//print(iz + " | "+ beforeBlinkVal[i]);
							}
						}

						blinkVal = Mathf.Lerp(blinkVal, 0.0f, blinkSpeed);
						if(blinkVal <= 2.0f) isBlinkinged = true;
					}
				}

				blendshapeEyeMesh.SetBlendShapeWeight(blinkerEyes,blinkVal);

				/*if(resetWhenBlink){
					for(int i = resetBlinkIndex[0]; i < resetBlinkIndex[1]; i++){
						blendshapeEyeMesh.SetBlendShapeWeight(i, blinkVal);
					}
				}*/

				if(isBlinkinged){
					blendshapeEyeMesh.SetBlendShapeWeight(blinkerEyes,0);
					isBlinking = false;
					isBlinkinged = false;
					isBlinkedFull = false;
				}
			}
		}
	}

	void OnDrawGizmos(){
		if(target && blendshapeEyeMesh){
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(target.position, gizmoSize);
			Gizmos.DrawLine(blendshapeEyeMesh.transform.position,target.position);
		}
	}

	void OnBecameInvisible(){
		enabled = false;
	}

	void OnBecameVisible(){
		enabled = true;
	}
}