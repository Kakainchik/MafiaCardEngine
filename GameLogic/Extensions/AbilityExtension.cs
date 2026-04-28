using GameLogic.Attributes;
using GameLogic.Cycles.Night.Abilities;

namespace GameLogic.Extensions
{
    public static class AbilityExtension
    {
        public static void PutByPriority(this List<IAbility> abilities, IAbility toPut)
        {
            abilities.Add(toPut);
            if(abilities.Count == 1) return;

            APriority putP = toPut.GetAbilityPriority();
            for(int i = abilities.Count - 1; ; i--)
            {
                IAbility swap = abilities[i - 1];
                APriority swapP = swap.GetAbilityPriority();

                if(swapP <= putP) return;

                abilities[i - 1] = abilities[i];
                abilities[i] = swap;
            }
        }
    }

    public class AbilityComparer : IComparer<IAbility>
    {
        private static Lazy<AbilityComparer> instance = new Lazy<AbilityComparer>();

        public static AbilityComparer Instance => instance.Value;

        public int Compare(IAbility? x, IAbility? y)
        {
            if(x == null || y == null)
            {
                return 0;
            }

            APriority xP = x.GetAbilityPriority();
            APriority yP = y.GetAbilityPriority();

            if(xP == yP)
            {
                return 0;
            }
            else if(xP < yP)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }
    }
}