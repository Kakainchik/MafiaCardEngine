using DiscordBot.Extensions;
using DiscordBot.Resources;
using WebServer.Shared.ParanoiaCorp.GameObjects;

namespace DiscordBot.Model
{
    public enum RoleVisual : byte
    {
        [LocalizedDescription("MasterOfCompromising", typeof(RoleNames), Locilize = RoleLocilize.NAME)]
        [LocalizedDescription("MasterOfCompromising", typeof(RoleDescriptions), Locilize = RoleLocilize.DESCRIPTION)]
        MASTER_OF_COMPROMISING = RoleSignature.MASTER_OF_COMPROMISING,

        [LocalizedDescription("Spy", typeof(RoleNames), Locilize = RoleLocilize.NAME)]
        [LocalizedDescription("Spy", typeof(RoleDescriptions), Locilize = RoleLocilize.DESCRIPTION)]
        SPY = RoleSignature.SPY,

        [LocalizedDescription("Hacker", typeof(RoleNames), Locilize = RoleLocilize.NAME)]
        [LocalizedDescription("Hacker", typeof(RoleDescriptions), Locilize = RoleLocilize.DESCRIPTION)]
        HACKER = RoleSignature.HACKER,

        [LocalizedDescription("ShadowDirector", typeof(RoleNames), Locilize = RoleLocilize.NAME)]
        [LocalizedDescription("ShadowDirector", typeof(RoleDescriptions), Locilize = RoleLocilize.DESCRIPTION)]
        SHADOW_DIRECTOR = RoleSignature.SHADOW_DIRECTOR,

        [LocalizedDescription("GeneralDirector", typeof(RoleNames), Locilize = RoleLocilize.NAME)]
        [LocalizedDescription("GeneralDirector", typeof(RoleDescriptions), Locilize = RoleLocilize.DESCRIPTION)]
        GENERAL_DIRECTOR = RoleSignature.GENERAL_DIRECTOR,

        [LocalizedDescription("Clerk", typeof(RoleNames), Locilize = RoleLocilize.NAME)]
        [LocalizedDescription("Clerk", typeof(RoleDescriptions), Locilize = RoleLocilize.DESCRIPTION)]
        CLERK = RoleSignature.CLERK,
        
        [LocalizedDescription("SecuritySpecialist", typeof(RoleNames), Locilize = RoleLocilize.NAME)]
        [LocalizedDescription("SecuritySpecialist", typeof(RoleDescriptions), Locilize = RoleLocilize.DESCRIPTION)]
        SECURITY_SPECIALIST = RoleSignature.SECURITY_SPECIALIST,

        [LocalizedDescription("Auditor", typeof(RoleNames), Locilize = RoleLocilize.NAME)]
        [LocalizedDescription("Auditor", typeof(RoleDescriptions), Locilize = RoleLocilize.DESCRIPTION)]
        AUDITOR = RoleSignature.AUDITOR,

        [LocalizedDescription("HRManager", typeof(RoleNames), Locilize = RoleLocilize.NAME)]
        [LocalizedDescription("HRManager", typeof(RoleDescriptions), Locilize = RoleLocilize.DESCRIPTION)]
        HR_MANAGER = RoleSignature.HR_MANAGER,

        [LocalizedDescription("SystemAdministrator", typeof(RoleNames), Locilize = RoleLocilize.NAME)]
        [LocalizedDescription("SystemAdministrator", typeof(RoleDescriptions), Locilize = RoleLocilize.DESCRIPTION)]
        SYSTEM_ADMINISTRATOR = RoleSignature.SYSTEM_ADMINISTRATOR,

        [LocalizedDescription("AnticrisisManager", typeof(RoleNames), Locilize = RoleLocilize.NAME)]
        [LocalizedDescription("AnticrisisManager", typeof(RoleDescriptions), Locilize = RoleLocilize.DESCRIPTION)]
        ANTICRISIS_MANAGER = RoleSignature.ANTICRISIS_MANAGER,

        [LocalizedDescription("Intern", typeof(RoleNames), Locilize = RoleLocilize.NAME)]
        [LocalizedDescription("Intern", typeof(RoleDescriptions), Locilize = RoleLocilize.DESCRIPTION)]
        INTERN = RoleSignature.INTERN,

        [LocalizedDescription("ScrumMaster", typeof(RoleNames), Locilize = RoleLocilize.NAME)]
        [LocalizedDescription("ScrumMaster", typeof(RoleDescriptions), Locilize = RoleLocilize.DESCRIPTION)]
        SCRUM_MASTER = RoleSignature.SCRUM_MASTER,

        [LocalizedDescription("Shareholder", typeof(RoleNames), Locilize = RoleLocilize.NAME)]
        [LocalizedDescription("Shareholder", typeof(RoleDescriptions), Locilize = RoleLocilize.DESCRIPTION)]
        SHAREHOLDER = RoleSignature.SHAREHOLDER,

        [LocalizedDescription("LayoffCandidate", typeof(RoleNames), Locilize = RoleLocilize.NAME)]
        [LocalizedDescription("LayoffCandidate", typeof(RoleDescriptions), Locilize = RoleLocilize.DESCRIPTION)]
        LAYOFF_CANDIDATE = RoleSignature.LAYOFF_CANDIDATE,

        [LocalizedDescription("StartupFounder", typeof(RoleNames), Locilize = RoleLocilize.NAME)]
        [LocalizedDescription("StartupFounder", typeof(RoleDescriptions), Locilize = RoleLocilize.DESCRIPTION)]
        STARUP_FOUNDER = RoleSignature.STARUP_FOUNDER,

        [LocalizedDescription("Evangelist", typeof(RoleNames), Locilize = RoleLocilize.NAME)]
        [LocalizedDescription("Evangelist", typeof(RoleDescriptions), Locilize = RoleLocilize.DESCRIPTION)]
        EVANGELIST = RoleSignature.EVANGELIST,

        [LocalizedDescription("Raider", typeof(RoleNames), Locilize = RoleLocilize.NAME)]
        [LocalizedDescription("Raider", typeof(RoleDescriptions), Locilize = RoleLocilize.DESCRIPTION)]
        RAIDER = RoleSignature.RAIDER,

        [LocalizedDescription("PoliticalStrategist", typeof(RoleNames), Locilize = RoleLocilize.NAME)]
        [LocalizedDescription("PoliticalStrategist", typeof(RoleDescriptions), Locilize = RoleLocilize.DESCRIPTION)]
        POLITICAL_STRATEGIST = RoleSignature.POLITICAL_STRATEGIST,

        [LocalizedDescription("Outsourcer", typeof(RoleNames), Locilize = RoleLocilize.NAME)]
        [LocalizedDescription("Outsourcer", typeof(RoleDescriptions), Locilize = RoleLocilize.DESCRIPTION)]
        OUTSOURCER = RoleSignature.OUTSOURCER,

        [LocalizedDescription("AlumniManager", typeof(RoleNames), Locilize = RoleLocilize.NAME)]
        [LocalizedDescription("AlumniManager", typeof(RoleDescriptions), Locilize = RoleLocilize.DESCRIPTION)]
        ALUMNI_MANAGER = RoleSignature.ALUMNI_MANAGER
    }
}