using System.Collections;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] private GameObject[] lanes;
    [SerializeField] private float maxDelay;
    [SerializeField] private float minDelay;
    [SerializeField] private GameObject[] cars;

    void Start()
    {
        StartCoroutine(SpawnCars());
    }

    void Update()
    {

    }


    private IEnumerator SpawnCars()
    {
        while (true)
        {
            float currentDelay = Random.Range(minDelay, maxDelay);

            SpawnCar();

            WaitForSeconds waitForSeconds = new WaitForSeconds(currentDelay);

            yield return waitForSeconds;
        }
    }

    private void SpawnCar()
    {
        int randomLaneIndex = Random.Range(0, lanes.Length);
        Transform spawnposition = lanes[randomLaneIndex].transform;

        int randomCarIndex = Random.Range(0, cars.Length);
        GameObject currentCar = cars[randomCarIndex].gameObject;

        GameObject carInstance = Instantiate(currentCar, spawnposition.position, spawnposition.rotation);

        //Small chance of another car spawning on another lane
        int randomInt = Random.Range(1, 100);
        if(randomInt < 20) //1 in 5 cars
        {
            int randomLaneIndex2 = randomLaneIndex;
            do
            {
                randomLaneIndex2 = Random.Range(0, lanes.Length);
            } while (randomLaneIndex2 == randomLaneIndex);

            Transform spawnposition2 = lanes[randomLaneIndex2].transform;

            int randomCarIndex2 = Random.Range(0, cars.Length);
            GameObject currentCar2 = cars[randomCarIndex2].gameObject;

            GameObject carInstance2 = Instantiate(currentCar2, spawnposition2.position, spawnposition2.rotation);
        }
    }
}

public class TrafficCarData
{
    public float positionX;
    public int laneIndex;
    public float speed;
}