namespace Interhaptics.Assets.Scripts.Extensions
{

    public static class OperatorExtensions
    {

        public static int AbsMod (this int a, int b)
        {
            int r = a % b;
            return r < 0 ? r+b : r;
        }

    }

}