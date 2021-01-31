using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;


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
	long lastSend = 0;
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
				else if (incomingData[0] == 2)
				{
					var ship = ships[incomingData[1]];
					ship.transform.position = new Vector3(getFloat(incomingData, 2), getFloat(incomingData, 6), 0);
					ship.transform.eulerAngles = new Vector3(0, 0, getFloat(incomingData, 10));
				}
				else if (incomingData[0] == 3)
				{
					var ship = ships[incomingData[1]];
					Debug.Log("Destroying " + ship);
					Destroy(ship);
				}
				else if(incomingData[0]==4)
                {
					SceneManager.LoadScene("Game");
				}
			}
			if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastSend > 500)
			{
				lastSend = DateTimeOffset.Now.ToUnixTimeMilliseconds();
				SendMessage(new byte[] { 4 });
			}
		}
	}
	void OnApplicationQuit()
	{
		Debug.Log("Closing Socket");
		SendMessage(new byte[] { 3 });
		socketConnection.GetStream().Flush();
		socketConnection.Close();
		Debug.Log("Closed");
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
			lastSend = DateTimeOffset.Now.ToUnixTimeMilliseconds();
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
			socketConnection = new TcpClient("74.140.3.27", 4162);
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
	public void CreateGameAL()
    {

    }
	public void CreateGame()
	{
		byte[] message = new byte[1];
		message[0] = 0;
		SendMessage(message);
	}
	public void ConnectToGameAL()
	{
		ConnectToGame("AAAA");
	}
	public void ConnectToGame(string id)
    {
		byte[] message = new byte[5];
		message[0] = 1;
		byte[] idBytes = Encoding.ASCII.GetBytes(id);
		Array.Copy(idBytes, 0, message, 1, 4);
		SendMessage(message);
    }
	public void SendPosition(GameObject obj)
	{
		byte[] xPos = BitConverter.GetBytes(obj.transform.position.x);
		byte[] yPos = BitConverter.GetBytes(obj.transform.position.y);
		byte[] rot = BitConverter.GetBytes(obj.transform.rotation.eulerAngles.z);
		byte[] outBytes = new byte[13];
		outBytes[0] = 2;
		Buffer.BlockCopy(xPos, 0, outBytes, 1, 4);
		Buffer.BlockCopy(yPos, 0, outBytes, 5, 4);
        Buffer.BlockCopy(rot, 0, outBytes, 9, 4);
		SendMessage(outBytes);
	}
}