using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public struct FrameRecord
{
	public Vector3 pos;
	public Quaternion rot;
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
	//private Thread writingThread;
	//private delegate void recordWriter(FrameRecord[] record);

	public void Start()
	{
		listID = 0;
		//records = new FrameRecord[][] { new FrameRecord[listLength], new FrameRecord[listLength] };
		//foreach(FrameRecord[] record in records)
		//{
		//	for(int i = 0; i < record.Length; i++)
		//	{
		//		record[i] = new FrameRecord();
		//	}
		//}
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
			realtime = Time.realtimeSinceStartup,
			rot = TrackerReference.rotation,
			sysTime = (now.Millisecond) + (now.Second * 1000) + (now.Minute * 60 * 1000),
		};
		record.AddLast(newEntry);


		//records[listID][id].deltaTime = Time.deltaTime;
		//records[listID][id].eulers = TrackerReference.eulerAngles;
		//records[listID][id].frameIndex = Time.frameCount;
		//records[listID][id].gameTime = Time.time;
		//records[listID][id].pos = TrackerReference.position;
		//records[listID][id].realtime = Time.realtimeSinceStartup;
		//records[listID][id].rot = TrackerReference.rotation;

		if(lr && renderLine)
		{
			lr.SetPosition(id, newEntry.pos);
		}

		if(logToConsole)
		{
			newEntry.Print();
		}

		//if (id == listLength - 1)
		//{
		//	//if (printToFile)
		//	//{
		//	//	writingThread = new Thread(new ThreadStart(curriedWriter(records[listID], (fileName+Time.frameCount+".txt"))));
		//	//	writingThread.Start();
		//	//}
		//
		//	listID = (listID + 1) % records.Length;
		//}
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
				entry.sysTime, entry.pos.x, entry.pos.y, entry.pos.z, entry.rot.w, entry.rot.x, entry.rot.y, entry.rot.z);
		}
		sr.Close();
	}

	private void OnDestroy()
	{
		writeToFile(record, fileName+record.Last.GetHashCode()+".txt");
	}

	//private Action curriedWriter(FrameRecord[] record, string name)
	//{
	//	return () =>
	//	{
	//		writeToFile(record, name);
	//	};
	//}
}
