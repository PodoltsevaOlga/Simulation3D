using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public int fieldSize;
    public int animalCount;
    public int animalVelocity;

    [System.Serializable]
    public struct AnimalData
    {
        public float[] position;
        public float[] color;
        public float[] velocity;
        public float[] foodPosition;
        public int idx;
    }

    public AnimalData[] animals;

    public GameData(int _fieldSize, int _animalCount, int _animalVelocity, GameObject[] _animalsGO, GameObject[] _foodGO)
    {
        fieldSize = _fieldSize;
        animalCount = _animalCount;
        animalVelocity = _animalVelocity;
        animals = new AnimalData[_animalsGO.Length];
        for(int i = 0; i < _animalsGO.Length; ++i)
        {
            if (_animalsGO[i] == null)
                continue;

            Vector3 position = _animalsGO[i].transform.position;
            animals[i].position = new float[3];
            animals[i].position[0] = position.x;
            animals[i].position[1] = position.y;
            animals[i].position[2] = position.z;

            Color color = _animalsGO[i].GetComponent<Renderer>().material.GetColor("_Color");
            animals[i].color = new float[4];
            animals[i].color[0] = color.r;
            animals[i].color[1] = color.g;
            animals[i].color[2] = color.b;
            animals[i].color[3] = color.a;

            if (_animalsGO[i].GetComponent<Rigidbody>() != null)
            {
                Vector3 velocity = _animalsGO[i].GetComponent<Rigidbody>().velocity;
                animals[i].velocity = new float[3];
                animals[i].velocity[0] = velocity.x;
                animals[i].velocity[1] = velocity.y;
                animals[i].velocity[2] = velocity.z;
            }
            

            if (_foodGO[i] != null && _foodGO[i].activeSelf)
            {
                Vector3 foodPosition = _foodGO[i].transform.position;
                animals[i].foodPosition = new float[3];
                animals[i].foodPosition[0] = foodPosition.x;
                animals[i].foodPosition[1] = foodPosition.y;
                animals[i].foodPosition[2] = foodPosition.z;
            }

            animals[i].idx = i;
        }
    }
}
