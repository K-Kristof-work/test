using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assets.Model.Data;
using System;
using Assets.Model;

public class UnitTest
{
    public delegate void Func(Field field);
    private GameData gameData;
    private CityLogic cityLogic;

    [SetUp]
    public void Init()
    {
        gameData = new GameData();
        gameData.SetUpGrid(50,50);
        cityLogic = new CityLogic(gameData);
    }

    private void LoopGrid(Func f)
    {
        for(int i = 0; i < gameData.gridWidth; i++)
        {
            for(int j = 0; j < gameData.gridHeight; j++)
            {
                f(gameData.grid[i][j]);
            }
        }
    }

    // A Test behaves as an ordinary method
    [Test]
    public void UnitTestSimplePasses()
    {
        Assert.AreEqual(1, 1);
        // Use the Assert class to test conditions
    }

    [Test]
    public void FindIncomingRoad()
    {
        LoopGrid((Field field) =>
        {
            if(field.zone.zone_type == ZoneType.IncomingRoad)
            {
                Assert.Pass();
            }
        });

        Assert.Fail();
    }

    [Test]
    public void CheckStartingBalance()
    {
        Assert.AreEqual(10000, gameData.balance);
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator UnitTestWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
