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
	#region private members 	
	private TcpClient socketConnection;
	private Thread clientReceiveThread;
	#endregion
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
	/// <summary> 	
	/// Runs in background clientReceiveThread; Listens for incomming data. 	
	/// </summary>     
	private void ListenForData()
	{
		try
		{
			socketConnection = new TcpClient("localhost", 4162);
			Debug.Log("Connecting");
			Byte[] bytes = new Byte[1024];
			while (true)
			{
				// Get a stream object for reading 				
				using (NetworkStream stream = socketConnection.GetStream())
				{
					int length;
					// Read incomming stream into byte arrary. 					
					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
					{
						var incommingData = new byte[length];
						Array.Copy(bytes, 0, incommingData, 0, length);
						// Convert byte array to string message. 						
						string serverMessage = Encoding.ASCII.GetString(incommingData);
						Debug.Log("server message received as: " + serverMessage);
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
		System.Buffer.BlockCopy(BitConverter.GetBytes(rot), 0, outBytes, 9, 4);
		SendMessage(outBytes);
    }
}