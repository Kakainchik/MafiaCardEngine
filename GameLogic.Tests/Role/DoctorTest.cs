using GameLogic.Cycles.Night;
using GameLogic.Cycles.Night.Visitors;
using GameLogic.Roles;
using GameLogic.Tests.Configs;

namespace GameLogic.Tests.Role
{
    [TestClass]
    public class DoctorTest
    {
        [TestMethod]
        public void MafiaKill_FewDoctorsHeal_OnlyDoctorHeal()
        {
            //Arrange
            Player doctor1 = new Player(1L, new DoctorRole());
            Player doctor2 = new Player(2L, new DoctorRole());
            Player doctor3 = new Player(3L, new DoctorRole());
            Player citizen1 = new Player(4L, new CitizenRole());

            List<Player> players = new List<Player>()
            {
                doctor1, doctor2, citizen1
            };
            NightCycle night = new NightCycle(players);

            //Act
            night.ConfirmAction(doctor1, citizen1);
            night.ConfirmAction(doctor2, citizen1);
            night.ConfirmAction(doctor3, citizen1);
            night.ExecuteActions();

            //Assert
            int expected = 1;
            int actual = citizen1.Role.Visitors.OfType<HealVisitor>()
                .Select(v => v.Visitor)
                .OfType<DoctorRole>()
                .Count();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MafiaKill_DoctorRevive_TargetIsAlive()
        {
            //Arrange
            List<Player> p = PlayerSetups.MinimumPlayersSetup();
            NightCycle night = new NightCycle(p);

            //Act
            //p[0] is Mafia
            night.ConfirmAction(p[0], p[2]);
            //p[1] is Doctor
            night.ConfirmAction(p[1], p[2]);

            IReadOnlyCollection<ActionLog> logs = night.ExecuteActions();

            //Assert
            Assert.IsTrue(p[2].IsAlive);
        }

        [TestMethod]
        public void CitizenIsKilledTwice_TwoDoctorsRevive_CitizenIsKilledAnyway()
        {
            //Arrange
            Player mafia = new Player(1L, new MafiaRole());
            Player vigilante = new Player(2L, new VigilanteRole() { Items = 5 });
            Player doctor1 = new Player(3L, new DoctorRole());
            Player doctor2 = new Player(4L, new DoctorRole());
            Player citizen = new Player(5L, new CitizenRole());

            List<Player> p = new List<Player>
            {
                mafia, vigilante, doctor1, doctor2, citizen
            };
            NightCycle night = new NightCycle(p);

            //Act
            //p[0] is Mafia
            night.ConfirmAction(mafia, citizen);
            //p[1] is Vigi
            night.ConfirmAction(vigilante, citizen);
            //p[2] and p[3] are Doctor
            night.ConfirmAction(doctor1, citizen);
            night.ConfirmAction(doctor2, citizen);

            IReadOnlyCollection<ActionLog> logs = night.ExecuteActions();

            //Assert
            Assert.IsFalse(citizen.IsAlive);
        }
    }
}