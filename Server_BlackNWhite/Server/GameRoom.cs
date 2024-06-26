﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Session;

namespace Server
{
    class GameRoom
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        object _lock = new object();

        public void SetCardData(ClientSession session, C_SetCard packet)
        {
            lock (_lock)
            {
                // 돌 받고
                session.SetCardNumber = packet.selectNum;
                session.SetCardColor = packet.selectCol;

                foreach(ClientSession s in _sessions)
                {
                    if(s.SessionId == packet.destinationId)
                    {
                        Console.WriteLine($"Find Destination Session : ID {packet.destinationId}");

						S_SetOtherCard SSet = new S_SetOtherCard();
						SSet.selectNum = packet.selectNum;
						SSet.selectCol = packet.selectCol;
                        s.Send(SSet.Write());
					}
                }
            }
        }


        public void Enter(ClientSession session)
        {
            lock(_lock)
            {   // 신규 유저 추가
                _sessions.Add(session);
                session.Room = this;

                // 신규 유저 접속시, 기존 유저 목록 전송
                S_PlayerList players = new S_PlayerList();
                foreach (ClientSession s in _sessions)
                {
                    players.players.Add(new S_PlayerList.Player() {
                        isSelf = (s == session),
                        playerId = s.SessionId,
                    });
                }
                session.Send(players.Write());

                // 신규 유저 접속 전체 공지
                S_BroadcastEnterGame enter = new S_BroadcastEnterGame();
                enter.playerId = session.SessionId;
                BroadCast(enter.Write());
            }

        }

        public void Leave(ClientSession session)
        {
            lock (_lock)
            {
                // 플레이어 제거하고
                _sessions.Remove(session);

                // 모두에게 알린다
                S_BroadcastLeaveGame leave = new S_BroadcastLeaveGame();
                leave.playerId = session.SessionId;
                BroadCast(leave.Write());
            }
        }

        public void BroadCast(ArraySegment<byte> segment) 
        {
            ArraySegment<byte> packet = segment;

            lock (_lock) // 
            {
                foreach(ClientSession s in _sessions)
                {
                    s.Send(segment);    // 리스트에 들어있는 모든 클라에 전송
                }
            }
        }

    }
}
