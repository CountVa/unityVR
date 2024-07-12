using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;
using UnityEngine.InputSystem;

public class SliceObject : MonoBehaviour
{
    
    public Transform startSlicePoint;
    public Transform endSlicePoint;
    public LayerMask sliceableLayer;

    public Material crossSectionMaterial;
    public float cutForce = 2000;

    public VelocityEstimator velocityEstimator;

    public AudioSource otherAudioSource;

    void Start()
    {
        
    }

    // проверка коллизии с объектом
    void FixedUpdate()
    {
        bool hasHit = Physics.Linecast(
            startSlicePoint.position, 
            endSlicePoint.position, 
            out RaycastHit hit, 
            sliceableLayer
        );
        if(hasHit)
        {
            GameObject target = hit.transform.gameObject;
            Slice(target);
        }
    }

    // Свойства частей разрубленного объекта
    public void SetupSlicedComponent(GameObject slicedObject)
    {
        Rigidbody rb = slicedObject.AddComponent<Rigidbody>();

        MeshCollider collider = slicedObject.AddComponent<MeshCollider>();

        collider.convex = true;

        rb.AddExplosionForce(cutForce, slicedObject.transform.position, 10);
    }

    // Разрубание объекта на две части
    public void Slice(GameObject target)
    {
        Vector3 velocity = velocityEstimator.GetVelocityEstimate();
        Vector3 planeNormal = Vector3.Cross(endSlicePoint.position - startSlicePoint.position, velocity);
        
        planeNormal.Normalize();

        SlicedHull hull = target.Slice(endSlicePoint.position, planeNormal);

        if(hull != null)
        {
            GameObject upperHull = hull.CreateUpperHull(target, crossSectionMaterial);
            SetupSlicedComponent(upperHull);

            GameObject lowerHull = hull.CreateLowerHull(target, crossSectionMaterial);
            SetupSlicedComponent(lowerHull);

            Destroy(target);
            audioSource.Play();
        }
    }
}
