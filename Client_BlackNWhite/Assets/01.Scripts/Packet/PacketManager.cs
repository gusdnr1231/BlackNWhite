using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient {
    class PacketManager 
    {
        #region 싱글톤
        // 패킷매니저는 수정할 일없으므로 싱글톤으로 간편히 유지
        static PacketManager _instance = new PacketManager();
        public static PacketManager Instance { get { return _instance; } }
        #endregion

        PacketManager()
        {
            Register();
        }

        Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>> _makeFunc
            = new Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>>();
        Dictionary<ushort, Action<PacketSession, IPacket>> _handler
            = new Dictionary<ushort, Action<PacketSession, IPacket>>();

        public void Register()
        {   // 멀티스레드 개입 차단 필요
            _makeFunc.Add((ushort)PacketID.S_BroadcastEnterGame, MakePacket<S_BroadcastEnterGame>);
            _handler.Add((ushort)PacketID.S_BroadcastEnterGame, PacketHandler.S_BroadcastEnterGameHandler);
            _makeFunc.Add((ushort)PacketID.S_BroadcastLeaveGame, MakePacket<S_BroadcastLeaveGame>);
            _handler.Add((ushort)PacketID.S_BroadcastLeaveGame, PacketHandler.S_BroadcastLeaveGameHandler);
            _makeFunc.Add((ushort)PacketID.S_PlayerList, MakePacket<S_PlayerList>);
            _handler.Add((ushort)PacketID.S_PlayerList, PacketHandler.S_PlayerListHandler);

            _makeFunc.Add((ushort)PacketID.S_BroadCastStone, MakePacket<S_BroadCastStone>);
            _handler.Add((ushort)PacketID.S_BroadCastStone, PacketHandler.S_BroadCastStoneHandler);
        }

        public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer
            ,Action<PacketSession, IPacket> onRecvCallback = null)  //  Action 콜백 : 입력되는 액션에 따라 Invoke
        {
            ushort count = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;    // id값을 가지고 switch 대신 딕셔너리에서 값을 찾고 등록된 핸들러에서 해당 작업 Invoke

            Func<PacketSession, ArraySegment<byte>, IPacket> func = null;
            if (_makeFunc.TryGetValue(id, out func))
            {
                IPacket packet = func.Invoke(session, buffer);
                if (onRecvCallback != null) // 액션이 실행되면 Invoke 
                    onRecvCallback.Invoke(session, packet);
                else
                    HandlePacket(session, packet);  // 디폴트 실행
            }

        }
        // 패킷 생성 부분
        T MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
        {
            T packet = new T();     // 패킷 만들기
            packet.Read(buffer);    // 들어온 패킷 읽기
            return packet;  
        }
        // 패킷 처리부분 분리
        public void HandlePacket(PacketSession session, IPacket packet)
        {
            Action<PacketSession, IPacket> action = null;
            if (_handler.TryGetValue(packet.Protocol, out action))
            {
                action.Invoke(session, packet);
            }
        }
    }
}
