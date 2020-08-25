using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class AnimalManager : MonoBehaviour
{
    private GameObject food;

    [SerializeField] private GameObject particleSystemPrefab;
    private List<GameObject> particlesPool = new List<GameObject>();

    private NavMeshAgent agent;

    public delegate Vector3? OnFoodUpdateDelegate(GameObject animal, GameObject food);
    private event OnFoodUpdateDelegate FoodUpdateDelegate;

    private Slider simulationSpeed;

    private float speed;

    private bool noFood = false;

    private void Start()
    {
        simulationSpeed.onValueChanged.AddListener(ChangeSpeed);
        
        Assert.IsNotNull(particleSystemPrefab);
        Assert.IsNotNull(particleSystemPrefab.GetComponent<ParticleSystem>());
    }

    private void OnDisable()
    {
        simulationSpeed.onValueChanged.RemoveListener(ChangeSpeed);
    }

    public void Initialize(GameObject _food, OnFoodUpdateDelegate _foodUpdate, Slider _simulationSpeed, float _animalSpeed)
    {
        food = _food;
        FoodUpdateDelegate = _foodUpdate;
        simulationSpeed = _simulationSpeed;
        speed = _animalSpeed;
        agent = GetComponent<NavMeshAgent>();
        Assert.IsNotNull(agent, "Animal " + name + " doesn't have NavMeshAgent and cannot move");
        agent.speed = _animalSpeed;
        if (food != null)
            agent.SetDestination(food.transform.position);
        else
        {
            agent.isStopped = true;
            noFood = true;
        }
            
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == food)
        { 
            GameObject particle = GetParticleFromPool();
            particle.transform.position = food.transform.position;
            particle.SetActive(true);
            particle.GetComponent<ParticleSystem>().Clear();
            particle.GetComponent<ParticleSystem>().Play();

            food.SetActive(false);
            Vector3? foodPosition = FoodUpdateDelegate(gameObject, food);
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            if (foodPosition != null)
            {
                agent.SetDestination(foodPosition.Value);
                agent.isStopped = false;
                food.transform.position = foodPosition.Value;
                food.SetActive(true);
            }
            else
                noFood = true;
        }
    }



    public void ChangeSpeed(float value)
    {
        if (!noFood)
        {
            if ((int)value == 0)
            {
                agent.isStopped = true;
                transform.position = new Vector3(transform.position.x, 0.0f, transform.position.z);
            }
                
            else
            {
                agent.isStopped = false;
                agent.SetDestination(food.transform.position);
            }
        }

    }

    private GameObject GetParticleFromPool()
    {
        foreach(GameObject particle in particlesPool)
        {
            ParticleSystem system = particle.GetComponent<ParticleSystem>();
            if (!particle.activeSelf)
                return particle;
        }
        GameObject newParticle = Instantiate(particleSystemPrefab, Vector3.zero, Quaternion.identity, transform);
        particlesPool.Add(newParticle);
        return newParticle;
    }

    private void OnDestroy()
    {
        foreach (GameObject particle in particlesPool)
            Destroy(particle);
    }
}
