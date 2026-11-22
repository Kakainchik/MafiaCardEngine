using GameLogic.Exceptions;
using GameLogic.ParanoiaCorp;
using GameLogic.ParanoiaCorp.Cycles;
using GameLogic.ParanoiaCorp.Roles;
using GameLogic.ParanoiaCorp.Attributes;
using GameLogic.ParanoiaCorp.Extensions;
using GameLogic.Roles;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameLogic.Tests.ParanoiaCorp
{
    [TestClass]
    public class ParanoiaEngineTests
    {
        [TestMethod]
        public void GameEngine_ThrowsException_WhenNoGeneralDirector()
        {
            var players = new[]
            {
                new Player(1, new SystemAdministratorRole()),
                new Player(2, new AuditorRole())
            };

            var intro = new IntroductoryDayCycle();
            Assert.ThrowsException<InitializingGameException>(() => new GameEngine(players, intro));
        }

        [TestMethod]
        public void GameEngine_ThrowsException_WhenFewerThanMinPlayers()
        {
            var players = new[]
            {
                new Player(1, new GeneralDirectorRole())
            };

            var intro = new IntroductoryDayCycle();
            Assert.ThrowsException<InitializingGameException>(() => new GameEngine(players, intro));
        }

        [TestMethod]
        public void GameEngine_InitializesCorrectly_WithValidPlayers()
        {
            var director = new Player(1, new GeneralDirectorRole());
            var sysadmin = new Player(2, new SystemAdministratorRole());
            var hacker = new Player(3, new HackerRole());

            var players = new[] { director, sysadmin, hacker };
            var intro = new IntroductoryDayCycle();
            var engine = new GameEngine(players, intro);

            Assert.IsNotNull(engine.GeneralDirector);
            Assert.AreEqual(1ul, director.Id);
            Assert.AreEqual(2, engine.AlivePlayers.Count);
            Assert.IsInstanceOfType(engine.CurrentCycle, typeof(IntroductoryDayCycle));
        }

        [TestMethod]
        public void GameEngine_SetsAnticrisisManagerItems_ProportionalToPlayerCount()
        {
            var director = new Player(1, new GeneralDirectorRole());
            var manager = new Player(2, new AnticrisisManagerRole());
            var players = new[]
            {
                director,
                manager,
                new Player(3, new SystemAdministratorRole()),
                new Player(4, new AuditorRole())
            };

            var intro = new IntroductoryDayCycle();
            var engine = new GameEngine(players, intro);

            var managerRole = (AnticrisisManagerRole)manager.Role;
            // 4 players / 4 + 1 = 2 items
            Assert.AreEqual(2, managerRole.Items);
        }

        [TestMethod]
        public void ParanoiaRoles_HaveExpectedTeams()
        {
            var director = new GeneralDirectorRole();
            var sysadmin = new SystemAdministratorRole();
            var hacker = new HackerRole();
            var founder = new StartupFounderRole();
            var outsourcer = new OutsourcerRole();
            var strategist = new PoliticalStrategistRole();
            var raider = new RaiderRole();

            Assert.AreEqual(Team.CORPORATION, director.GetTeam());
            Assert.AreEqual(Team.CORPORATION, sysadmin.GetTeam());
            Assert.AreEqual(Team.SYNDICATE, hacker.GetTeam());
            Assert.AreEqual(Team.STARTUP, founder.GetTeam());
            Assert.AreEqual(Team.OUTSOURCE, outsourcer.GetTeam());
            Assert.AreEqual(Team.SINGLES, strategist.GetTeam());
            Assert.AreEqual(Team.SINGLES, raider.GetTeam());
        }

        [TestMethod]
        public void IntroductoryCycle_TransitionsTo_OvertimeCycle()
        {
            var director = new Player(1, new GeneralDirectorRole());
            var sysadmin = new Player(2, new SystemAdministratorRole());
            var hacker = new Player(3, new HackerRole());
            var players = new[] { director, sysadmin, hacker };

            var intro = new IntroductoryDayCycle();
            var engine = new GameEngine(players, intro);
            intro.Engine = engine;

            engine.NextTurn();
            Assert.IsInstanceOfType(engine.CurrentCycle, typeof(OvertimeCycle));
        }
    }
}
