using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class ImporterSingle : MonoBehaviour
{

	new public AnimationClip animation;

	public TextAsset data;

	public bool read;
	public bool isFromUnity;

	private void Start()
	{
		if (GetComponent<Animation>())
			GetComponent<Animation>().Play();
	}

	// Update is called once per frame
	void Update()
	{
		if (read)
		{
			read = false;

			animation.ClearCurves();

			AnimationCurve positionXUnity = new AnimationCurve();
			AnimationCurve positionYUnity = new AnimationCurve();
			AnimationCurve positionZUnity = new AnimationCurve();
			AnimationCurve rotationXUnity = new AnimationCurve();
			AnimationCurve rotationYUnity = new AnimationCurve();
			AnimationCurve rotationZUnity = new AnimationCurve();
			AnimationCurve rotationWUnity = new AnimationCurve();


			string[] lines = data.text.Split('\n');

			//ovr data has empty line at start and end
			long firstTimestamp = long.Parse(lines[1].Split(new char[] { ',' }, 9)[0]);
			long lastTimestamp = long.Parse(lines[lines.Length - 2].Split(new char[] { ',' }, 9)[0]);

			//string[] line = new string[9];

			ParseData(0, lines.Length - 2, lines, animation, isFromUnity);
		}

	}

	void ParseData(int firstLineIndex, int lastLineIndex, string[] data, AnimationClip animation, bool unity)
	{

		AnimationCurve positionX = new AnimationCurve();
		AnimationCurve positionY = new AnimationCurve();
		AnimationCurve positionZ = new AnimationCurve();
		AnimationCurve rotationX = new AnimationCurve();
		AnimationCurve rotationY = new AnimationCurve();
		AnimationCurve rotationZ = new AnimationCurve();
		AnimationCurve rotationW = new AnimationCurve();


		string[] line = new string[9];

		long firstTimestamp = long.Parse(data[firstLineIndex].Trim().Split(new char[] { ',' }, 9)[0]);

		//int keyIndex = 0;
		for (int i = firstLineIndex; i < lastLineIndex; i++)
		{
			line = data[i].Trim().Split(new char[] { ',' }, 9);
			float timestamp = (long.Parse(line[0]) - firstTimestamp) / 1000f;

			Vector3 p = new Vector3(
				float.Parse(line[2].Substring(4)),
				float.Parse(line[3].Substring(4)),
				float.Parse(line[4].Substring(4)));

			positionX.AddKey(timestamp, unity ? -p.x : p.x);
			positionY.AddKey(timestamp, p.y);
			positionZ.AddKey(timestamp, unity ? -p.z : p.z);

			Quaternion q = new Quaternion(
				float.Parse(line[6].Substring(5)),
				float.Parse(line[7].Substring(5)),
				float.Parse(line[8].Substring(5)),
				float.Parse(line[5].Substring(5)));

			//q = Quaternion.identity * q;

			rotationW.AddKey(timestamp, q.w);
			rotationX.AddKey(timestamp, unity ? -q.x : q.x);
			rotationY.AddKey(timestamp, unity ? q.y : -q.y);
			rotationZ.AddKey(timestamp, -q.z);


			//keyIndex++;
		}

		animation.SetCurve("", typeof(Transform), "m_LocalPosition.x", positionX);
		animation.SetCurve("", typeof(Transform), "m_LocalPosition.y", positionY);
		animation.SetCurve("", typeof(Transform), "m_LocalPosition.z", positionZ);
		animation.SetCurve("", typeof(Transform), "m_LocalRotation.w", rotationW);
		animation.SetCurve("", typeof(Transform), "m_LocalRotation.x", rotationX);
		animation.SetCurve("", typeof(Transform), "m_LocalRotation.y", rotationY);
		animation.SetCurve("", typeof(Transform), "m_LocalRotation.z", rotationZ);

		animation.EnsureQuaternionContinuity();

		for (int keyIndex = 0; keyIndex < positionX.keys.Length; keyIndex++)
		{

			AnimationUtility.SetKeyBroken(positionX, keyIndex, true);
			AnimationUtility.SetKeyBroken(positionY, keyIndex, true);
			AnimationUtility.SetKeyBroken(positionZ, keyIndex, true);
			AnimationUtility.SetKeyBroken(rotationX, keyIndex, true);
			AnimationUtility.SetKeyBroken(rotationY, keyIndex, true);
			AnimationUtility.SetKeyBroken(rotationZ, keyIndex, true);
			AnimationUtility.SetKeyBroken(rotationW, keyIndex, true);

			AnimationUtility.SetKeyLeftTangentMode(positionX, keyIndex, AnimationUtility.TangentMode.Linear);
			AnimationUtility.SetKeyLeftTangentMode(positionY, keyIndex, AnimationUtility.TangentMode.Linear);
			AnimationUtility.SetKeyLeftTangentMode(positionZ, keyIndex, AnimationUtility.TangentMode.Linear);
			AnimationUtility.SetKeyLeftTangentMode(rotationX, keyIndex, AnimationUtility.TangentMode.Linear);
			AnimationUtility.SetKeyLeftTangentMode(rotationY, keyIndex, AnimationUtility.TangentMode.Linear);
			AnimationUtility.SetKeyLeftTangentMode(rotationZ, keyIndex, AnimationUtility.TangentMode.Linear);
			AnimationUtility.SetKeyLeftTangentMode(rotationW, keyIndex, AnimationUtility.TangentMode.Linear);
		}
	}

	public static float RemapUnclamped(float s, float from1, float from2, float to1, float to2)
	{
		return (to1 + (s - from1) * (to2 - to1) / (from2 - from1));
	}
}
