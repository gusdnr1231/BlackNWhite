using DummyClient;
using DummyClient.Session;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
	// 서버 플래그.	
	public bool m_isServer = false;
	// 접속 플래그.
	private bool m_isConnected = false;

	// 이벤트 알림 델리게이트.
	public delegate void EventHandler(NetEventState state);
	private EventHandler m_handler;

	// 이벤트 함수 등록.
	public void RegisterEventHandler(EventHandler handler)
	{
		m_handler += handler;
	}

	// 이벤트 통지 함수 삭제.
	public void UnregisterEventHandler(EventHandler handler)
	{
		m_handler -= handler;
	}

	// 세션 1개만 사용 예정이므로, SessionManager 미사용
	ServerSession _session = new ServerSession();


	// 컨텐츠단 데이터 전송!
	public void Send(ArraySegment<byte> sendBuff)
	{
		_session.Send(sendBuff);
		Debug.Log("서버로 송신!");
	}

	public bool ConnectToServer(IPAddress ipAddr, int port)
	{
		try
		{
			Debug.Log("연결 시작.");
			IPEndPoint endPoint = new IPEndPoint(ipAddr, port); // IP주소, 포트번호 입력
			Connector connector = new Connector();
			connector.Connect(endPoint, () => { return _session; }, 1);
			Debug.Log("Connection success.");
			m_isConnected = true;
		}
		catch (Exception e)
		{
			Debug.Log("Connect fail");
			return false;
		}

		if (m_handler != null)
		{
			// 접속 결과를 알립니다.
			NetEventState state = new NetEventState();
			state.type = NetEventType.Connect;
			state.result = (m_isConnected == true) ? NetEventResult.Success : NetEventResult.Failure;
			m_handler(state);
			Debug.Log("event handler called");

		}

		return true;
	}


	// 서버인지 확인.
	public bool IsServer()
	{
		return m_isServer;
	}

	// 접속 확인.
	public bool IsConnected()
	{
		return m_isConnected;
	}




	void Start()
	{

	}
	void Update()
	{
		List<IPacket> list = PacketQueue.Instance.PopAll();
		foreach (IPacket packet in list)
			PacketManager.Instance.HandlePacket(_session, packet);
	}
}
