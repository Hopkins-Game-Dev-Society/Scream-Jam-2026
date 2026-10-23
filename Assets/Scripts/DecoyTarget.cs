
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Health))]
public class DecoyTarget : MonoBehaviour
{
    private static readonly List<DecoyTarget> activeDecoys =
        new List<DecoyTarget>();

    [SerializeField] private float lifetime = 5f;

    private Health health;

    public Rigidbody2D Rigidbody { get; private set; }
    public Health Health => health;

    public static IReadOnlyList<DecoyTarget> ActiveDecoys =>
        activeDecoys;

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (!activeDecoys.Contains(this))
        {
            activeDecoys.Add(this);
        }
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (health == null || health.IsDead)
        {
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        activeDecoys.Remove(this);
    }
}
