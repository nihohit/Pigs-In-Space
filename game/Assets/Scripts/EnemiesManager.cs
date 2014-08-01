using System.Collections.Generic;

public interface IHostileEntity
{
    void Act();
}

public static class EnemiesManager
{
    private static List<IHostileEntity> s_activeEntities = new List<IHostileEntity>();

    public static void AddEnemy(IHostileEntity enemy)
    {
        s_activeEntities.Add(enemy);
    }

    public static void EnemiesTurn()
    {
        foreach (var enemy in s_activeEntities.ToArray())
        {
            enemy.Act();
        }
    }

    internal static void Remove(IHostileEntity enemy)
    {
        s_activeEntities.Remove(enemy);
    }
}