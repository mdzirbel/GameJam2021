using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Networking : MonoBehaviour
{
	public static Networking Instance;
	public GameObject exampleShip;
	public GameObject exampleExplosion;
	public GameObject exampleShipExplosion;
	public GameObject examplePing;
	public GameObject exampleMissle;
	public string defaultGame;
	#region private members 	
	private TcpClient socketConnection;
	private Thread clientReceiveThread;
	#endregion
	private Dictionary<byte, GameObject> ships = new Dictionary<byte, GameObject>();
	private Dictionary<byte, GameObject> dumbMissles = new Dictionary<byte, GameObject>();
	private System.Object threadLocker = new System.Object();
	byte[] TYPE_TO_LENGTH = new byte[] { 5, 2, 14, 2, 2, 9, 2, 9, 9, 5, 14, 2};
	// Use this for initialization 	
	void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("There is more than one instance!");
			Destroy(gameObject);
			return;
		}
		DontDestroyOnLoad(this.gameObject);
		Instance = this;
		ConnectToTcpServer();
	}
	long lastSend = 0;
	long lastPingTime = 0;
	void FixedUpdate()
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
					Debug.Log("Destroying Ship " + ship);
					Destroy(ship);
				}
				else if (incomingData[0] == 4)
				{
					SceneManager.LoadScene("Game");
					//UnityEngine.UI.Text textLabel = GameObject.Find("YouLabel").GetComponent<UnityEngine.UI.Text>();
					//textLabel.text = "You are ship " + incomingData[1];
				}
				else if (incomingData[0] == 5)
				{
					float xPos = getFloat(incomingData, 1);
					float yPos = getFloat(incomingData, 5);
					Vector3 explosionPosition = new Vector3(xPos, yPos, 0);
					System.Random rnd = new System.Random();
					Quaternion rotation = Quaternion.Euler(0, 0, rnd.Next(360));
					GameObject explosion = Instantiate(exampleExplosion, explosionPosition, rotation);
				}
				else if (incomingData[0] == 6)
				{
				}
				else if (incomingData[0] == 7)
				{
					float xPos = getFloat(incomingData, 1);
					float yPos = getFloat(incomingData, 5);
					Vector3 explosionPosition = new Vector3(xPos, yPos, 0);
					System.Random rnd = new System.Random();
					Quaternion rotation = Quaternion.Euler(0, 0, rnd.Next(360));
					GameObject explosion = Instantiate(exampleShipExplosion, explosionPosition, rotation);
				}
				else if (incomingData[0] == 8)
				{
					float xPos = getFloat(incomingData, 1);
					float yPos = getFloat(incomingData, 5);
					Vector3 pingPosition = new Vector3(xPos, yPos, 0);
					System.Random rnd = new System.Random();
					Quaternion rotation = Quaternion.Euler(0, 0, rnd.Next(360));
					GameObject explosion = Instantiate(examplePing, pingPosition, rotation);
				}
				else if (incomingData[0] == 9)
				{
					lastPingTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
					GameObject textBefore = GameObject.FindWithTag("DisconnectedLabel");
					if (textBefore != null)
					{
						Text DisconnectedLabel = textBefore.GetComponent<Text>();
						DisconnectedLabel.text = "Ping: "+ getInt(incomingData,1);
					}
					SendMessage(new byte[] { 4 });
				}
				else if (incomingData[0] == 10)
				{
					var dumbMissle = Instantiate(exampleMissle, new Vector3(0, 0, 0), Quaternion.identity);
					dumbMissle.transform.position = new Vector3(getFloat(incomingData, 2), getFloat(incomingData, 6), 0);
					dumbMissle.transform.eulerAngles = new Vector3(0, 0, getFloat(incomingData, 10));
					dumbMissles.Add(incomingData[1], dumbMissle);
				}
				/*else if (incomingData[0] == 11) no need for movement streaming
				{
					var dumbMissle = dumbMissles[incomingData[1]];
					dumbMissle.transform.position = new Vector3(getFloat(incomingData, 2), getFloat(incomingData, 6), 0);
					dumbMissle.transform.eulerAngles = new Vector3(0, 0, getFloat(incomingData, 10));
				}*/
				else if (incomingData[0] == 11)
				{
					GameObject dumbMissle = dumbMissles[incomingData[1]];
					Debug.Log("Destroying Missle" + dumbMissle);
					Destroy(dumbMissle);
					dumbMissles.Remove(incomingData[1]);
				}
			}
			foreach (KeyValuePair<byte, GameObject> entry in dumbMissles)
			{
				GameObject playerShip = GameObject.FindGameObjectsWithTag("PlayerShip")[0];
				entry.Value.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, getOpacity(playerShip, entry.Value));
			}
		}
		
		if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastPingTime > 2000)
		{
			GameObject textBefore = GameObject.FindWithTag("DisconnectedLabel");
			if (textBefore != null)
			{
				Text DisconnectedLabel = textBefore.GetComponent<Text>();
				DisconnectedLabel.text = "High ping!";
			}
		}
	}
	float getOpacity(GameObject playerShip, GameObject obj)
    {
		float dist = Vector3.Distance(playerShip.transform.position, obj.transform.position);
		if(dist>2)
        {
			dist = 2;
        }
		return 1-(dist/2);
	}
	bool stopThread = false;
	void OnApplicationQuit()
	{
		Debug.Log("Closing Socket");
		SendMessage(new byte[] { 3 });
		socketConnection.GetStream().Flush();
		stopThread = true;
		clientReceiveThread.Join();
		socketConnection.Close();
		Debug.Log("Closed");
		if (!Application.isEditor) { System.Diagnostics.Process.GetCurrentProcess().Kill(); }
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
	int getInt(byte[] theArray, int start)
	{
		int f = System.BitConverter.ToInt32(theArray, start);
		return f;
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
			if (defaultGame.Equals("AAAA"))
            {
				ConnectToGameAL();
			}
			Debug.Log("Connected:" + socketConnection.Connected);
			NetworkStream stream = socketConnection.GetStream();
			while (!stopThread && stream.CanRead)
			{
				Byte[] bytes = new Byte[1024];
				// Get a stream object for reading 			
				int numRead;
				// Read incoming stream into byte arrary. 
				while (stream.DataAvailable)
				{
					numRead = stream.Read(bytes, 0, bytes.Length);
					lock (threadLocker)
					{
						var multiMessageData = new byte[numRead];
						Array.Copy(bytes, 0, multiMessageData, 0, numRead);
						int index = 0;
						while (index != multiMessageData.Length)
						{
							int messageLength = TYPE_TO_LENGTH[multiMessageData[index]];
							byte[] messageData = new byte[messageLength];
							Array.Copy(multiMessageData, index, messageData, 0, messageLength);
							procData.Enqueue(messageData);
							//Debug.Log(messageData.Length);
							index += messageLength;
						}
					}
				}
				Thread.Sleep(1);
			}
			Debug.Log("Thread Stopped");
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
	public void SendExplosion(Vector3 position)
	{
		byte[] xPos = BitConverter.GetBytes(position.x);
		byte[] yPos = BitConverter.GetBytes(position.y);
		byte[] outBytes = new byte[9];
		outBytes[0] = 5;
		Buffer.BlockCopy(xPos, 0, outBytes, 1, 4);
		Buffer.BlockCopy(yPos, 0, outBytes, 5, 4);
		SendMessage(outBytes);
	}
	public void SendDied()
	{
		byte[] outBytes = new byte[1];
		outBytes[0] = 6;
		SendMessage(outBytes);
	}
	public void SendShipExplosion(Vector3 position)
	{
		byte[] xPos = BitConverter.GetBytes(position.x);
		byte[] yPos = BitConverter.GetBytes(position.y);
		byte[] outBytes = new byte[9];
		outBytes[0] = 7;
		Buffer.BlockCopy(xPos, 0, outBytes, 1, 4);
		Buffer.BlockCopy(yPos, 0, outBytes, 5, 4);
		SendMessage(outBytes);
	}
	public void SendPing(Vector3 position)
	{
		System.Random rnd = new System.Random();
		if (rnd.Next(100) > 101)//percent chance to fail
		{
			return;
		}
		//Debug.Log("Send Ping");
		byte[] xPos = BitConverter.GetBytes(position.x);
		byte[] yPos = BitConverter.GetBytes(position.y);
		byte[] outBytes = new byte[9];
		outBytes[0] = 9;
		Buffer.BlockCopy(xPos, 0, outBytes, 1, 4);
		Buffer.BlockCopy(yPos, 0, outBytes, 5, 4);
		SendMessage(outBytes);
	}
	public void CreateTorp(byte num, float x, float y, float rotA)
	{
		//Debug.Log("Created: " + num);
		byte[] xPos = BitConverter.GetBytes(x);
		byte[] yPos = BitConverter.GetBytes(y);
		byte[] rot = BitConverter.GetBytes(rotA);
		byte[] outBytes = new byte[14];
		outBytes[0] = 10;
		outBytes[1] = num;
		Buffer.BlockCopy(xPos, 0, outBytes, 2, 4);
		Buffer.BlockCopy(yPos, 0, outBytes, 6, 4);
		Buffer.BlockCopy(rot, 0, outBytes, 10, 4);
		SendMessage(outBytes);
	}
	public void DestroyTorp(byte num)
	{
		//Debug.Log("Deleted: " + num);
		byte[] outBytes = new byte[2];
		outBytes[0] = 11;
		outBytes[1] = num;
		SendMessage(outBytes);
	}
}