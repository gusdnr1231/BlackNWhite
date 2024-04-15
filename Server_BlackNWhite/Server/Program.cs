using Server.Session;
using ServerCore;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    class Program
    {
        static Listener _listener = new Listener();
        public static GameRoom Room = new GameRoom();

        private const int m_port = 50765;

        static void Main(string[] args) 
        {

            // DNS
            IPHostEntry iphost = Dns.GetHostEntry(Dns.GetHostName());
            Console.WriteLine(iphost.AddressList[1]);
            IPAddress ipAddr = iphost.AddressList[1];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, m_port); // IP주소, 포트번호 입력

            _listener.init(endPoint, () => { return SessionManager.instance.Generate(); });  // GameSession 새로 만들어 매개변수 대입
            Console.WriteLine("Listening...(영업중이야)");                   // 세부 구현은 OnAcceptCompleted에서 상세구현

            while (true)
            {
                //프로그램 종료 막기 위해 while
            }
        }
    }

   
}
