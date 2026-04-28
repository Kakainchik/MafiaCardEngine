using GameLogic.Cycles.Night;
using GameLogic.Roles;

namespace GameLogic.Tests.Role
{
    [TestClass]
    public class TerroristTest
    {
        [TestMethod]
        public void TerroristBlow_TargetAndHimself_BothIsKilled()
        {
            //Arrange
            Player terrorist = new Player(1L, new TerroristRole());
            Player doctor = new Player(2L, new DoctorRole());

            List<Player> p = new List<Player>
            {
                terrorist, doctor
            };
            NightCycle night = new NightCycle(p);

            //Act
            night.ConfirmAction(terrorist, doctor);
            night.ExecuteActions();

            //Assert
            Assert.IsFalse(terrorist.IsAlive);
            Assert.IsFalse(doctor.IsAlive);
        }

        [TestMethod]
        public void TerroristBlow_DoctorHeals_TargetIsKilled()
        {
            //Arrange
            Player terrorist = new Player(1L, new TerroristRole());
            Player doctor = new Player(2L, new DoctorRole());
            Player citizen = new Player(3L, new CitizenRole());

            List<Player> p = new List<Player>
            {
                terrorist, doctor, citizen
            };
            NightCycle night = new NightCycle(p);

            //Act
            night.ConfirmAction(terrorist, citizen);
            night.ConfirmAction(doctor, citizen);

            night.ExecuteActions();

            //Assert
            Assert.IsFalse(terrorist.IsAlive);
            Assert.IsFalse(doctor.IsAlive);
            Assert.IsFalse(citizen.IsAlive);
        }

        [TestMethod]
        public void EveryoneVisitsBlowTarget_ExceptDoctor_EveryoneIsKilled()
        {
            //Arrange
            Player terrorist = new Player(1L, new TerroristRole());
            Player police = new Player(2L, new PolicemanRole());
            Player mafia = new Player(3L, new MafiaRole());
            Player whore = new Player(4L, new WhoreRole());
            Player doctor = new Player(5L, new DoctorRole());

            List<Player> p = new List<Player>
            {
                terrorist, police, mafia, whore, doctor
            };
            NightCycle night = new NightCycle(p);

            //Act
            night.ConfirmAction(terrorist, doctor);
            night.ConfirmAction(police, doctor);
            night.ConfirmAction(mafia, doctor);
            night.ConfirmAction(whore, doctor);

            night.ExecuteActions();

            //Assert
            foreach(var player in p)
            {
                Assert.IsFalse(player.IsAlive);
            }
        }

        [TestMethod]
        public void TerroristBlow_WhoreBlocks_NobodyIsKilled()
        {
            //Arrange
            Player terrorist = new Player(1L, new TerroristRole());
            Player doctor = new Player(2L, new DoctorRole());
            Player whore = new Player(3L, new WhoreRole());

            List<Player> p = new List<Player>
            {
                terrorist, doctor, whore
            };
            NightCycle night = new NightCycle(p);

            //Act
            night.ConfirmAction(terrorist, doctor);
            night.ConfirmAction(whore, terrorist);

            night.ExecuteActions();

            //Assert
            Assert.IsTrue(terrorist.IsAlive);
            Assert.IsTrue(doctor.IsAlive);
        }

        [TestMethod]
        public void TerroristBlow_TargetLeftHome_OnlyTerroristIsKilled()
        {
            //Arrange
            Player terrorist = new Player(1L, new TerroristRole());
            Player doctor = new Player(2L, new DoctorRole());
            Player citizen = new Player(3L, new CitizenRole());

            List<Player> p = new List<Player>
            {
                terrorist, doctor, citizen
            };
            NightCycle night = new NightCycle(p);

            //Act
            night.ConfirmAction(terrorist, doctor);
            night.ConfirmAction(doctor, citizen);

            night.ExecuteActions();

            //Assert
            Assert.IsFalse(terrorist.IsAlive);
            Assert.IsTrue(doctor.IsAlive);
            Assert.IsTrue(citizen.IsAlive);
        }
    }
}