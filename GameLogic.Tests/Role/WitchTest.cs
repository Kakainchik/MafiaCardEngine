using GameLogic.Cycles.Night;
using GameLogic.Roles;

namespace GameLogic.Tests.Role
{
    [TestClass]
    public class WitchTest
    {
        [TestMethod]
        public void MafiaKill_WitchControlMafia_MafiaKillAnother()
        {
            //Arrange
            Player witch1 = new Player(1L, new WitchRole());
            Player mafia1 = new Player(2L, new MafiaRole());
            Player citizen1 = new Player(3L, new CitizenRole());
            Player citizen2 = new Player(4L, new CitizenRole());

            List<Player> players = new List<Player>()
            {
                witch1, mafia1, citizen1, citizen2
            };
            NightCycle night = new NightCycle(players);

            //Act
            night.ConfirmAction(witch1, mafia1, citizen2);
            night.ConfirmAction(mafia1, citizen1);
            night.ExecuteActions();

            //Assert
            Assert.IsFalse(citizen2.IsAlive);
        }
    }
}