using GameLogic.Cycles.Night;
using GameLogic.Roles;
using GameLogic.Tests.Configs;

namespace GameLogic.Tests.Role
{
    [TestClass]
    public class MafiaTest
    {
        [TestMethod]
        public void Mafia_KillCitizen_CitizenNotAlive()
        {
            //Arrange
            List<Player> p = PlayerSetups.MinimumMafiaCitizenPlayersSetup();
            NightCycle night = new NightCycle(p);

            //p[0] is Mafia
            CitizenRole citizen = (CitizenRole)p[1].Role;

            //Act
            night.ConfirmAction(p[0], p[1]);
            night.ExecuteActions();

            //Assert
            Assert.IsFalse(citizen.IsAlive);
        }

        [TestMethod]
        public void FewMafia_KillCitizens_OnlyCitizenNotAlive()
        {
            //Arrange
            Player mafia1 = new Player(1L, new MafiaRole());
            Player mafia2 = new Player(2L, new MafiaRole());
            Player citizen1 = new Player(3L, new CitizenRole());
            Player citizen2 = new Player(4L, new CitizenRole());

            List<Player> players = new List<Player>()
            {
                mafia1, mafia2, citizen1, citizen2
            };
            NightCycle night = new NightCycle(players);

            //Act
            night.ConfirmAction(mafia1, citizen1);
            night.ConfirmAction(mafia2, citizen2);
            night.ExecuteActions();

            //Assert
            int expected = 1;
            int actual = players.Select(p => p.Role)
                .OfType<CitizenRole>()
                .Count(citi => !citi.IsAlive);

            Assert.AreEqual(expected, actual);
        }
    }
}