using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sync.Theater.Tests
{
    [TestClass]
    public class SyncTests
    {
        [TestMethod]
        public void SyncTheater_CreateRoomNoCodeSuccessful()
        {
            SyncTheater.Start();

            var sr = SyncTheater.CreateRoom();

            var fsr = SyncTheater.GetRoomByCode(sr.RoomCode);

            Assert.AreEqual(sr, fsr);

            SyncTheater.Stop();
        }

        [TestMethod]
        public void SyncTheater_CreateRoomWithCodeSuccessful()
        {
            SyncTheater.Start();

            var sr = SyncTheater.CreateRoom("ABC123");
            var fsr = SyncTheater.GetRoomByCode(sr.RoomCode);

            Assert.AreEqual(sr, fsr);

            SyncTheater.Stop();
        }

        [TestMethod]
        public void SyncRoom_AddServiceAndGetByNickname()
        {
            SyncTheater.Start();

            var sr = SyncTheater.CreateRoom();
            var ss = new SyncService(sr);
            var testss = sr.GetServiceByNickname(ss.Nickname);

            Assert.AreEqual(ss.Nickname, testss.Nickname);

            SyncTheater.Stop();
        }
    }
}
