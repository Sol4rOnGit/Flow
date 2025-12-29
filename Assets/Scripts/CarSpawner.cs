using System.Collections;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] private GameObject[] lanes;
    [SerializeField] private float maxDelay;
    [SerializeField] private float minDelay;
    [SerializeField] private int percentageChanceOfDoubleCars;
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

    int currentBitmask;
    int previousBitmask;

    private void SpawnCar()
    {
        currentBitmask = 0;

        int randomLaneIndex = Random.Range(0, lanes.Length);
        Transform spawnposition = lanes[randomLaneIndex].transform;

        int randomCarIndex = Random.Range(0, cars.Length);
        GameObject currentCar = cars[randomCarIndex].gameObject;

        GameObject carInstance = Instantiate(currentCar, spawnposition.position, spawnposition.rotation);

        currentBitmask |= 1 << randomLaneIndex;

        //Small chance of another car spawning on another lane
        int randomInt = Random.Range(1, 100);
        if (randomInt < (percentageChanceOfDoubleCars + 1))
        {
            int randomLaneIndex2 = randomLaneIndex;
            int modifiedCurrentBitmask;
            do
            {
                randomLaneIndex2 = Random.Range(0, lanes.Length);
                modifiedCurrentBitmask = currentBitmask;
                modifiedCurrentBitmask |= 1 << randomLaneIndex2;
                //currentBitmask |= 1 << randomLaneIndex2; corrupts the bitmask
            } while
            (
                randomLaneIndex2 == randomLaneIndex || (previousBitmask == 3 && modifiedCurrentBitmask == 6) || (previousBitmask == 6 && modifiedCurrentBitmask == 3)
            );

            currentBitmask = modifiedCurrentBitmask;

            Transform spawnposition2 = lanes[randomLaneIndex2].transform;

            int randomCarIndex2 = Random.Range(0, cars.Length);
            GameObject currentCar2 = cars[randomCarIndex2].gameObject;

            GameObject carInstance2 = Instantiate(currentCar2, spawnposition2.position, spawnposition2.rotation);
        }

        previousBitmask = currentBitmask;
    }
}

public class TrafficCarData
{
    public float positionX;
    public int laneIndex;
    public float speed;
}