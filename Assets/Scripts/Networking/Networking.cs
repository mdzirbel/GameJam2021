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
				byte[] incommingData = procData.Dequeue();
				if (incommingData[0] == 1)
				{
					var ship = Instantiate(exampleShip, new Vector3(0, 0, 0), Quaternion.identity);
					ships.Add(incommingData[1], ship);
				}
				if (incommingData[0] == 2)
				{
					var ship = ships[incommingData[1]];
					ship.transform.position = new Vector3(getFloat(incommingData, 2), getFloat(incommingData, 6), 0);
					ship.transform.eulerAngles = new Vector3(0, 0, getFloat(incommingData, 10));
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
		float f = System.BitConverter.ToSingle(theArray, start);
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
					// Read incomming stream into byte arrary. 					
					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
					{
						var incommingData = new byte[length];
						Array.Copy(bytes, 0, incommingData, 0, length);
						Debug.Log(incommingData[0]);
						if (incommingData[0] == 2)
						{
							//Debug.Log(incommingData[2] + "\t" + incommingData[3] + "\t" + incommingData[4] + "\t" + incommingData[5]);
						}
						lock (threadLocker)
						{
							procData.Enqueue(incommingData);
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
		float xPos = obj.transform.position.x;
		float yPos = obj.transform.position.y;
		float rot = obj.transform.eulerAngles.z;
		byte[] outBytes = new byte[13];
		outBytes[0] = 2;
		System.Buffer.BlockCopy(BitConverter.GetBytes(xPos), 0, outBytes, 1, 4);
		System.Buffer.BlockCopy(BitConverter.GetBytes(yPos), 0, outBytes, 5, 4);
		//Debug.Log(outBytes[5] + "\t" + outBytes[6] + "\t" + outBytes[7] + "\t" + outBytes[8]);
		System.Buffer.BlockCopy(BitConverter.GetBytes(rot), 0, outBytes, 9, 4);
		SendMessage(outBytes);
    }
}