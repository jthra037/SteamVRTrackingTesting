using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class Importer : MonoBehaviour {

    public AnimationClip animationOVR;
    public AnimationClip animationUnity;

    public TextAsset DataOVR;
    public TextAsset DataUnity;

    public bool read;

    public float positionFactor = 1;
    public int numLines;

    private void Start()
    {
        if (GetComponent<Animation>())
            GetComponent<Animation>().Play();
    }

    // Update is called once per frame
    void Update () {
        if (read)
        {
            read = false;

            animationOVR.ClearCurves();
            animationUnity.ClearCurves();


            AnimationCurve positionXUnity = new AnimationCurve();
            AnimationCurve positionYUnity = new AnimationCurve();
            AnimationCurve positionZUnity = new AnimationCurve();
            AnimationCurve rotationXUnity = new AnimationCurve();
            AnimationCurve rotationYUnity = new AnimationCurve();
            AnimationCurve rotationZUnity = new AnimationCurve();
            AnimationCurve rotationWUnity = new AnimationCurve();


            string[] linesOVR = DataOVR.text.Split('\n');
            string[] linesUnity = DataUnity.text.Split('\n');


            //ovr data has empty line at start and end
            long firstTimestampOVR = long.Parse(linesOVR[1].Split(new char[] { ',' }, 9)[0]);
            long lastTimestampOVR = long.Parse(linesOVR[linesOVR.Length - 2].Split(new char[] { ',' }, 9)[0]);


            long firstTimestampUnity = long.Parse(linesUnity[0].Split(new char[] { ',' }, 9)[0]);
            long lastTimestampUnity = long.Parse(linesUnity[linesUnity.Length - 2].Split(new char[] { ',' }, 9)[0]);


            string[] lineUnity = new string[9];
            string[] lineOVR = new string[9];

            int firstIndexOVR = 1;
            int firstIndexUnity = 0;

            int lastIndexOVR = linesOVR.Length - 2;
            int lastIndexUnity = linesUnity.Length - 2;
            
            //find the index of the first common timestamp
            while (true)
            {
                lineOVR = linesOVR[firstIndexOVR].Split(new char[] { ',' }, 9);
                lineUnity = linesUnity[firstIndexUnity].Split(new char[] { ',' }, 9);

                if (ulong.Parse(lineOVR[0]) < ulong.Parse(lineUnity[0]))
                {
                    firstIndexOVR++;
                    continue;
                }
                else if (ulong.Parse(lineOVR[0]) > ulong.Parse(lineUnity[0]))
                {
                    firstIndexUnity++;
                    continue;
                }
                else
                    break;
            }


            //find the index of the last common timestamp
            while (true)
            {
                lineOVR = linesOVR[lastIndexOVR].Split(new char[] { ',' }, 9);
                lineUnity = linesUnity[lastIndexUnity].Split(new char[] { ',' }, 9);

                if (ulong.Parse(lineOVR[0]) > ulong.Parse(lineUnity[0]))
                {
                    lastIndexOVR--;
                    continue;
                }
                else if (ulong.Parse(lineOVR[0]) < ulong.Parse(lineUnity[0]))
                {
                    lastIndexUnity--;
                    continue;
                }
                else
                    break;
            }


            ParseData(firstIndexOVR, lastIndexOVR, linesOVR, animationOVR, false);
            ParseData(firstIndexUnity, lastIndexUnity, linesUnity, animationUnity, true);



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
        }

        animation.SetCurve("", typeof(Transform), "m_LocalPosition.x", positionX);
        animation.SetCurve("", typeof(Transform), "m_LocalPosition.y", positionY);
        animation.SetCurve("", typeof(Transform), "m_LocalPosition.z", positionZ);
        animation.SetCurve("", typeof(Transform), "m_LocalRotation.w", rotationW);
        animation.SetCurve("", typeof(Transform), "m_LocalRotation.x", rotationX);
        animation.SetCurve("", typeof(Transform), "m_LocalRotation.y", rotationY);
        animation.SetCurve("", typeof(Transform), "m_LocalRotation.z", rotationZ);
    }

    public static float RemapUnclamped(float s, float from1, float from2, float to1, float to2)
    {
        return (to1 + (s - from1) * (to2 - to1) / (from2 - from1));
    }
}
