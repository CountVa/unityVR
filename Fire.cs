

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FireBullet : MonoBehaviour
{
    
    public GameObject bullet;
    public Transform spawnPoint;
    public float fireSpeed = 20;
    public float fireRate = 0.1f; // Время между выстрелами

    public AudioClip shootSound; // Звук выстрела
    public AudioClip reloadSound; 
    private AudioSource audioSource; // Компонент для проигрывания звука

    private bool canFire = true;
    private bool canHold = true;
    private Coroutine fireCoroutine;
    private int shotCounter = 0;

    private void Start()
    {
        // Слушатель на активацию и деактивацию стрельбы
        XRGrabInteractable grabbable = GetComponent<XRGrabInteractable>();
        grabbable.activated.AddListener(Fire);
        grabbable.deactivated.AddListener(StopFiring);       
    }

    void Update()
    {
        // Проверка на удержание оружия
        if (grabInteractable && (directInteractorRight || directInteractorLeft))
        {
            if (grabInteractable.isSelected && 
            (grabInteractable.selectingInteractor == directInteractorRight || 
            grabInteractable.selectingInteractor == directInteractorLeft))
            {
                canHold = true;             
            }
            else 
            {
                canHold = false;
            }
        }
    }

    //Функция стрельбы
    private IEnumerator FireCoroutine()
    {
        while (canFire && canHold)
        {
            GameObject spawnedBullet = Instantiate(bullet);
            spawnedBullet.transform.position = spawnPoint.position;
            spawnedBullet.GetComponent<Rigidbody>().velocity = spawnPoint.forward * fireSpeed;

            shotCounter++;
            if (shotCounter >= maxShotCount)
            {
                canFire = false;
            }

            audioSource.PlayOneShot(shootSound); 
            
            if (grabInteractable.selectingInteractor == directInteractorRight)
            {
                directInteractorRight.xrController.SendHapticImpulse(intensity, duration);
            }
            else if (grabInteractable.selectingInteractor == directInteractorLeft)
            {
                directInteractorLeft.xrController.SendHapticImpulse(intensity, duration);
            }

            yield return new WaitForSeconds(fireRate);
        }
    }

    // Вызов стрельбы
    public void Fire(ActivateEventArgs arg)
    {
        audioSource = GetComponent<AudioSource>(); 

        if (!canFire)
        {
            return;
        }

        fireCoroutine = StartCoroutine(FireCoroutine());
    }

    // Остановка стрельбы
    public void StopFiring(DeactivateEventArgs arg)
    {
        if (fireCoroutine != null)
        {
            StopCoroutine(fireCoroutine);
        }
    }

    // перезарядка при коллизии с объектом
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("EnergicCube"))
        {
            interactable = collision.gameObject.GetComponent<XRBaseInteractable>();
         
            if (interactable && !isInteracting)
            {
                Destroy(collision.gameObject); 
                audioSource.PlayOneShot(reloadSound);
                
                shotCounter = 0;
                canFire = true;
            }
        }
    }   
}