using GameLogic.ParanoiaCorp.Roles;
using WebServer.Shared.Attributes;

namespace WebServer.Shared.ParanoiaCorp.GameObjects
{
    public enum RoleSignature : byte
    {
        [RoleType(typeof(MasterOfCompromisingRole))]
        MASTER_OF_COMPROMISING = 0,

        [RoleType(typeof(SpyRole))]
        SPY = 1,

        [RoleType(typeof(HackerRole))]
        HACKER = 2,

        [RoleType(typeof(ShadowDirectorRole))]
        SHADOW_DIRECTOR = 3,

        [RoleType(typeof(GeneralDirectorRole))]
        GENERAL_DIRECTOR = 4,

        [RoleType(typeof(ClerkRole))]
        CLERK = 5,

        [RoleType(typeof(SecuritySpecialistRole))]
        SECURITY_SPECIALIST = 6,

        [RoleType(typeof(AuditorRole))]
        AUDITOR = 7,

        [RoleType(typeof(HRManagerRole))]
        HR_MANAGER = 8,

        [RoleType(typeof(SystemAdministratorRole))]
        SYSTEM_ADMINISTRATOR = 9,

        [RoleType(typeof(AnticrisisManagerRole))]
        ANTICRISIS_MANAGER = 10,

        [RoleType(typeof(InternRole))]
        INTERN = 11,

        [RoleType(typeof(ScrumMasterRole))]
        SCRUM_MASTER = 12,

        [RoleType(typeof(ShareholderRole))]
        SHAREHOLDER = 13,

        [RoleType(typeof(LayoffCandidateRole))]
        LAYOFF_CANDIDATE = 14,

        [RoleType(typeof(StartupFounderRole))]
        STARUP_FOUNDER = 15,

        [RoleType(typeof(EvangelistRole))]
        EVANGELIST = 16,

        [RoleType(typeof(RaiderRole))]
        RAIDER = 17,

        [RoleType(typeof(PoliticalStrategistRole))]
        POLITICAL_STRATEGIST = 18,

        [RoleType(typeof(OutsourcerRole))]
        OUTSOURCER = 19,

        [RoleType(typeof(AlumniManagerRole))]
        ALUMNI_MANAGER = 20
    }
}