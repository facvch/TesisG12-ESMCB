namespace Domain.Others.Utils
{
    internal static class RandomUtils
    {
        internal static int GenerarLegajo()
        {
            // Genera un número aleatorio entre 100000 y 999999 inclusive
            return Random.Shared.Next(100000, 1000000);
        }
    }
}
