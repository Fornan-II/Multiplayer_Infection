using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour {

    public GameObject healthPanel;
    public Text healthText;
    public Image healthImage;

    protected int _maxHealth = 100;

    public void UpdateHealthUI(int currentHealth)
    {
        healthText.text = "Health: " + currentHealth;
        healthImage.fillAmount = (float)currentHealth / _maxHealth;
    }

    public void SetMaxHealth(int maxHealth)
    {
        _maxHealth = maxHealth;
    }

    public void IsVisible(bool value)
    {
        healthPanel.SetActive(value);
    }
}
