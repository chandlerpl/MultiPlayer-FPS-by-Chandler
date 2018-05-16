using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AxlPlay
{
    public class WeaponBaseData : MonoBehaviour
    {


        [HideInInspector]
        public Vector3 weaponBaseInitialPosition;

        [HideInInspector]
        public Vector3 weaponBaseInitialLocalEulerAngles;


        [HideInInspector]

        public Quaternion weaponBaseInitialRotation;

        void Awake()
        {
            weaponBaseInitialPosition = transform.localPosition;
            weaponBaseInitialLocalEulerAngles = transform.localEulerAngles;

            weaponBaseInitialRotation = transform.localRotation;
        }

        public void PickupedWeapon(float z)
        {
            weaponBaseInitialPosition = new Vector3(weaponBaseInitialPosition.x, weaponBaseInitialPosition.y, z);

        }
    }
}