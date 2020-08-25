using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using UnityEngine.AI;
using System.Collections;


public class GameManager : MonoBehaviour
{
    private int fieldSize = 2;
    [SerializeField] private GameObject groundGO;

    private int animalCount = 1;
    [SerializeField] private GameObject animalPrefab = null;
    [SerializeField] private GameObject foodPrefab = null;
    private int animalVelocity = 1; 

    private GameObject[] animalsGO;
    private GameObject[] foodGO;
    private float fieldHalfSize;
    private const float FOOD_TIME_REACH = 5.0f;
    private const float GENERATION_TIME_LIMIT = 1.0f;

    private int simulationSpeed = 1;

    [SerializeField] private Canvas startScreen;
    [SerializeField] private Canvas parametersScreen;
    [SerializeField] private Canvas gameScreen;
    [SerializeField] private Slider simulationSpeedSlider;

    private const string SAVE_FILE_NAME = "mysave.bin";

    [SerializeField] private GameObject navMeshBuilder;
    //builder need some frames to bake in runtime and set new surface active
    private const int MAGIC_FRAME_NUMBER = 5;

    private float defaultFixedDeltaTime;

    [SerializeField] private Transform spawnObject;


    // Start is called before the first frame update
    void Start()
    {
        startScreen.enabled = true;
        Button loadGame = startScreen.transform.Find("Load Game").GetComponent<Button>();
        loadGame.interactable = SaveSystem.isSaveFileExists(SAVE_FILE_NAME);
        parametersScreen.enabled = false;
        gameScreen.enabled = false;

        defaultFixedDeltaTime = Time.fixedDeltaTime;

        if (groundGO == null)
        {
            groundGO = GameObject.FindGameObjectWithTag("Ground");
            Assert.IsNotNull(groundGO, "There are no ground gameobject on the scene");
        }

        Assert.IsNotNull(animalPrefab, "There are no animal prefab attached to script");
        Assert.IsNotNull(animalPrefab.GetComponent<AnimalManager>(), "Animal prefab doesn't contain AnimalManager script");

        Assert.IsNotNull(foodPrefab, "There are no food prefab attached to script");
    }

    public void NewGame()
    {
        startScreen.enabled = false;
        parametersScreen.enabled = true;
    }

    public void StartGame()
    {
        parametersScreen.enabled = false;
        gameScreen.enabled = true;

        GenerateField();
        StartCoroutine(StartGameAnimalGeneration());
        
    }

    private IEnumerator StartGameAnimalGeneration()
    {
        for (int i = 0; i < MAGIC_FRAME_NUMBER; ++i)
            yield return new WaitForEndOfFrame();
        GenerateAnimalsRandomly();
        GenerateFoodRandomly();
        InitializeAnimals();
    }

    private IEnumerator LoadGameAnimalGeneration(GameData.AnimalData[] animalData)
    {
        for (int i = 0; i < MAGIC_FRAME_NUMBER; ++i)
            yield return new WaitForEndOfFrame();
        GenerateAnimalsFromSave(animalData);
        GenerateFoodFromSave(animalData);
        InitializeAnimals();
    }

    private void InitializeAnimals()
    {
        for (int i = 0; i < animalsGO.Length; ++i)
            animalsGO[i].GetComponent<AnimalManager>().Initialize(foodGO[i], GenerateFoodPositionForAnimal, simulationSpeedSlider, (float) animalVelocity);
    }

    public void LoadGame()
    {
        startScreen.enabled = false;
        gameScreen.enabled = true;

        GameData data = SaveSystem.LoadGame(SAVE_FILE_NAME);

        fieldSize = data.fieldSize;
        GenerateField();

        animalCount = data.animalCount;
        animalVelocity = data.animalVelocity;
        StartCoroutine(LoadGameAnimalGeneration(data.animals));
        
    }

    public void SaveGame()
    {
        SaveSystem.SaveGame(SAVE_FILE_NAME, fieldSize, animalCount, animalVelocity, animalsGO, foodGO);
    }


    private void GenerateField()
    {
        fieldHalfSize = (float)fieldSize / 2.0f;
        navMeshBuilder.GetComponent<LocalNavMeshBuilder>().m_Size = new Vector3(fieldSize, 6.0f, fieldSize);
        groundGO.transform.localScale = new Vector3(fieldSize, 1, fieldSize);
        groundGO.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
        Camera.main.orthographicSize = fieldHalfSize;
        Camera.main.transform.position = new Vector3(0, Camera.main.transform.position.y, 0);
        Camera.main.transform.eulerAngles = new Vector3(90, 0, 0);
        Camera.main.GetComponent<CameraMove>().maxZoom = fieldHalfSize;

        GameObject.Find("NavMeshBuilder").GetComponent<LocalNavMeshBuilder>().UpdateNavMesh();
    }
    
    private bool CheckOverlapUnit(Vector3 position1, Vector3 position2)
    {
        return Mathf.Abs(position1.x - position2.x) < 1.0f && Mathf.Abs(position1.z - position2.z) < 1.0f;
    }

    private bool CheckOverlapUnits(Vector3 position, GameObject[] gameObjects, GameObject gameObject = null)
    {
        for (int i = 0; i < gameObjects.Length; ++i)
        {
            if (gameObjects[i] != null && gameObjects[i] != gameObject && CheckOverlapUnit(position, gameObjects[i].transform.position))
                return true;
        }
        return false;
    }

    public Vector3? GenerateFoodPositionForAnimal(GameObject animal, GameObject food = null)
    {
        if (animal == null)
            return null;
        Vector3 position, animalPosition = animal.transform.position;
        float time = Time.realtimeSinceStartup;
        float foodRadius = FOOD_TIME_REACH * animalVelocity;
        int idx = 0;
        do
        {
            position = new Vector3(UnityEngine.Random.Range(-fieldHalfSize + 0.5f, fieldHalfSize - 0.5f), 0.0f, UnityEngine.Random.Range(-fieldHalfSize + 0.5f, fieldHalfSize - 0.5f));
            ++idx;
        } while (Time.realtimeSinceStartup - time < GENERATION_TIME_LIMIT
                && ((position - animalPosition).magnitude > foodRadius
                || CheckOverlapUnit(position, animalPosition))
                || CheckOverlapUnits(position, foodGO, food));
        if (Time.realtimeSinceStartup - time >= GENERATION_TIME_LIMIT)
        {
            Debug.Log("can't generate food position for animal " + animal.name);
            Debug.Log("Tried " + idx + " times");
            return null;
        }
        return position;
    }

    private void GenerateFoodRandomly()
    {
        foodGO = new GameObject[animalCount];
        for (int i = 0; i < animalCount; ++i)
        {
            Vector3? position = GenerateFoodPositionForAnimal(animalsGO[i]);
            if (position == null)
                continue;
            foodGO[i] = Instantiate(foodPrefab, position.Value, Quaternion.identity, spawnObject) as GameObject;
            foodGO[i].name = "Food_" + i.ToString();
            foodGO[i].GetComponent<Renderer>().material.SetColor("_Color", animalsGO[i].GetComponent<Renderer>().material.GetColor("_Color"));
        }
    }

    private void GenerateAnimalsRandomly()
    {
        animalsGO = new GameObject[animalCount];
        for (int i = 0; i < animalCount; ++i)
        {
            Vector3 position;
            int idx = 0;
            float time = Time.realtimeSinceStartup;
            do
            {
                position = new Vector3(UnityEngine.Random.Range(-fieldHalfSize + 0.5f, fieldHalfSize - 0.5f), 0.0f, UnityEngine.Random.Range(-fieldHalfSize + 0.5f, fieldHalfSize - 0.5f));
                ++idx;
            } while (Time.realtimeSinceStartup - time < GENERATION_TIME_LIMIT
                    && CheckOverlapUnits(position, animalsGO));
            if (Time.time - time >= GENERATION_TIME_LIMIT)
            {
                Debug.Log("can't generate position for animal " + i);
                Debug.Log("Tried " + idx + " times");
                continue;
            }
            Color color = new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), 1.0f);
            animalsGO[i] = Instantiate(animalPrefab, position, Quaternion.identity, spawnObject) as GameObject;
            animalsGO[i].name = "Animal_" + i.ToString();
            animalsGO[i].GetComponent<Renderer>().material.SetColor("_Color", color);
        }
    }

    private void GenerateAnimalsFromSave(GameData.AnimalData[] animalData)
    {
        animalsGO = new GameObject[animalData.Length];
        for (int i = 0; i < animalData.Length; ++i)
        {
            //Empty animal - some error in save
            if (animalData[i].position.Length == 0)
            {
                Debug.Log("Animal " + animalData[i].idx.ToString() + "is empty in save file");
                continue;
            }

            int index = animalData[i].idx;

            Vector3 position = new Vector3(animalData[i].position[0], animalData[i].position[1], animalData[i].position[2]);
            Color color = new Color(animalData[i].color[0], animalData[i].color[1], animalData[i].color[2], animalData[i].color[3]);
            animalsGO[index] = Instantiate(animalPrefab, position, Quaternion.identity) as GameObject;
            animalsGO[index].name = "Animal_" + index.ToString();
            animalsGO[index].GetComponent<Renderer>().material.SetColor("_Color", color);
        }
    }

    private void GenerateFoodFromSave(GameData.AnimalData[] animalData)
    {
        foodGO = new GameObject[animalData.Length];
        for (int i = 0; i < animalData.Length; ++i)
        {
            int index = animalData[i].idx;
            Color color = new Color(animalData[i].color[0], animalData[i].color[1], animalData[i].color[2], animalData[i].color[3]);
            Vector3? foodPosition;
            if (animalData[i].foodPosition.Length != 0)
                foodPosition = new Vector3(animalData[i].foodPosition[0], animalData[i].foodPosition[1], animalData[i].foodPosition[2]);
            //Save was when animal ate food and it became enabled
            else
                foodPosition = GenerateFoodPositionForAnimal(animalsGO[i]);
            if (foodPosition != null)
            {
                foodGO[index] = Instantiate(foodPrefab, foodPosition.Value, Quaternion.identity) as GameObject;
                foodGO[index].name = "Food_" + index.ToString();
                foodGO[index].GetComponent<Renderer>().material.SetColor("_Color", animalsGO[i].GetComponent<Renderer>().material.GetColor("_Color"));
            }
        }
    }

    public void SetFieldSize(float value)
    {
        fieldSize = (int)value;
    }

    public void SetAnimalCount(float value)
    {
        animalCount = (int)value;
    }

    public void SetAnimalVelocity(float value)
    {
        animalVelocity = (int)value;
    }

    public void SetSimulationSpeed(float value)
    {
        simulationSpeed = (int)value;
        Time.timeScale = value;
        Time.fixedDeltaTime = defaultFixedDeltaTime * value;
    }
}
