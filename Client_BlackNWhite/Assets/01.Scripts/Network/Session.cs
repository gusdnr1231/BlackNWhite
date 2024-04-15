using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace ServerCore
{
    public abstract class PacketSession : Session 
    {
        public static readonly int HeaderSize = 2;

        // 상속클래스에서 변경 못하도록 씰(봉인)
        // 패킷구조 : [size(2)][packetId(2)][내용......] 
        public sealed override int OnRecv(ArraySegment<byte> buffer)    
        {
            int processLen = 0;

            while(true)
            {
                // 최소한 헤더(사이즈)는 받을 수 있는지 확인
                if (buffer.Count < HeaderSize)   
                    break;
                // 헤더(size(2))를 읽고, 내용이 몇바이트 짜리 패킷인지 확인 
                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < dataSize)    // 부분적 도착
                    break;
                // 패킷 조립, OnRecvPacket에 단위 패킷만큼 전달 (매개변수 해석 : 버퍼, 시작점, 끝점)
                OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));  
                // 조립 완료되면 패킷 읽기 커서 이동
                processLen += dataSize;
                // 여기까지 오면 패킷 1개, 읽기 성공한 것

                // 현재까지 처리한 패킷의 데이터를 버퍼에서 제거하고 남은 데이터를 새로운 버퍼에 할당
                buffer = new ArraySegment<byte>(
                    buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
            }
            // 현재까지 처리한 누적 데이터 크기 반환
            return processLen;
        }
        // 상속클래스에서 OnRecv를 이것으로 대체하여 사용하도록 유도
        public abstract void OnRecvPacket(ArraySegment<byte> buffer);
    }

    public abstract class Session
    {
        Socket _socket;
        int _disconnected = 0;

        RecvBuffer _recvBuffer = new RecvBuffer(1024);

        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();
        List<ArraySegment<byte>> _pendinglist = new List<ArraySegment<byte>>();
        object _lock = new object();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract void OnDisconnected(EndPoint endPoint);
        public abstract void OnSend(int numOfBytes);
        public abstract int OnRecv(ArraySegment<byte> buffer);


        public void Start(Socket socket)
        {
            _socket = socket;
            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted); // (2-2) 낚시대 들기
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
            // RegisterSend();

            RegisterRecv(); // (1) 수신대기
        }
        #region 데이터 수신
        // 1. 연결대기
        void RegisterRecv()
        {
            _recvBuffer.Clean(); // 커서 뒤로 이동 방지
            ArraySegment<byte> segment = _recvBuffer.WriteSegment;
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            bool pending = _socket.ReceiveAsync(_recvArgs);
            if (pending == false)
                OnRecvCompleted(null, _recvArgs);    // (2-1) 낚시대 들어올리기(데이터수신 발생)
        }
        // 2. 데이터수신
        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {   // 조건1 : 내가 몇바이트를 받았는가? (연결이 끊길경우 0바이트 수신)
            // 조건2 : 연결에 특별한 문제 없는지 체크
            if(args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                // TODO
                try
                {
                    // Write 커서 이동
                    if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    // 컨텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리했는지 받는다
                    int processLen = OnRecv(_recvBuffer.ReadSegment);
                    if (processLen < 0 || _recvBuffer.DataSize < processLen)
                    {
                        Disconnect();
                        return;
                    }
                    // Read 커서 이동
                    if (_recvBuffer.OnRead(processLen) == false)
                    {
                        Disconnect();
                        return;
                    }

                    RegisterRecv(); // (3) 낚시대 다시 던지기(이벤트 재호출)
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnReceiveCompleted Failed! {e}");
                }
            }
            else
            {
                // Disconnect
            }
        }
        #endregion

        #region 데이터송신
        public void Send(ArraySegment<byte> sendBuff) 
        {
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pendinglist.Count == 0)    // 대기중인 것이 없을때
                    RegisterSend();
            }

        }

        void RegisterSend()
        {
            while(_sendQueue.Count > 0) // SendQueue가 빌때까지 반복
            {
                ArraySegment<byte> buff = _sendQueue.Dequeue();
                _pendinglist.Add(buff);
            }
            _sendArgs.BufferList = _pendinglist;

            bool pending = _socket.SendAsync(_sendArgs);
            if(pending == false)    // 바로 보낼 수 있을 때
            {
                OnSendCompleted(null, _sendArgs);   // 이벤트 버퍼 초기화
            }
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock(_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        _sendArgs.BufferList = null;    // 이벤트의 버퍼리스트 깔끔히 정리
                        _pendinglist.Clear();           // 정리

                        OnSend(_sendArgs.BytesTransferred);
                        //TODO
                        if (_sendQueue.Count > 0)   // 혹시 아직 SendQueue에 값이 있다면 전송
                        {
                            RegisterSend();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"OnReceiveCompleted Failed! {ex}");
                    }

                }
                else
                {
                    Disconnect();
                }
            }

        }



        public void Disconnect()
        {
            if(Interlocked.Exchange(ref _disconnected, 1) == 1)
            {
                return;
            }
            OnDisconnected(_socket.RemoteEndPoint);
            // 연결종료
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }
        #endregion
    }
}
