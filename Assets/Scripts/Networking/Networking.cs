using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;


public class Networking : MonoBehaviour
{
	public static Networking Instance;
	public GameObject exampleShip;
	#region private members 	
	private TcpClient socketConnection;
	private Thread clientReceiveThread;
	#endregion
	private Dictionary<int, GameObject> ships = new Dictionary<int, GameObject>();
	private System.Object threadLocker = new System.Object();
	// Use this for initialization 	
	void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("There is more than one instance!");
			return;
		}

		Instance = this;
		ConnectToTcpServer();
	}
	// Update is called once per frame
	void Update()
	{
		lock (threadLocker)
		{
			while (procData.Count > 0)
			{
				byte[] incomingData = procData.Dequeue();
				if (incomingData[0] == 1)
				{
					var ship = Instantiate(exampleShip, new Vector3(0, 0, 0), Quaternion.identity);
					ships.Add(incomingData[1], ship);
				}
				if (incomingData[0] == 2)
				{
					var ship = ships[incomingData[1]];
					ship.transform.position = new Vector3(getFloat(incomingData, 2), getFloat(incomingData, 6), 0);
					ship.transform.eulerAngles = new Vector3(0, 0, getFloat(incomingData, 10));
				}
			}
		}
	}
	void OnApplicationQuit()
    {
		Debug.Log("Closing Socket");
		socketConnection.Close();
	}
	/// <summary> 	
	/// Setup socket connection. 	
	/// </summary> 	
	private void ConnectToTcpServer()
	{
		try
		{
			clientReceiveThread = new Thread(new ThreadStart(ListenForData));
			clientReceiveThread.IsBackground = true;
			clientReceiveThread.Start();
		}
		catch (Exception e)
		{
			Debug.Log("On client connect exception " + e);
		}
	}
	float getFloat(byte[] theArray, int start)
	{
		float f = floatify(BitConverter.ToInt32(theArray, start));
		return f;
	}

	Queue<byte[]> procData = new Queue<byte[]>();
	/// <summary> 	
	/// Runs in background clientReceiveThread; Listens for incomming data. 	
	/// </summary>     
	private void ListenForData()
	{
		try
		{
			socketConnection = new TcpClient("localhost", 4162);
			Debug.Log("Connecting");
			while (true)
			{
				Byte[] bytes = new Byte[1024];
				// Get a stream object for reading 				
				using (NetworkStream stream = socketConnection.GetStream())
				{
					int length;
					// Read incoming stream into byte arrary. 					
					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
					{
						var incomingData = new byte[length];
						Array.Copy(bytes, 0, incomingData, 0, length);
						Debug.Log(incomingData[0]);
						if (incomingData[0] == 2)
						{
							//Debug.Log(incommingData[2] + "\t" + incommingData[3] + "\t" + incommingData[4] + "\t" + incommingData[5]);
						}
						lock (threadLocker)
						{
							procData.Enqueue(incomingData);
						}
					}
				}
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}
	/// <summary> 	
	/// Send message to server using socket connection. 	
	/// </summary> 	
	private void SendMessage(byte[] bytes)
	{
		if (socketConnection == null)
		{
			Debug.Log("Not Connected");
			return;
		}
		try
		{
			// Get a stream object for writing. 			
			NetworkStream stream = socketConnection.GetStream();
			if (stream.CanWrite)
			{
				stream.Write(bytes, 0, bytes.Length);
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}
	public void SendPosition(GameObject obj)
	{
		byte[] xPos = IntMutilation(intify(obj.transform.position.x));
		byte[] yPos = IntMutilation(intify(obj.transform.position.y));
		byte[] rot = IntMutilation(intify(obj.transform.eulerAngles.z));
		byte[] outBytes = new byte[16];
		outBytes[0] = 2;
		Buffer.BlockCopy(xPos, 0, outBytes, 1, 5);
		Buffer.BlockCopy(yPos, 0, outBytes, 6, 5);
        Buffer.BlockCopy(rot, 0, outBytes, 11, 5);
        //Debug.Log(outBytes[8].ToString("X") + " " + outBytes[7].ToString("X") + " " + outBytes[6].ToString("X") + " " + outBytes[5].ToString("X"));
		SendMessage(outBytes);
	}

	private int intify(float x)
	{
		return (int)(1000 * x);
	}
	private float floatify(int x)
	{
		return ((float)x) / 1000;
	}

	// Does unholy things to ints
	private static byte[] IntMutilation(int x)
	{
		// Create byte systems for integer and output long
		byte[] xBytes = BitConverter.GetBytes(x);
		byte[] mutilatedBytes = new byte[5];

		// Add 0x20 to the stupid numbers, and record that we did that in the extra space in the long
		for (int i = 0; i < 4; i++)
		{
			if (xBytes[i] >= 0x80 && xBytes[i] < 0xA0)
			{
				xBytes[i] += 0x20;
				mutilatedBytes[4] += (byte)(1 << i);
			}
		}
		// Copy the mutilated int into the long
		Buffer.BlockCopy(xBytes, 0, mutilatedBytes, 0, 4);
		return mutilatedBytes;
	}

	// I almost wish I believed in God so I could un-believe in him for this error
	private static int IntUnMutilation(byte[] x)
	{
		byte[] xBytes = new byte[4];
		Buffer.BlockCopy(x, 0, xBytes, 0, 4);
		for (int i = 0; i < 4; i++)
		{
			if ((x[4] & (byte)(1 << i)) > 0)
			{
				xBytes[i] -= 0x20;
			}
		}
		return BitConverter.ToInt32(xBytes, 0);
	}
}