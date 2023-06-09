using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Model;
using Assets.Model.Data;

public class GameModel : MonoBehaviour
{
    public GameData gameData;
    public CityLogic cityLogic;
    public PlayerAction playerAction;

    public static GameModel instance;

    private void Awake()
    {
        instance = this;
        gameData = new GameData();
        cityLogic = new CityLogic(gameData);
        playerAction = new PlayerAction(gameData);
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
