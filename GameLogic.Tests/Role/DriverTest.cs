using GameLogic.Cycles.Night;
using GameLogic.Roles;

namespace GameLogic.Tests.Role
{
    [TestClass]
    public class DriverTest
    {
        [TestMethod]
        public void DriversSwap_SecondaryTargets_DriverIsKilled()
        {
            //Arrange
            List<Player> p = new List<Player>
            {
                new Player(1L, new MafiaRole()),
                new Player(2L, new CitizenRole()),
                new Player(3L, new DoctorRole()),
                new Player(4L, new PolicemanRole()),
                new Player(5L, new DriverRole()),
                new Player(6L, new DriverRole())
            };
            NightCycle night = new NightCycle(p);

            //Act
            //p[0] is Mafia
            night.ConfirmAction(p[0], p[2]);
            //p[4] and [5] are Driver
            //                                       0 1  2--3  4 5
            night.ConfirmAction(p[4], p[2], p[3]); //0 1  3- 2 -4 5
            night.ConfirmAction(p[5], p[2], p[4]); //0 1 [4] 2  3 5

            night.ExecuteActions();

            //Assert
            Assert.IsFalse(p[4].IsAlive);
        }

        [TestMethod]
        public void DriversSwap_SameTargets_DoctorIsKilledAnyway()
        {
            //Arrange
            List<Player> p = new List<Player>
            {
                new Player(1L, new MafiaRole()),
                new Player(2L, new CitizenRole()),
                new Player(3L, new DoctorRole()),
                new Player(4L, new PolicemanRole()),
                new Player(5L, new DriverRole()),
                new Player(6L, new DriverRole())
            };
            NightCycle night = new NightCycle(p);

            //Act
            //p[0] is Mafia
            night.ConfirmAction(p[0], p[2]);
            //p[4] and [5] are Driver
            //                                       0 1  2--3  4 5
            night.ConfirmAction(p[4], p[2], p[3]); //0 1  3--2  4 5
            night.ConfirmAction(p[5], p[3], p[2]); //0 1 [2] 3  4 5

            night.ExecuteActions();

            //Assert
            Assert.IsFalse(p[2].IsAlive);
        }

        [TestMethod]
        public void DriversSpawTwice_DoctorIsTarget_PolicemanIsKilled()
        {
            //Arrange
            List<Player> p = new List<Player>
            {
                new Player(1L, new MafiaRole()),
                new Player(2L, new CitizenRole()),
                new Player(3L, new DoctorRole()),
                new Player(4L, new PolicemanRole()),
                new Player(5L, new DriverRole()),
                new Player(6L, new DriverRole())
            };
            NightCycle night = new NightCycle(p);

            //Act
            //p[0] is Mafia
            night.ConfirmAction(p[0], p[2]);
            //p[4] and [5] are Driver
            //                                       0 1--2  3 4 5
            night.ConfirmAction(p[4], p[1], p[2]); //0 2  1--3 4 5
            night.ConfirmAction(p[5], p[2], p[3]); //0 2 [3] 1 4 5
            night.ConfirmAction(p[2], p[1]);

            night.ExecuteActions();

            //Assert
            Assert.IsFalse(p[3].IsAlive);
        }

        [TestMethod]
        public void DriverSpaws_SwapMafiaWithHisTarget_MafiaKillsHimself()
        {
            //Arrange
            List<Player> p = new List<Player>
            {
                new Player(1L, new MafiaRole()),
                new Player(2L, new CitizenRole()),
                new Player(3L, new DriverRole())
            };
            NightCycle night = new NightCycle(p);

            //Act
            //p[0] is Mafia
            night.ConfirmAction(p[0], p[1]);
            //p[2] is Driver
            //                                       0--1  2
            night.ConfirmAction(p[2], p[0], p[1]); //1 [0] 2

            night.ExecuteActions();

            //Assert
            Assert.IsFalse(p[0].IsAlive);
        }

        [TestMethod]
        public void DriverSpaws_SwapSerialWithHisTarget_MafiaKillsHimself()
        {
            //Arrange
            List<Player> p = new List<Player>
            {
                new Player(1L, new SerialKillerRole()),
                new Player(2L, new CitizenRole()),
                new Player(3L, new DriverRole())
            };
            NightCycle night = new NightCycle(p);

            //Act
            //p[0] is Mafia
            night.ConfirmAction(p[0], p[1]);
            //p[2] is Driver
            //                                       0--1  2
            night.ConfirmAction(p[2], p[1], p[0]); //1 [0] 2

            night.ExecuteActions();

            //Assert
            Assert.IsFalse(p[0].IsAlive);
        }

        [TestMethod]
        public void DriverSwap_WitcherControlMafia_DoctorIsKilled()
        {
            //Arrange
            List<Player> p = new List<Player>
            {
                new Player(1L, new MafiaRole()),
                new Player(2L, new DriverRole()),
                new Player(3L, new DoctorRole()),
                new Player(4L, new CitizenRole()),
                new Player(5L, new WitchRole())
            };
            NightCycle night = new NightCycle(p);

            //Act
            //p[0] is Mafia
            night.ConfirmAction(p[0], p[2]);
            //p[1] is Driver
            night.ConfirmAction(p[1], p[2], p[3]);// 0 1 [2]-3  (4)
            //p[4] is Witch                          0 1 [3] 2  (4)
            night.ConfirmAction(p[4], p[0], p[3]);// 0 1  3 [2] (4)

            night.ExecuteActions();

            //Assert
            Assert.IsFalse(p[2].IsAlive);
        }

        [TestMethod]
        public void DriverSwap_PoliceActs_PoliceInvestigatesOther()
        {
            //Arrange
            List<Player> p = new List<Player>
            {
                new Player(1L, new MafiaRole()),
                new Player(2L, new CitizenRole()),
                new Player(3L, new DriverRole()),
                new Player(4L, new PolicemanRole())
            };
            NightCycle night = new NightCycle(p);

            //Act
            night.ConfirmAction(p[2], p[0], p[1]);
            night.ConfirmAction(p[2], p[0], p[3]);
            night.ConfirmAction(p[3], p[1]);

            night.ExecuteActions();

            //Assert

        }
    }
}