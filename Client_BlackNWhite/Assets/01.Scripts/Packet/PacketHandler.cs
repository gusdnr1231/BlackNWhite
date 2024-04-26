using DummyClient.Session;
using ServerCore;
using UnityEngine;

namespace DummyClient
{
    class PacketHandler {
        // 어떤 세션에서 조립된 패킷인지, 어떤 내용의 패킷인지

        // 플레이어 입장 신호 보내기
        public static void S_BroadcastEnterGameHandler(PacketSession session, IPacket packet)
        {
            S_BroadcastEnterGame pkt = packet as S_BroadcastEnterGame;
            ServerSession serverSession = session as ServerSession;

        }

        // 플레이어 퇴장 신호 보내기
        public static void S_BroadcastLeaveGameHandler(PacketSession session, IPacket packet)
        {
            S_BroadcastLeaveGame pkt = packet as S_BroadcastLeaveGame;
            ServerSession serverSession = session as ServerSession;

        }

        // 현재 플레이어 리스트
        public static void S_PlayerListHandler(PacketSession session, IPacket packet)
        {
            S_PlayerList pkt = packet as S_PlayerList;
            ServerSession serverSession = session as ServerSession;

        }

		public static void S_SetOtherCardHandler(PacketSession session, IPacket packet)
		{
			S_SetOtherCard pkt = packet as S_SetOtherCard;
			ServerSession serverSession = session as ServerSession;

            PlayerManager.Instance.SetCard(pkt);
		}

		public static void S_BroadCastRoundHandler(PacketSession session, IPacket packet)
        {
            S_BroadCastRound pkt = packet as S_BroadCastRound;
            ServerSession serverSession = session as ServerSession;

            
        }

	}
}


