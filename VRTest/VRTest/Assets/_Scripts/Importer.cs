using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class Importer : MonoBehaviour {

    public AnimationClip animation;
    public TextAsset text;
    public bool read;

    public float positionFactor = 1;
    public int numLines;
	
	// Update is called once per frame
	void Update () {
        if (read)
        {
            read = false;

            animation.ClearCurves();
            //EditorCurveBinding[] curveBinding = AnimationUtility.GetCurveBindings(animation);
            //string s = curveBinding[0].propertyName;
            AnimationCurve positionX = new AnimationCurve();
            AnimationCurve positionY = new AnimationCurve();
            AnimationCurve positionZ = new AnimationCurve();
            AnimationCurve rotationX = new AnimationCurve();
            AnimationCurve rotationY = new AnimationCurve();
            AnimationCurve rotationZ = new AnimationCurve();
            AnimationCurve rotationW = new AnimationCurve();


            string[] lines = text.text.Split('\n');
            string[] line = new string[9];

            int numLines2 = Mathf.Min(numLines, lines.Length - 2);

            float firstTimestamp = float.Parse(lines[1].Split(new char[] { ',' }, 9)[0]);
            float lastTimestamp = float.Parse(lines[numLines2-1].Split(new char[] { ',' }, 9)[0]);
            
            for (int i = 1; i < numLines2 -1; i++)
            {

                line = lines[i].Split(new char[] { ',' }, 9);

                if (line[1] == " HMD")
                    continue;

                float timestamp = (float.Parse(line[0]) - firstTimestamp) / 1000f;
                
                positionX.AddKey(timestamp, float.Parse(line[2].Substring(5)) * positionFactor);
                positionY.AddKey(timestamp, float.Parse(line[3].Substring(5)) * positionFactor);
                positionZ.AddKey(timestamp, float.Parse(line[4].Substring(5)) * positionFactor);
                
                rotationX.AddKey(timestamp, float.Parse(line[5].Substring(6)) * positionFactor);
                rotationY.AddKey(timestamp, float.Parse(line[6].Substring(6)) * positionFactor);
                rotationZ.AddKey(timestamp, float.Parse(line[7].Substring(6)) * positionFactor);
                rotationW.AddKey(timestamp, float.Parse(line[8].Substring(6)) * positionFactor);
            }

            animation.SetCurve("", typeof(Transform), "m_LocalPosition.x", positionX);
            animation.SetCurve("", typeof(Transform), "m_LocalPosition.y", positionY);
            animation.SetCurve("", typeof(Transform), "m_LocalPosition.z", positionZ);
            animation.SetCurve("", typeof(Transform), "m_LocalRotation.x", rotationX);
            animation.SetCurve("", typeof(Transform), "m_LocalRotation.y", rotationY);
            animation.SetCurve("", typeof(Transform), "m_LocalRotation.z", rotationZ);
            animation.SetCurve("", typeof(Transform), "m_LocalRotation.w", rotationW);
        }

    }

    public static float RemapUnclamped(float s, float from1, float from2, float to1, float to2)
    {
        return (to1 + (s - from1) * (to2 - to1) / (from2 - from1));
    }
}
