using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient {

    public enum PacketID
    {
        PlayerInfoReq = 1,
        Test = 2,
    }

    interface IPacket
    {
        ushort Protocol { get; }
        void Read(ArraySegment<byte> segment);
        ArraySegment<byte> Write();
    }

    public class PlayerInfoReq : IPacket
    {
        public long playerId;
        public string name;

        public ushort Protocol { get { return (ushort)PacketID.PlayerInfoReq; } }

        public ArraySegment<byte> Write()
        {
            // 직렬화
            ArraySegment<byte> segment = SendBufferHelper.Open(4096);
            ushort count = 0;
            bool success = true;

            Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

            // 혹시, 복사 실패할 경우 false 반환하여 예외처리, 주로 데이터 사이즈 문제로 false 반환.

            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.PlayerInfoReq);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
            count += sizeof(long);

            //string
            ushort nameLen = (ushort)Encoding.Unicode.GetBytes(
                this.name, 0, this.name.Length, segment.Array, segment.Offset + count + sizeof(ushort));   // UTF16으로 name의 길이 반환
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
            count += sizeof(ushort);
            count += nameLen;

            success &= BitConverter.TryWriteBytes(s, count);
            if (success == false)   // 예외처리
                return null;

            return SendBufferHelper.Close(count);
        }

        public void Read(ArraySegment<byte> segment)
        {
            ushort count = 0;

            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);  // 파싱

            count += sizeof(ushort);
            count += sizeof(ushort);
            this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));  // PlayerID 파싱
            count += sizeof(long); // 데이터 크기 누적

            // string 파싱
            ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);
            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
            
        }
    }
}
