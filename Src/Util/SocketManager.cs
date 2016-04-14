using System.Collections.Generic;
using System;
using System.Threading;
using WebSocketSharp.Net;
using WebSocketSharp;
using SocketIO;
using UnityEngine;

public class SocketManager {
	public string url = "ws://127.0.0.1:6060/socket.io/?EIO=4&transport=websocket";
	public int reconnectDelay = 5;
	public float ackExpirationTime = 1800f;
	public float pingInterval = 25f;
	public float pingTimeout = 60f;

	public WebSocket socket { get { return ws; } }
	public string sid { get; set; }
	public bool IsConnected { get { return connected; } }

	private volatile bool connected;
	private volatile bool thPinging;
	private volatile bool thPong;
	private volatile bool wsConnected;

	private Thread socketThread;
	private Thread pingThread;
	private WebSocket ws;

	private Encoder encoder;
	private Decoder decoder;
	private Parser parser;

	private Dictionary<string, List<Action<SocketIOEvent>>> handlers;
	private List<Ack> ackList;

	private int packetId;

	private object eventQueueLock;
	private Queue<SocketIOEvent> eventQueue;

	private object ackQueueLock;
	private Queue<Packet> ackQueue;

	public SocketManager() {
		encoder = new Encoder();
		decoder = new Decoder();
		parser = new Parser();
		handlers = new Dictionary<string, List<Action<SocketIOEvent>>>();
		ackList = new List<Ack>();
		sid = null;
		packetId = 0;

		ws = new WebSocket(url);
		ws.OnOpen += OnOpen;
		ws.OnMessage += OnMessage;
		ws.OnError += OnError;
		ws.OnClose += OnClose;
		wsConnected = false;

		eventQueueLock = new object();
		eventQueue = new Queue<SocketIOEvent>();

		ackQueueLock = new object();
		ackQueue = new Queue<Packet>();

		connected = false;
	}

	public void Update() {
		lock(eventQueueLock){ 
			while(eventQueue.Count > 0){
				EmitEvent(eventQueue.Dequeue());
			}
		}

		lock(ackQueueLock){
			while(ackQueue.Count > 0){
				InvokeAck(ackQueue.Dequeue());
			}
		}

		if(wsConnected != ws.IsConnected){
			wsConnected = ws.IsConnected;
			if(wsConnected){
				EmitEvent("connect");
			} else {
				EmitEvent("disconnect");
			}
		}

		// GC expired acks
		if(ackList.Count == 0) { return; }
		if(DateTime.Now.Subtract(ackList[0].time).TotalSeconds < ackExpirationTime){ return; }
		ackList.RemoveAt(0);
	}

	public void Disconnect() {
		EmitClose();
		if (socketThread != null) 	{ socketThread.Abort(); }
		if (pingThread != null) 	{ pingThread.Abort(); }
		connected = false;
	}

	public void Connect() {
		connected = true;

		socketThread = new Thread(RunSocketThread);
		socketThread.IsBackground = true;
		socketThread.Start(ws);

		pingThread = new Thread(RunPingThread);
		pingThread.IsBackground = true;
		pingThread.Start(ws);
	}

	public void On(string ev, Action<SocketIOEvent> callback) {
		if (!handlers.ContainsKey(ev)) {
			handlers[ev] = new List<Action<SocketIOEvent>>();
		}
		handlers[ev].Add(callback);
	}

	public void Off(string ev, Action<SocketIOEvent> callback) {
		if (!handlers.ContainsKey(ev)) {
			#if SOCKET_IO_DEBUG
			debugMethod.Invoke("[SocketIO] No callbacks registered for event: " + ev);
			#endif
			return;
		}

		List<Action<SocketIOEvent>> l = handlers [ev];
		if (!l.Contains(callback)) {
			#if SOCKET_IO_DEBUG
			debugMethod.Invoke("[SocketIO] Couldn't remove callback action for event: " + ev);
			#endif
			return;
		}

		l.Remove(callback);
		if (l.Count == 0) {
			handlers.Remove(ev);
		}
	}

	public void Emit(string ev)
	{
		EmitMessage(-1, string.Format("[\"{0}\"]", ev));
	}

	public void Emit(string ev, Action<JSONObject> action)
	{
		EmitMessage(++packetId, string.Format("[\"{0}\"]", ev));
		ackList.Add(new Ack(packetId, action));
	}

	public void Emit(string ev, JSONObject data)
	{
		EmitMessage(-1, string.Format("[\"{0}\",{1}]", ev, data));
	}

	public void Emit(string ev, JSONObject data, Action<JSONObject> action)
	{
		EmitMessage(++packetId, string.Format("[\"{0}\",{1}]", ev, data));
		ackList.Add(new Ack(packetId, action));
	}

	#region Private Methods

	private void RunSocketThread(object obj)
	{
		WebSocket webSocket = (WebSocket)obj;
		while(connected){
			if(webSocket.IsConnected){
				Thread.Sleep(reconnectDelay);
			} else {
				webSocket.Connect();
			}
		}
		webSocket.Close();
	}

	private void RunPingThread(object obj)
	{
		WebSocket webSocket = (WebSocket)obj;

		int timeoutMilis = Mathf.FloorToInt(pingTimeout * 1000);
		int intervalMilis = Mathf.FloorToInt(pingInterval * 1000);

		DateTime pingStart;

		while(connected)
		{
			if(!wsConnected){
				Thread.Sleep(reconnectDelay);
			} else {
				thPinging = true;
				thPong =  false;

				EmitPacket(new Packet(EnginePacketType.PING));
				pingStart = DateTime.Now;

				while(webSocket.IsConnected && thPinging && (DateTime.Now.Subtract(pingStart).TotalSeconds < timeoutMilis)){
					Thread.Sleep(200);
				}

				if(!thPong){
					webSocket.Close();
				}

				Thread.Sleep(intervalMilis);
			}
		}
	}

	private void EmitMessage(int id, string raw)
	{
		EmitPacket(new Packet(EnginePacketType.MESSAGE, SocketPacketType.EVENT, 0, "/", id, new JSONObject(raw)));
	}

	private void EmitClose()
	{
		EmitPacket(new Packet(EnginePacketType.MESSAGE, SocketPacketType.DISCONNECT, 0, "/", -1, new JSONObject("")));
		EmitPacket(new Packet(EnginePacketType.CLOSE));
	}

	private void EmitPacket(Packet packet)
	{
		#if SOCKET_IO_DEBUG
		debugMethod.Invoke("[SocketIO] " + packet);
		#endif

		try {
			ws.Send(encoder.Encode(packet));
		} catch(SocketIOException ex) {
			#if SOCKET_IO_DEBUG
			debugMethod.Invoke(ex.ToString());
			#endif
		}
	}

	private void OnOpen(object sender, EventArgs e)
	{
		EmitEvent("open");
	}

	private void OnMessage(object sender, MessageEventArgs e)
	{
		#if SOCKET_IO_DEBUG
		debugMethod.Invoke("[SocketIO] Raw message: " + e.Data);
		#endif
		Packet packet = decoder.Decode(e);

		switch (packet.enginePacketType) {
			case EnginePacketType.OPEN: 	HandleOpen(packet);		break;
			case EnginePacketType.CLOSE: 	EmitEvent("close");		break;
			case EnginePacketType.PING:		HandlePing();	   		break;
			case EnginePacketType.PONG:		HandlePong();	   		break;
			case EnginePacketType.MESSAGE: 	HandleMessage(packet);	break;
		}
	}

	private void HandleOpen(Packet packet)
	{
		#if SOCKET_IO_DEBUG
		debugMethod.Invoke("[SocketIO] Socket.IO sid: " + packet.json["sid"].str);
		#endif
		sid = packet.json["sid"].str;
		EmitEvent("open");
	}

	private void HandlePing()
	{
		EmitPacket(new Packet(EnginePacketType.PONG));
	}

	private void HandlePong()
	{
		thPong = true;
		thPinging = false;
	}

	private void HandleMessage(Packet packet)
	{
		if(packet.json == null) { return; }

		if(packet.socketPacketType == SocketPacketType.ACK){
			for(int i = 0; i < ackList.Count; i++){
				if(ackList[i].packetId != packet.id){ continue; }
				lock(ackQueueLock){ ackQueue.Enqueue(packet); }
				return;
			}

			#if SOCKET_IO_DEBUG
			debugMethod.Invoke("[SocketIO] Ack received for invalid Action: " + packet.id);
			#endif
		}

		if (packet.socketPacketType == SocketPacketType.EVENT) {
			SocketIOEvent e = parser.Parse(packet.json);
			lock(eventQueueLock){ eventQueue.Enqueue(e); }
		}
	}

	private void OnError(object sender, ErrorEventArgs e)
	{
		EmitEvent("error");
	}

	private void OnClose(object sender, CloseEventArgs e)
	{
		EmitEvent("close");
	}

	private void EmitEvent(string type)
	{
		EmitEvent(new SocketIOEvent(type));
	}

	private void EmitEvent(SocketIOEvent ev)
	{
		if (!handlers.ContainsKey(ev.name)) { return; }
		foreach (Action<SocketIOEvent> handler in this.handlers[ev.name]) {
			try{
				handler(ev);
			} catch(Exception ex){
				#if SOCKET_IO_DEBUG
				debugMethod.Invoke(ex.ToString());
				#endif
			}
		}
	}

	private void InvokeAck(Packet packet)
	{
		Ack ack;
		for(int i = 0; i < ackList.Count; i++){
			if(ackList[i].packetId != packet.id){ continue; }
			ack = ackList[i];
			ackList.RemoveAt(i);
			ack.Invoke(packet.json);
			return;
		}
	}

	#endregion
}
