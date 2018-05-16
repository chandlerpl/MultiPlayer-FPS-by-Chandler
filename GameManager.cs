using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AxlPlay;
namespace AxlPlay
{
    public class GameManager : Photon.PunBehaviour
    {
        public UIEffects PauseMenuUI;
        public AudioClip PickUpAmmoSound;
        public GameObject MobileUI;
        public float PickUpDistance = 3f;
        public UIEffects Sight2DSniper;
        public UIEffects InteractIcon;
        //  public KeyCode PickupKey;

        public AudioClip HitMarkerSound;
        public AudioSource GameAudioSource;

        public GameObject KillPopUp;
        public UIEffects DamageIndicator;
        public UIEffects BloodSplash;

        public GameObject FadeWhenSight2D;

        public static GameManager Instance;
        public GameObject HealthPanel;
        public Text HealthUI;
        public float TimeToUnrecoil = 0.2f;
        public float SpeedCrosshairExpand = 5f;
        public float HitCrosshairTime = 1f;
        public RectTransform CrosshairLeft;
        public RectTransform CrosshairRight;
        public RectTransform CrosshairUp;
        public RectTransform CrosshairDown;

        [HideInInspector]
        public Health PlayerHealth;
        [HideInInspector]
        public Weapon PlayerWeapon;
        public GameObject UIWeapon;
        public UIEffects Crosshair;
        public UIEffects HitCrosshair;
        public Text AmmunitionMagazine;
        public Text Ammunition;

        private bool didRecoil;

        // private bool inRecoil;
        private float timer;
        private void Awake()
        {
            Instance = this;
        }

        void Update()
        {
            if (MultiplayerGameManager.Instance.LocalPlayer != null && !Application.isMobilePlatform)
            {
                if (InputManager.inputManager.GetButtonDown(InputManager.inputManager.PauseBt))
                {
                    // fade in pause menu 
                    if (PauseMenuUI)
                    {
                        if (PauseMenuUI.canvasGroup.alpha > 0.9f)
                        {
                            PauseMenuUI.DoFadeOut();
                            MultiplayerGameManager.Instance.ResumeGame();
                        }
                        else
                        {
                            PauseMenuUI.DoFadeIn();
                            MultiplayerGameManager.Instance.PauseGame();
                        }
                    }
                }
            }

        }
        // draw the recoil in the crosshair
        public void RecoilCrosshair()
        {
            if (PlayerWeapon == null)
                return;
            // Left -X
            Vector2 newCrossLeftPos = CrosshairLeft.anchoredPosition;
            newCrossLeftPos.x = -PlayerWeapon.CrosshairShootingPrecision;
            CrosshairLeft.anchoredPosition = Vector2.Lerp(CrosshairLeft.anchoredPosition, newCrossLeftPos, Time.deltaTime * SpeedCrosshairExpand);
            // Down -Y
            Vector2 newCrossDownPos = CrosshairDown.anchoredPosition;
            newCrossDownPos.y = -PlayerWeapon.CrosshairShootingPrecision;
            CrosshairDown.anchoredPosition = Vector2.Lerp(CrosshairDown.anchoredPosition, newCrossDownPos, Time.deltaTime * SpeedCrosshairExpand);
            // Up Y
            Vector2 newCrossUpPos = CrosshairUp.anchoredPosition;
            newCrossUpPos.y = PlayerWeapon.CrosshairShootingPrecision;
            CrosshairUp.anchoredPosition = Vector2.Lerp(CrosshairUp.anchoredPosition, newCrossUpPos, Time.deltaTime * SpeedCrosshairExpand);
            // Right Y
            Vector2 newCrossRightPos = CrosshairRight.anchoredPosition;
            newCrossRightPos.x = PlayerWeapon.CrosshairShootingPrecision;
            CrosshairRight.anchoredPosition = Vector2.Lerp(CrosshairRight.anchoredPosition, newCrossRightPos, Time.deltaTime * SpeedCrosshairExpand);


        }
        // return the crosshair to original pos

        public void UnRecoilCrosshair()
        {
            if (PlayerWeapon != null)
            {

                // Left -X
                Vector2 newCrossLeftPos = CrosshairLeft.anchoredPosition;
                newCrossLeftPos.x = -PlayerWeapon.Owner.GetCurrentCrosshairState();

                CrosshairLeft.anchoredPosition = Vector2.Lerp(CrosshairLeft.anchoredPosition, newCrossLeftPos, Time.deltaTime * (SpeedCrosshairExpand / 2));
                // Down -Y
                Vector2 newCrossDownPos = CrosshairDown.anchoredPosition;
                newCrossDownPos.y = -PlayerWeapon.Owner.GetCurrentCrosshairState();
                CrosshairDown.anchoredPosition = Vector2.Lerp(CrosshairDown.anchoredPosition, newCrossDownPos, Time.deltaTime * (SpeedCrosshairExpand / 2));
                // Right X
                Vector2 newCrossRightPos = CrosshairRight.anchoredPosition;
                newCrossRightPos.x = PlayerWeapon.Owner.GetCurrentCrosshairState();
                CrosshairRight.anchoredPosition = Vector2.Lerp(CrosshairRight.anchoredPosition, newCrossRightPos, Time.deltaTime * (SpeedCrosshairExpand / 2));
                // Up Y
                Vector2 newCrossUpPos = CrosshairUp.anchoredPosition;
                newCrossUpPos.y = PlayerWeapon.Owner.GetCurrentCrosshairState();
                CrosshairUp.anchoredPosition = Vector2.Lerp(CrosshairUp.anchoredPosition, newCrossUpPos, Time.deltaTime * (SpeedCrosshairExpand / 2));

                if (CrosshairLeft.anchoredPosition.x < -(PlayerWeapon.CrosshairShootingPrecision - 0.1f) && CrosshairLeft.anchoredPosition.x > -(PlayerWeapon.Owner.GetCurrentCrosshairState() - 0.1f) && didRecoil)
                {
                    didRecoil = false;
                }

            }
        }
        // show how many ammo
        public void UpdateAmmoUI()
        {
            if (PlayerWeapon)
            {

                Ammunition.text = ("/ " + PlayerWeapon.ammunition).ToString();
                AmmunitionMagazine.text = PlayerWeapon.cartridgeAmmo.ToString();

            }
        }

        // get reference from the health script
        public void GetPlayerHealth(Health _health)
        {
            HealthPanel.gameObject.SetActive(true);
            PlayerHealth = _health;
        }
        // fade out Crosshair UI
        public void FadeOutCrosshair()
        {
            Crosshair.DoFadeOut();
        }
        // fade in Crosshair UI
        public void FadeInCrosshair()
        {
            Crosshair.DoFadeIn();
        }
        // show hit marker when the weapon hits something
        public void WeaponHit()
        {
            Crosshair.DoFadeOut();
            HitCrosshair.DoFadeIn();
            if (GameAudioSource && HitMarkerSound)
                GameAudioSource.PlayOneShot(HitMarkerSound);

            StartCoroutine(HitCrosshairActive());
        }
        IEnumerator HitCrosshairActive()
        {
            yield return new WaitForSeconds(HitCrosshairTime);

            if (!PlayerWeapon.isAimingDown)
                Crosshair.DoFadeIn();
            HitCrosshair.DoFadeOut();
        }
        // when start using weapon put the crosshair with distance
        public void UsingWeapon()
        {
            UIWeapon.gameObject.SetActive(true);
            Vector2 crossLeftPos = CrosshairLeft.anchoredPosition;
            crossLeftPos.x = -PlayerWeapon.Owner.GetCurrentCrosshairState();
            CrosshairLeft.anchoredPosition = crossLeftPos;

            Vector2 crossDownPos = CrosshairDown.anchoredPosition;
            crossDownPos.y = -PlayerWeapon.Owner.GetCurrentCrosshairState();
            CrosshairDown.anchoredPosition = crossDownPos;

            Vector2 crossRightPos = CrosshairRight.anchoredPosition;
            crossRightPos.x = PlayerWeapon.Owner.GetCurrentCrosshairState();
            CrosshairRight.anchoredPosition = crossRightPos;

            Vector2 crossUpPos = CrosshairUp.anchoredPosition;
            crossUpPos.y = PlayerWeapon.Owner.GetCurrentCrosshairState();
            CrosshairUp.anchoredPosition = crossUpPos;


            UpdateAmmoUI();
        }
        //  put the crosshair with specific distance

        public void ExpandCrosshair(float amount)
        {
            // Left -X
            Vector2 newCrossLeftPos = CrosshairLeft.anchoredPosition;
            newCrossLeftPos.x = -amount;
            if (Vector2.Distance(CrosshairLeft.anchoredPosition, newCrossLeftPos) < 0.01f)
                return;

            CrosshairLeft.anchoredPosition = Vector2.Lerp(CrosshairLeft.anchoredPosition, newCrossLeftPos, Time.deltaTime * SpeedCrosshairExpand);
            // Down -Y
            Vector2 newCrossDownPos = CrosshairDown.anchoredPosition;
            newCrossDownPos.y = -amount;
            CrosshairDown.anchoredPosition = Vector2.Lerp(CrosshairDown.anchoredPosition, newCrossDownPos, Time.deltaTime * SpeedCrosshairExpand);
            // Up Y
            Vector2 newCrossUpPos = CrosshairUp.anchoredPosition;
            newCrossUpPos.y = amount;
            CrosshairUp.anchoredPosition = Vector2.Lerp(CrosshairUp.anchoredPosition, newCrossUpPos, Time.deltaTime * SpeedCrosshairExpand);
            // Right Y
            Vector2 newCrossRightPos = CrosshairRight.anchoredPosition;
            newCrossRightPos.x = amount;
            CrosshairRight.anchoredPosition = Vector2.Lerp(CrosshairRight.anchoredPosition, newCrossRightPos, Time.deltaTime * SpeedCrosshairExpand);


        }
        // desactivate UI Weapon ( ammo ammount)
        public void StopUsingWeapon()
        {
            if (UIWeapon)
                UIWeapon.gameObject.SetActive(false);
            if (Ammunition)

                Ammunition.text = "";


        }
    }
}