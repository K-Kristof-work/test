using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Model;
using Assets.Model.Data;

public class GameModel : MonoBehaviour
{
    private GameData gameData;
    private CityLogic cityLogic;

    private void Awake()
    {
        gameData = new GameData();
        cityLogic = new CityLogic(gameData);
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
}
