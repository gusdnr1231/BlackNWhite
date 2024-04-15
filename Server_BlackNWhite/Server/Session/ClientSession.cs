using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server.Session
{

    // 유저네임

    class ClientSession : PacketSession // 게임컨텐츠 영역
    {                           // 데이터의 송수신 구현보다, 송수신시의 동작 작성
        public int SessionId { get; set; }
        public GameRoom Room { get; set; }

        public int StonePosition { get; set; }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected bytes: {endPoint}");

            // 채팅 게임룸 생성
            Program.Room.Enter(this);
        }
        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisConnected bytes: {endPoint}");
            SessionManager.instance.Remove(this);
            if(Room!= null)
            {
                Room.Leave(this);
                Room = null;
            }
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numOfBytes)             // 샌드 이벤트 발생시 동작
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
}
