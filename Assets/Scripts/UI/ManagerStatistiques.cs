using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ManagerStatistiques : MonoBehaviour
{
    public static ManagerStatistiques instance;

    public List<Ship> allships;

    public LayerMask playerShipLayerMask;
    public LayerMask invaderLayerMask;

    public List<Ship> shipPlayer;
    public List<Ship> shipInvaders;

    public float playerDamageLast10Seconds;
    public float invaderDamageLast10Seconds;

    public float playerDPS;
    public float invaderDPS;

    public float playerCurrentPV;
    public float invaderCurrentPV;

    public Image barShips;
    public Image barPV;
    public Image barDPS;
    public Image barDPTen;

    public Image globalSituation;

    public TextMeshProUGUI textPlayerShip;
    public TextMeshProUGUI textInvaderShip;
    public TextMeshProUGUI textPlayerPV;
    public TextMeshProUGUI textInvaderPV;
    public TextMeshProUGUI textPlayerDPS;
    public TextMeshProUGUI textInvaderDPS;
    public TextMeshProUGUI textPlayerDPTen;
    public TextMeshProUGUI textInvaderDPTen;


    private void OnEnable()
    {
        instance = this;

        // Récupère tous les vaisseaux déjà présents et actifs dans la scène
        // AVANT de s'abonner, pour ne pas les compter deux fois.
        Ship[] existingShips = FindObjectsByType<Ship>(FindObjectsSortMode.None);
        foreach (Ship ship in existingShips)
        {
            if (ship.gameObject.activeSelf)
                HandleShipCreated(ship);
        }


        Ship.OnShipCreated += HandleShipCreated;
        Ship.OnShipTakeDamage += HandleShipTakeDamage;
        Ship.OnShipDestroyed += HandleShipDestroyed;

        StartCoroutine(UpdateDamage());
        StartCoroutine(UpdateUI());
    }


    private void OnDisable()
    {
        Ship.OnShipCreated -= HandleShipCreated;
        Ship.OnShipTakeDamage -= HandleShipTakeDamage;
        Ship.OnShipDestroyed -= HandleShipDestroyed;
    }

    [Tooltip("Vitesse de rattrapage du lerp. Plus haut = rattrape plus vite (moins de lissage visible), plus bas = plus lent/fluide.")]
    public float fillSmoothSpeed = 8f;

    void FixedUpdate()
    {
        float ratioNbShip = SafeRatio(shipPlayer.Count, shipInvaders.Count);
        UpdateBarSmooth(barShips, ratioNbShip);

        float ratioPV = SafeRatio(playerCurrentPV, invaderCurrentPV);
        UpdateBarSmooth(barPV, ratioPV);

        float ratioDPS = SafeRatio(playerDPS, invaderDPS);
        UpdateBarSmooth(barDPS, ratioDPS);

        float ratioDPSTEN = SafeRatio(playerDamageLast10Seconds, invaderDamageLast10Seconds);
        UpdateBarSmooth(barDPTen, ratioDPSTEN);

        float globalRatio = (ratioNbShip + ratioPV + ratioDPS + ratioDPSTEN) / 4;
        UpdateBarSmooth(globalSituation, globalRatio);
    }

    /// <summary>
    /// Lerp exponentiel vers targetValue, indépendant du framerate. Contrairement
    /// à Mathf.Lerp(bar.fillAmount, target, vitesse * Time.fixedDeltaTime) — un
    /// piège classique — cette formule donne le MÊME résultat visuel peu importe
    /// le framerate/le fixedDeltaTime, parce qu'elle compose correctement le
    /// taux de rattrapage sur plusieurs frames au lieu de l'additionner linéairement.
    /// </summary>
    void UpdateBarSmooth(Image bar, float targetValue)
    {
        float t = 1f - Mathf.Exp(-fillSmoothSpeed * Time.fixedDeltaTime);
        bar.fillAmount = Mathf.Lerp(bar.fillAmount, targetValue, t);
    }

    public IEnumerator UpdateUI()
    {
        //barShips.fillAmount = SafeRatio(shipPlayer.Count, shipInvaders.Count);
        //barPV.fillAmount = SafeRatio(playerCurrentPV, invaderCurrentPV);
        //barDPS.fillAmount = SafeRatio(playerDPS, invaderDPS);
        //barDPTen.fillAmount = SafeRatio(playerDamageLast10Seconds, invaderDamageLast10Seconds);

        playerCurrentPV = 0;
        invaderCurrentPV = 0;

        foreach (Ship ship in shipPlayer)
        {
            playerCurrentPV += ship.pv;
        }
        foreach (Ship ship in shipInvaders)
        {
            invaderCurrentPV += ship.pv;
        }

        textPlayerShip.text = shipPlayer.Count.ToString();
        textInvaderShip.text = shipInvaders.Count.ToString();
        textPlayerPV.text = playerCurrentPV.ToString("F0");
        textInvaderPV.text = invaderCurrentPV.ToString("F0");
        textPlayerDPS.text = playerDPS.ToString("F1");
        textInvaderDPS.text = invaderDPS.ToString("F1");
        textPlayerDPTen.text = playerDamageLast10Seconds.ToString("F1");
        textInvaderDPTen.text = invaderDamageLast10Seconds.ToString("F1");

        yield return new WaitForSeconds(0.2f);
        StartCoroutine(UpdateUI());
    }

    /// <summary>
    /// Ratio a / (a + b), sécurisé contre la division par zéro. Si les deux
    /// valeurs sont à 0 (aucun combat encore), retourne 0.5 (barre à
    /// l'équilibre) plutôt qu'un NaN qui casse le rendu du Canvas.
    /// </summary>
    float SafeRatio(float a, float b)
    {
        float total = a + b;
        if (total <= 0f) return 0.5f;
        return Mathf.Clamp01(a / total);
    }

    public IEnumerator UpdateDamage()
    {
        playerDamageLast10Seconds = GetPlayerDamageLast10Seconds();
        invaderDamageLast10Seconds = GetInvaderDamageLast10Seconds();
        playerDPS = GetPlayerDPS(); invaderDPS = GetInvaderDPS();
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(UpdateDamage());
    }

    private void HandleShipCreated(Ship ship)
    {
        allships.Add(ship);

        if ((playerShipLayerMask.value & (1 << ship.gameObject.layer)) != 0)
        {
            shipPlayer.Add(ship);
        }
        else if ((invaderLayerMask.value & (1 << ship.gameObject.layer)) != 0)
        {
            shipInvaders.Add(ship);
        }
    }


    private void HandleShipTakeDamage(Ship ship, float damage)
    {
        if ((playerShipLayerMask.value & (1 << ship.gameObject.layer)) != 0)
        {
            invaderDamageInstances.Add(new DamageInstance(damage));
        }
        else if ((invaderLayerMask.value & (1 << ship.gameObject.layer)) != 0)
        {
            playerDamageInstances.Add(new DamageInstance(damage));
        }
    }


    private void HandleShipDestroyed(Ship ship)
    {
        // Retirer le Ship des listes
        allships.Remove(ship);
        shipPlayer.Remove(ship);
        shipInvaders.Remove(ship);
    }


    // =========================
    // DAMAGE / DPS
    // =========================

    private struct DamageInstance
    {
        public float damage;
        public float time;

        public DamageInstance(float damage)
        {
            this.damage = damage;
            time = Time.time;
        }
    }

    private List<DamageInstance> playerDamageInstances = new();
    private List<DamageInstance> invaderDamageInstances = new();


    public float GetPlayerDamageLast10Seconds()
    {
        return GetDamageLast10Seconds(playerDamageInstances);
    }

    public float GetInvaderDamageLast10Seconds()
    {
        return GetDamageLast10Seconds(invaderDamageInstances);
    }


    private float GetDamageLast10Seconds(List<DamageInstance> damageInstances)
    {
        float totalDamage = 0f;
        float cutoff = Time.time - 10f;

        for (int i = damageInstances.Count - 1; i >= 0; i--)
        {
            if (damageInstances[i].time < cutoff)
            {
                damageInstances.RemoveAt(i);
            }
            else
            {
                totalDamage += damageInstances[i].damage;
            }
        }

        return totalDamage;
    }


    public float GetPlayerDPS()
    {
        return GetPlayerDamageLast10Seconds() / 10f;
    }

    public float GetInvaderDPS()
    {
        return GetInvaderDamageLast10Seconds() / 10f;
    }
}