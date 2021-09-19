using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int maxHealth = 1;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private SpriteRenderer healthBar;
    [SerializeField] private SpriteRenderer healthFill;

    private int _currentHealth;
    
    public Vector3 TargetPosition { get; set; }
    public int CurrentPathIndex { get; set; }
    // Fungsi ini terpanggil sekali setiap kali menghidupkan game object yang memiliki script ini
    private void OnEnable()
    {
        _currentHealth = maxHealth;
        healthFill.size = healthBar.size;
    }

    public void MoveToTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, TargetPosition, moveSpeed * Time.deltaTime);
    }

    public void SetTargetPosition(Vector3 targetPosition)
    {
        TargetPosition = targetPosition;
        healthBar.transform.parent = null;
        // Mengubah rotasi dari enemy
        Vector3 distance = TargetPosition - transform.position;

        if (Mathf.Abs(distance.y) > Mathf.Abs(distance.x))
        {
            // Menghadap atas
            if (distance.y > 0)
            {
                transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));
            }
            else
            // Menghadap bawah
            {
                transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, -90f));
            }
        }
        else
        {
            // Menghadap kanan (default)
            if (distance.x > 0)
            {
                transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
            }
            else
            // Menghadap kiri
            {
                transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 180f));
            }
        }

        healthBar.transform.parent = transform;
    }

    public void SetCurrentPathIndex(int currentIndex)
    {
        CurrentPathIndex = currentIndex;
    }

    public void ReduceEnemyHealth(int damage)
    {
        _currentHealth -= damage;
        AudioPlayer.Instance.PlaySfx("hit-enemy");
        if (_currentHealth <= 0)
        {
            gameObject.SetActive(false);
            AudioPlayer.Instance.PlaySfx("enemy-die");
        }

        float healthPercentage = (float) _currentHealth / maxHealth;
        var healthBarSize = healthBar.size;
        healthFill.size = new Vector2(healthPercentage * healthBarSize.x, healthBarSize.y);
    }
}
