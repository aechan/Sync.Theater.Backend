using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sync.Theater.Events;
using Sync.Theater.Models;
using Sync.Theater.Utils;
using Sync.Theater;

namespace Sync.Theater.Tests
{
    [TestClass]
    public class UtilsTests
    {
        public UtilsTests()
        {
            ConfigManager.loadLevel = SyncTheater_ConfigLoadLevel.CONSOLE;
        }

        [TestMethod]
        public void ConfigManager_IsConfigNull()
        {
            Assert.IsNotNull(ConfigManager.Config);
        }

        [TestMethod]
        public void ConfigManager_WasConfigLoadedProperly()
        {
            Assert.AreEqual(ConfigManager.Config.JWTSecret, "hello world");
        }

        [TestMethod]
        public void GfycatNameGenerator_ReturnsUppercasedString()
        {
            string test = "hello world";

            Assert.AreEqual("Hello world",GfycatNameGenerator.FirstLetterToUpper(test));
        }

        [TestMethod]
        public void GfycatNameGenerator_ReturnsThreeWordName()
        {
            string name = GfycatNameGenerator.GetName();

            int count = 0;
            for (int i = 0; i < name.Length; i++)
            {
                if (char.IsUpper(name[i])) count++;
            }

            Assert.AreEqual(3, count);
        }

        [TestMethod]
        public void SyncLogger_FormatsLogProperly()
        {
            SyncLogger log = new SyncLogger("Test");

            string test = log.TestLog("this is a {0}", "test");

            Assert.AreEqual(DateTime.Now.ToString() + " [Test] this is a test", test);
        }
    }
}
