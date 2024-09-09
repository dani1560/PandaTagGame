using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;


namespace EpicToonFX
{
    public class ETFXFireProjectile : MonoBehaviour
    {
        RaycastHit hit;
        public GameObject[] projectiles;
        public GameObject Bulllt;
        public Transform spawnPosition;
        public int currentProjectile = 0;
        public float speed = 2000;

        //MyGUI _GUI;
       // ETFXButtonScript selectedProjectileButton;
        void Start()
        {
            //selectedProjectileButton = GameObject.Find("Button").GetComponent<ETFXButtonScript>();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                nextEffect();
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                nextEffect();
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                previousEffect();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                previousEffect();
            }
            Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
            Vector3 lookAt = ray.direction * 1000 + this.transform.position;
            lookAt.y += 8f;
            this.transform.LookAt(lookAt);
            //if (Input.GetMouseButtonDown(0))
            //{

            //    if (!EventSystem.current.IsPointerOverGameObject())
            //    {
            //        if (Physics.Raycast(transform.position, ray.direction, out hit, 1000f))
            //        {
            //            GameObject projectile = Instantiate(projectiles[currentProjectile], spawnPosition.position, Quaternion.identity) as GameObject;
            //            projectile.transform.LookAt(hit.point);
            //            projectile.GetComponent<Rigidbody>().AddForce(projectile.transform.forward * speed);
            //            projectile.GetComponent<ETFXProjectileScript>().impactNormal = hit.normal;
            //        }
            //    }

            //}
            Debug.DrawRay(Camera.main.ScreenPointToRay(Input.mousePosition).origin, Camera.main.ScreenPointToRay(Input.mousePosition).direction * 100, Color.yellow);
        }

        public void nextEffect()
        {
            if (currentProjectile < projectiles.Length - 1)
                currentProjectile++;
            else
                currentProjectile = 0;
           // selectedProjectileButton.getProjectileNames();
        }

        public void previousEffect()
        {
            if (currentProjectile > 0)
                currentProjectile--;
            else
                currentProjectile = projectiles.Length - 1;
            //selectedProjectileButton.getProjectileNames();
        }
        public void Attack()
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
            Vector3 lookAt = ray.direction * 1000 + this.transform.position;
            lookAt.y += 8f;
            this.transform.LookAt(lookAt);
            if (Physics.Raycast(transform.position, ray.direction, out hit, 1000f))
            {
                GameObject projectile = Instantiate(Bulllt, spawnPosition.position, Quaternion.identity) as GameObject;
                projectile.transform.LookAt(hit.point);
                projectile.GetComponent<Rigidbody>().AddForce(projectile.transform.forward * speed);
                projectile.GetComponent<ETFXProjectileScript>().impactNormal = hit.normal;
            }
        }
        public void EnemyAttack()
        {
                GameObject projectile = Instantiate(Bulllt, spawnPosition.position,spawnPosition.rotation) as GameObject;
                projectile.transform.LookAt(spawnPosition);
                projectile.GetComponent<Rigidbody>().AddForce(projectile.transform.forward * speed);
                projectile.GetComponent<ETFXProjectileScript>().impactNormal = hit.normal;
        }
        public void AdjustSpeed(float newSpeed)
        {
            speed = newSpeed;
        }
    }
}