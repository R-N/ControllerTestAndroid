using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.UI;
using System.Collections.Generic;
using PDollarGestureRecognizer;

public class ControllerSender : MonoBehaviour {
	public string receiverIP = "127.0.0.1";
	public static bool connected = false;
	bool hasGyro = false;
	bool hasAccel = false;
	bool hasVib = false;
	bool hasCompass = false;
	int receiverPort = 2581;
	public static NetworkClient cli = null;
	public GameObject ActualUI = null;
	public GameObject FaceUI = null;
	public InputField ipField = null;




	public class NetMsgId {
		public const short hasGyro = 100;
		public const short hasAccel = 101;
		public const short hasVib = 102;
		public const short inputFloat = 103;
		public const short inputVector2 = 104;
		public const short inputVector3 = 105;
		public const short inputQuaternion = 106;
		public const short inputGesture = 107;
		public const short inputBool = 108;
		public const short vibrate = 109;
		public const short inputButton = 110;
		public const short inputVector2Array = 111;
        public const short hasCompass = 112;
    }

	// Use this for initialization
	void Start () {
        cli = new NetworkClient ();
        ConnectionConfig config = new ConnectionConfig ();
        config.FragmentSize = 8;
        config.PacketSize = 256;
        config.MaxSentMessageQueueSize = 4;
        config.MaxCombinedReliableMessageSize = 4;
        config.WebSocketReceiveBufferMaxSize = 4;
        config.AddChannel (QosType.ReliableSequenced);
        config.AddChannel (QosType.Unreliable);
        cli.Configure (config, 1);
        cli.RegisterHandler(NetMsgId.vibrate, (msg) => SetIP());
	}

	public void SetIP(){
		receiverIP = ipField.text;
	}

	public void Connect(){
		StopCoroutine ("WaitForConnection");
		if (cli.isConnected) {
			cli.Disconnect ();
		}
		connected = false;
		SetIP ();
		cli.Connect (receiverIP, receiverPort);
		StartCoroutine ("WaitForConnection");
	}

	public IEnumerator WaitForConnection(){
		
		while (!cli.isConnected) {
			yield return new WaitForEndOfFrame ();
		}
		connected = cli.isConnected;
		if (connected) {
			hasGyro = SystemInfo.supportsGyroscope;
			Input.gyro.enabled = hasGyro;
			hasGyro = hasGyro && Input.gyro.enabled;

			hasAccel = SystemInfo.supportsAccelerometer;

			hasVib = SystemInfo.supportsVibration;

			Input.compass.enabled = true;
			hasCompass = Input.compass.enabled;
            SendReliable(NetMsgId.hasGyro, hasGyro);
            SendReliable(NetMsgId.hasAccel, hasAccel);
            SendReliable(NetMsgId.hasVib, hasVib);
            SendReliable(NetMsgId.hasCompass, hasCompass);
			StartCoroutine ("CheckConnection");
			ActualUI.SetActive (connected);
			FaceUI.SetActive (!connected);
		} else {
			Connect ();
		}
	}

	public IEnumerator CheckConnection(){
		connected = cli.isConnected;
		while (cli.isConnected) {
			yield return new WaitForEndOfFrame ();
		}
		if (!cli.isConnected) {
			connected = false;
			ActualUI.SetActive (connected);
			FaceUI.SetActive (!connected);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (connected) {
            NetworkWriter writer;
			if (hasGyro) {
				SendInput ("GyroAttitudeRaw", Input.gyro.attitude);

				SendInput ("GyroAccel", Input.gyro.userAcceleration);
				SendInput ("GyroRotationRate", Input.gyro.rotationRate);
				SendInput ("GyroGravity", Input.gyro.gravity);
			}
			if (hasAccel)
				SendInput ("Accel", Input.acceleration);

			SendInput("Joystick", Joystick.padAxis);
		}
	}

	public void SendInput(string name, Point[] points){
		if (connected) {
			NetworkWriter writer = new NetworkWriter ();
			writer.StartMessage (NetMsgId.inputGesture);
			writer.Write (name);
			int length = points.Length;
			writer.Write (length);
			for (int a = 0; a < length; a++) {
                writer.Write (new Vector2(points [a].X, points [a].Y));
				writer.Write (points [a].StrokeID);
            }
            writer.FinishMessage();
			cli.SendWriter (writer, 0);
		}
	}
	public void SendInput(string name, Vector2[] points){
		if (connected) {
			NetworkWriter writer = new NetworkWriter ();
			writer.StartMessage (NetMsgId.inputGesture);
			writer.Write (name);
			int length = points.Length;
			writer.Write (length);
			for (int a = 0; a < length; a++) {
				writer.Write (points [a]);
            }
            writer.FinishMessage();
			cli.SendWriter (writer, 0);
		}
	}
	public void SendInput(string name, bool value){
		NetworkWriter writer = new NetworkWriter();
		writer.StartMessage(NetMsgId.inputBool);
		writer.Write (name);
		writer.Write(value);
		writer.FinishMessage();
		cli.SendWriter(writer, 0);
	}
	public void SendInput(string name, float value){
		NetworkWriter writer = new NetworkWriter();
		writer.StartMessage(NetMsgId.inputFloat);
		writer.Write (name);
        writer.Write ((double)value);
        writer.FinishMessage();
		cli.SendWriter(writer, 0);
	}
	public void SendInput(string name, Vector2 value){
		NetworkWriter writer = new NetworkWriter();
		writer.StartMessage(NetMsgId.inputVector2);
		writer.Write (name);
        writer.Write (value);
        writer.FinishMessage();
		cli.SendWriter(writer, 0);
	}
	public void SendInput(string name, Vector3 value){
		NetworkWriter writer = new NetworkWriter();
		writer.StartMessage(NetMsgId.inputVector3);
		writer.Write (name);
        writer.Write (value);
        writer.FinishMessage();
		cli.SendWriter(writer, 0);
	}
	public void SendInput(string name, Quaternion value){
		NetworkWriter writer = new NetworkWriter();
		writer.StartMessage(NetMsgId.inputQuaternion);
		writer.Write (name);
        writer.Write (value);
        writer.FinishMessage();
		cli.SendWriter(writer, 0);
	}
    
    public void SendReliable(short id, bool value){
        NetworkWriter writer = new NetworkWriter();
        writer.StartMessage(id);
        writer.Write(value);
        writer.FinishMessage();
        cli.SendWriter(writer, 0);
    }
    public void SendReliable(short id, Quaternion value){
        NetworkWriter writer = new NetworkWriter();
        writer.StartMessage(id);
        writer.Write(value);
        writer.FinishMessage();
        cli.SendWriter(writer, 0);
    }
    public void SendReliable(short id, Vector3 value){
        NetworkWriter writer = new NetworkWriter();
        writer.StartMessage(id);
        writer.Write(value);
        writer.FinishMessage();
        cli.SendWriter(writer, 0);
    }
    public void SendReliable(short id, Vector2 value){
        NetworkWriter writer = new NetworkWriter();
        writer.StartMessage(id);
        writer.Write(value);
        writer.FinishMessage();
        cli.SendWriter(writer, 0);
    }
    public void SendReliable(short id, float value){
        NetworkWriter writer = new NetworkWriter();
        writer.StartMessage(id);
        writer.Write(value);
        writer.FinishMessage();
        cli.SendWriter(writer, 0);
    }
}
