using Server.Session;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server {
    class PacketHandler 
    {
        // 카드 놓는거
        public static void C_SetCardHandler(PacketSession session, IPacket packet)
        {
            C_SetCard movePacket = packet as C_SetCard;
            ClientSession clientSession = session as ClientSession;
            if (clientSession.Room == null)
                return;
            Console.WriteLine($"{movePacket}");
            
            GameRoom room = clientSession.Room;
            room.SetCardData(clientSession, movePacket);
        }

        // 클라가 떠났을 때, room에서 내쫓는 동작
        public static void C_LeaveGameHandler(PacketSession session, IPacket packet)
        {
            ClientSession clientSession = session as ClientSession;

            if (clientSession.Room == null)
                return;

            GameRoom room = clientSession.Room;
            room.Leave(clientSession);
        }
    }
}
