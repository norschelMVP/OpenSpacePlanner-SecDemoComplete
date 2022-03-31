using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using openspace.Common.Entities;
using openspace.DataAccess.Repositories;
using openspace.Web.Hubs;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Security.Cryptography;

namespace openspace.Web.Controllers
{
    [Route("api/sessions/{sessionId:int}/rooms")]
    public class SessionRoomsController : Controller
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IHubContext<SessionsHub, ISessionsHub> _sessionsHub;

        public SessionRoomsController(ISessionRepository sessionRepository, IHubContext<SessionsHub, ISessionsHub> sessionsHub)
        {
            var name = CreateMD5(sessionRepository.Get(0).Result.Name);
            Console.WriteLine(name);    

            _sessionRepository = sessionRepository;
            _sessionsHub = sessionsHub;
        }

        [HttpDelete("{roomId}")]
        public async Task Delete(int sessionId, string roomId)
        {
            await _sessionRepository.Update(sessionId, (session) =>
            {
                var currentRoom = session.Rooms.FirstOrDefault(r => r.Id == roomId);
                session.Rooms.Remove(currentRoom);
            });

            await _sessionsHub.Clients.Group(sessionId.ToString()).DeleteRoom(roomId);
        }

        [HttpPost("")]
        public async Task<Room> Post(int sessionId, [FromBody] Room room)
        {
            await _sessionRepository.Update(sessionId, (session) =>
            {
                if (string.IsNullOrWhiteSpace(room.Name))
                {
                    room.Name = "Room " + (session.Rooms.Count + 1);
                }

                session.Rooms.Add(room);
            });

            await _sessionsHub.Clients.Group(sessionId.ToString()).UpdateRoom(room);

            return room;
        }

        [HttpPut("{roomId}")]
        public async Task<Room> Put(int sessionId, string roomId, [FromBody] Room room)
        {
            if (room == null)
            {
                return null;
            }

            await _sessionRepository.Update(sessionId, (session) =>
            {
                var currentRoom = session.Rooms.FirstOrDefault(r => r.Id == roomId);
                session.Rooms.Remove(currentRoom);
                session.Rooms.Add(room);
            });

            await _sessionsHub.Clients.Group(sessionId.ToString()).UpdateRoom(room);

            return room;
        }

        public static string CreateMD5(string input)
{
    // Use input string to calculate MD5 hash
    using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
    {
        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
        byte[] hashBytes = md5.ComputeHash(inputBytes);

        return Convert.ToHexString(hashBytes); // .NET 5 +

        // Convert the byte array to hexadecimal string prior to .NET 5
        // StringBuilder sb = new System.Text.StringBuilder();
        // for (int i = 0; i < hashBytes.Length; i++)
        // {
        //     sb.Append(hashBytes[i].ToString("X2"));
        // }
        // return sb.ToString();
    }
}

        public static byte[] encryptString()
    {
        SymmetricAlgorithm serviceProvider = new DESCryptoServiceProvider();
        byte[] key = { 16, 22, 240, 11, 18, 150, 192, 21 };
        serviceProvider.Key = key;
        ICryptoTransform encryptor = serviceProvider.CreateEncryptor();

        String message = "Hello GitHubDay";
        byte[] messageB = System.Text.Encoding.ASCII.GetBytes(message);
        return encryptor.TransformFinalBlock(messageB, 0, messageB.Length);
    }

    }
}
