using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class AICar : MonoBehaviour
{
    public float drivingSpeed = 30;
    public bool isChaosAllowed = true;
    public bool isReversed = false;

    private Rigidbody rb;
    private bool isChaosMode = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        if (isReversed)
        {
            //drivingSpeed *= -1;
            transform.Rotate(0f, 180f, 0f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Destroy if below
        if(transform.position.y < -40) {
            Destroy(gameObject);
        }

        //Destroy if out of bounds
        if (isReversed) { if (transform.position.x < 45) { Destroy(gameObject); } }
        else { if (transform.position.x > 2745) { Destroy(gameObject); } }
    }

    private void FixedUpdate()
    {
        RunCar();
    }

    private void RunCar()
    {
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 forwardVelocity = transform.right * drivingSpeed;

        if (isChaosMode)
        {
            rb.linearVelocity = new Vector3(forwardVelocity.x, currentVelocity.y, currentVelocity.z);  //Mathf.Lerp(currentVelocity.z, 0f, Time.deltaTime))
            return;
        }

        rb.linearVelocity = new Vector3(forwardVelocity.x, currentVelocity.y, 0f);  //Mathf.Lerp(currentVelocity.z, 0f, Time.deltaTime))
        rb.angularVelocity = Vector3.zero;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!isChaosAllowed)
        {
            return;
        }

        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Car")){
            StartCoroutine(onCrash());
        }

    }

    private IEnumerator onCrash()
    {
        WaitForSeconds cooldown = new WaitForSeconds(5.0f);
        isChaosMode = true;
        rb.constraints = RigidbodyConstraints.None;
        yield return cooldown;

        Destroy(gameObject);
    }
}
