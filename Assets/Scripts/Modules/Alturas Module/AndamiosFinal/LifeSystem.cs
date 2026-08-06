using System;

/// <summary>
/// Sistema de vidas reutilizable. Clase pura de C# (no MonoBehaviour).
/// Instanciar desde Module4Manager pasando maxLives.
/// </summary>
public class LifeSystemFinal
{
    public int CurrentLives { get; private set; }
    public int MaxLives     { get; private set; }

    /// <summary>Disparado cada vez que cambian las vidas: (actual, máximo).</summary>
    public event Action<int, int> OnLivesChanged;

    /// <summary>Disparado cuando CurrentLives llega a 0.</summary>
    public event Action OnLivesEmpty;

    public LifeSystemFinal(int maxLives)
    {
        MaxLives     = maxLives;
        CurrentLives = maxLives;
    }

    public void LoseLife()
    {
        if (CurrentLives <= 0) return;
        CurrentLives--;
        OnLivesChanged?.Invoke(CurrentLives, MaxLives);
        if (CurrentLives == 0)
            OnLivesEmpty?.Invoke();
    }

    public void Reset()
    {
        CurrentLives = MaxLives;
        OnLivesChanged?.Invoke(CurrentLives, MaxLives);
    }
}
