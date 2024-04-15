using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient.Session
{

    class ServerSession : PacketSession // 게임컨텐츠 영역
    {                           // 데이터의 송수신 구현보다, 송수신시의 동작 작성
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected bytes: {endPoint}");

        }
        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisConnected bytes: {endPoint}");
        }
        public override void OnRecvPacket(ArraySegment<byte> buffer)  // 리시브 이벤트 발생시 동작
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);  // 패킷매니저 시작(해석기 가동)
        }
        public override void OnSend(int numOfBytes)             // 샌드 이벤트 발생시 동작
        {
            //Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
}
