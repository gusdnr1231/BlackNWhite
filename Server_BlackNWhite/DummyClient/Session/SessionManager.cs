using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient.Session
{
    class SessionManager
    {
        static SessionManager _session = new SessionManager();
        public static SessionManager instance { get { return _session; } }

        List<ServerSession> _sessions = new List<ServerSession>();
        object _lock = new object();

        public ServerSession Generate()
        {
            lock (_lock)
            {
                ServerSession session = new ServerSession();
                _sessions.Add(session);
                return session;
            }
        }

        public void SendForEach()
        {
            lock (_lock)
            {
                foreach (ServerSession session in _sessions)
                {
                    PlayerInfoReq packet = new PlayerInfoReq();
                    packet.playerId = 22;
                    packet.name = "KING";
                    ArraySegment<byte> segment = packet.Write();

                    session.Send(segment);
                }
            }
        }
    }
}
