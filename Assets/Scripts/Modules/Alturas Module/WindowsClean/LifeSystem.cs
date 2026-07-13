using System;
using UnityEngine;

public class LifeSystem : MonoBehaviour
{
    [SerializeField] public int maxLifes = 3;

    public int currentLifes;

    public event Action<int> OnLifehChanged;
    public event Action OnGameOver;

    public void Awake()
    {
        currentLifes = maxLifes;
    }

    public void ReceiveDamage(int damage)
    {
        currentLifes -= damage;

        OnLifehChanged?.Invoke(currentLifes);

        if(currentLifes <= 0)
        {
            Debug.Log("Se quedo sin vidas");
            OnGameOver?.Invoke();

           

        }
    }
}
