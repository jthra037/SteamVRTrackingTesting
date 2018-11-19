using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public struct FrameRecord
{
	public Vector3 pos;
	public Vector3 localPos;
	public Quaternion rot;
	public Quaternion localRot;
	public Vector3 eulers;
	public int frameIndex;
	public float deltaTime;
	public float gameTime;
	public float realtime;
	public long sysTime;

	public void Print()
	{
		Debug.Log(string.Format(("Position: {0}\n" +
			"Rotation: {1}\n" +
			"GameTime: {2}\n" +
			 "RealTime: {3}"), pos, eulers, gameTime, realtime));
	}
}

public class ObjectMovementRecorder : MonoBehaviour
{

	public Transform TrackerReference;

	public int listLength = 4000;
	public LinkedList<FrameRecord> record;

	public LineRenderer lr;
	public bool renderLine;
	public bool logToConsole;
	public bool printToFile;
	public string fileName;

	private int listID;

	public void Start()
	{
		listID = 0;
		record = new LinkedList<FrameRecord>();

		if (lr)
			lr.positionCount = listLength;
	}

	public void Update()
	{
		int id = Time.frameCount % listLength;
		DateTime now = DateTime.UtcNow;
		FrameRecord newEntry = new FrameRecord
		{
			deltaTime = Time.deltaTime,
			eulers = TrackerReference.eulerAngles,
			frameIndex = Time.frameCount,
			gameTime = Time.time,
			pos = TrackerReference.position,
			localPos = TrackerReference.localPosition,
			realtime = Time.realtimeSinceStartup,
			rot = TrackerReference.rotation,
			localRot = TrackerReference.localRotation,
			sysTime = (now.Millisecond) + (now.Second * 1000) + (now.Minute * 60 * 1000),
		};
		record.AddLast(newEntry);

		if(lr && renderLine)
		{
			lr.SetPosition(id, newEntry.pos);
		}

		if(logToConsole)
		{
			newEntry.Print();
		}
	}

	private void writeToFile(ICollection<FrameRecord> record, string name)
	{
		if (File.Exists(name))
		{
			Console.WriteLine("{0} already exists.", name);
			return;
		}
		StreamWriter sr = File.CreateText(name);
		foreach(FrameRecord entry in record)
		{
			sr.WriteLine("{0}, RightHand, x = {1}, y = {2}, z = {3}, qw = {4}, qx = {5}, qy = {6}, qz = {7}",
				entry.sysTime, entry.localPos.x, entry.localPos.y, entry.localPos.z, entry.localRot.w, entry.localRot.x, entry.localRot.y, entry.localRot.z);
		}
		sr.Close();
	}

	private void OnDestroy()
	{
		writeToFile(record, fileName+DateTime.Now.Minute + ".txt");
	}
}
