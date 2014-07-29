using System;

public static class Randomizer
{
    private static Random s_random = new Random();

    public static double NextDouble(double min, double max)
    {
        return min + s_random.NextDouble() * (max - min);
    }
}