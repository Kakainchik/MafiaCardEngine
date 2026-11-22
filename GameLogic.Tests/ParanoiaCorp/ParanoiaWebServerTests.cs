using GameLogic;
using GameLogic.ParanoiaCorp;
using GameLogic.ParanoiaCorp.Roles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebServer.Model.Game;
using WebServer.Model.ParanoiaCorp;
using WebServer.Shared.GameObjects;
using ParanoiaRoleSignature = WebServer.Shared.ParanoiaCorp.GameObjects.RoleSignature;

namespace GameLogic.Tests.ParanoiaCorp
{
    [TestClass]
    public class ParanoiaWebServerTests
    {
        [TestMethod]
        public void RoleCatalogue_ContainsAll21Roles_WithValidMetadata()
        {
            var roles = ParanoiaRoleCatalogue.GetAllRoles();
            Assert.AreEqual(21, roles.Count);

            foreach (var r in roles)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(r.RoleKey), $"RoleKey is empty for {r.NameEn}");
                Assert.IsFalse(string.IsNullOrWhiteSpace(r.NameEn), $"NameEn is empty for {r.RoleKey}");
                Assert.IsFalse(string.IsNullOrWhiteSpace(r.NameRu), $"NameRu is empty for {r.RoleKey}");
                Assert.IsFalse(string.IsNullOrWhiteSpace(r.TeamName), $"TeamName is empty for {r.RoleKey}");
                Assert.IsFalse(string.IsNullOrWhiteSpace(r.FactionColor), $"FactionColor is empty for {r.RoleKey}");
                Assert.IsFalse(string.IsNullOrWhiteSpace(r.DescriptionEn), $"DescriptionEn is empty for {r.RoleKey}");
                Assert.IsFalse(string.IsNullOrWhiteSpace(r.DescriptionRu), $"DescriptionRu is empty for {r.RoleKey}");
            }
        }

        [TestMethod]
        public void RoleCatalogue_FindsRoleByRoleKeyAndSignature()
        {
            var director = ParanoiaRoleCatalogue.GetRoleInfo(ParanoiaRoleSignature.GENERAL_DIRECTOR);
            Assert.IsNotNull(director);
            Assert.AreEqual("General Director", director.NameEn);
            Assert.AreEqual("Генеральный Директор", director.NameRu);

            var hacker = ParanoiaRoleCatalogue.GetRoleInfo("HACKER");
            Assert.IsNotNull(hacker);
            Assert.AreEqual("Hacker", hacker.NameEn);
            Assert.AreEqual("Хакер", hacker.NameRu);
            Assert.AreEqual("Syndicate", hacker.TeamName);
        }

        [TestMethod]
        public async Task ParanoiaGameSession_InitializesAndGeneratesPersonalizedViews()
        {
            var directorPlayer = new PlayerDomain("Director", new RGB(0, 0, 0), new Player(100, new GeneralDirectorRole()));
            var sysadminPlayer = new PlayerDomain("Alice", new RGB(0, 0, 0), new Player(200, new SystemAdministratorRole()));
            var hackerPlayer = new PlayerDomain("Bob", new RGB(0, 0, 0), new Player(300, new HackerRole()));

            var players = new[] { directorPlayer, sysadminPlayer, hackerPlayer };
            var session = new ParanoiaGameSession(42, players, null!);

            Assert.AreEqual(42, session.RoomId);
            Assert.IsNotNull(session.Engine);

            // Director View
            var directorView = session.GetPlayerView(100);
            Assert.AreEqual("Intro", directorView.Phase);
            Assert.AreEqual("General Director", directorView.Self.RoleNameEn);
            Assert.AreEqual("Corporation", directorView.Self.Faction);
            Assert.AreEqual(3, directorView.Roster.Count);

            // Hacker View
            var hackerView = session.GetPlayerView(300);
            Assert.AreEqual("Hacker", hackerView.Self.RoleNameEn);
            Assert.AreEqual("Syndicate", hackerView.Self.Faction);

            // Advance through Intro Ready
            await session.HandleIntroReadyAsync(100);
            await session.HandleIntroReadyAsync(200);
            await session.HandleIntroReadyAsync(300);

            // Engine advances to Overtime
            var overtimeView = session.GetPlayerView(100);
            Assert.AreEqual("Overtime", overtimeView.Phase);

            // Chat verification
            bool canDirectorSendPublic = await session.SendChatMessageAsync(100, "PUBLIC", "Good morning board.");
            // During overtime, public chat is restricted to board meeting or read-only
            Assert.IsFalse(canDirectorSendPublic);

            // Hacker can write in SYNDICATE chat
            bool canHackerSendSyndicate = await session.SendChatMessageAsync(300, "SYNDICATE", "Breach prepared.");
            Assert.IsTrue(canHackerSendSyndicate);

            var syndicateMessages = session.GetChatHistory(300, "SYNDICATE");
            Assert.AreEqual(1, syndicateMessages.Count);
            Assert.AreEqual("Breach prepared.", syndicateMessages[0].Content);
        }
    }
}
