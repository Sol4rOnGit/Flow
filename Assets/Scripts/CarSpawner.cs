using System.Collections;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CarSpawner : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] private GameObject[] lanes;
    [SerializeField] private float maxDelay;
    [SerializeField] private float minDelay;
    [SerializeField] private int percentageChanceOfDoubleCars;
    [SerializeField] private GameObject[] cars;
    [SerializeField] private bool isStrictDriving = false;

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
        //Reset bitmask
        currentBitmask = 0;

        //Select a Random Car
        int randomCarIndex = Random.Range(0, cars.Length);
        GameObject currentCar = cars[randomCarIndex].gameObject;

        //Select a random lane (that isn't 010)
        int randomLaneIndex = 0;
        Transform spawnposition = null;

        while (currentBitmask == 2 || currentBitmask == 0)
        {
            //Select a random Lane
            randomLaneIndex = Random.Range(0, lanes.Length);
            spawnposition = lanes[randomLaneIndex].transform;

            //Set Bitmask
            currentBitmask |= 1 << randomLaneIndex;
        }

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

            carInstance2.GetComponent<AICar>().isChaosAllowed = isStrictDriving ? false : true;
        }

        //Spawn the random car
        GameObject carInstance = Instantiate(currentCar, spawnposition.position, spawnposition.rotation);

        carInstance.GetComponent<AICar>().isChaosAllowed = isStrictDriving ? false : true;
        previousBitmask = currentBitmask;
    }
}

public class TrafficCarData
{
    public float positionX;
    public int laneIndex;
    public float speed;
}