using Unity.VisualScripting;
using UnityEngine;

public class AICar : MonoBehaviour
{
    public float drivingSpeed = 30;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.y < -40)
        {
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        RunCar();
    }

    private void RunCar()
    {
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 forwardVelocity = transform.right * drivingSpeed;
        rb.linearVelocity = new Vector3(forwardVelocity.x, currentVelocity.y, 0f);
        rb.angularVelocity = Vector3.zero;
    }
}
