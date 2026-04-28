using GameLogic.Roles;

namespace GameLogic.Tests.Configs
{
    internal static class PlayerSetups
    {
        internal static List<Player> MinimumPlayersSetup()
        {
            return new List<Player>
            {
                new Player(1L, new MafiaRole()),//0
                new Player(2L, new DoctorRole()),//1
                new Player(3L, new CitizenRole()),//2
                new Player(4L, new ProstituteRole()),//3
                new Player(5L, new CitizenRole()),//4
            };
        }

        internal static List<Player> MinimumMafiaCitizenPlayersSetup()
        {
            return new List<Player>
            {
                new Player(1L, new MafiaRole()),
                new Player(2L, new CitizenRole()),
                new Player(3L, new CitizenRole()),
                new Player(4L, new CitizenRole()),
                new Player(5L, new CitizenRole()),
            };
        }

        internal static List<Player> LargePlayersSetup()
        {
            return new List<Player>(MinimumPlayersSetup())
            {
                new Player(6L, new MafiaRole()),//5
                new Player(7L, new WitchRole()),//6
                new Player(8L, new DriverRole()),//7
                new Player(9L, new RecruitRole()),//8
                new Player(10L, new GodfatherRole()),//9
                new Player(11L, new WhoreRole()),//10
                new Player(12L, new VigilanteRole()),//11
                new Player(13L, new SurgeonRole()),//12
                new Player(14L, new PolicemanRole()),//13
                new Player(15L, new DetectiveRole()),//14
                new Player(16L, new CultistRole()),//15
                new Player(17L, new CultusLeaderRole()),//16
                new Player(18L, new SerialKillerRole()),//17
                new Player(19L, new ZombieRole()),//18
                new Player(20L, new CursedRole())//19
            };
        }
    }
}