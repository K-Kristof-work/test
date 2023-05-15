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
    //gamedata tests

    public delegate void Func(Field field);
    private GameData gameData;
    private CityLogic cityLogic;

    [SetUp]
    public void Init()
    {
        gameData = new GameData();
        gameData.SetUpGrid(20,20);
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

	[Test]
	public void TestInitialLoans()
	{
		Assert.AreEqual(0, gameData.loans);
	}

	[Test]
	public void TestGridWidthAndHeight()
	{
		Assert.AreEqual(20, gameData.gridWidth);
		Assert.AreEqual(20, gameData.gridHeight);
	}

	[Test]
	public void TestChangeZoneType()
	{
		gameData.ChangeZoneType(0, 0, ZoneType.Residential, 1);
		Assert.AreEqual(ZoneType.Residential, gameData.grid[0][0].zone.zone_type);
	}

	[Test]
	public void TestIsFieldValid_WithInvalidValues_ReturnsFalse()
	{
		Assert.IsFalse(gameData.isFieldValid(-1, 0));
		Assert.IsFalse(gameData.isFieldValid(0, -1));
		Assert.IsFalse(gameData.isFieldValid(20, 0));
		Assert.IsFalse(gameData.isFieldValid(0, 20));
	}

	[Test]
	public void TestIsFieldValid_WithValidValues_ReturnsTrue()
	{
		Assert.IsTrue(gameData.isFieldValid(0, 0));
		Assert.IsTrue(gameData.isFieldValid(19, 19));
	}

	// buildingplacer tests

	private BuildingPlacer buildingPlacer;

	[SetUp]
	public void Setup()
	{
		gameData = new GameData();
		gameData.SetUpGrid(20, 20);

		Dictionary<ZoneType, List<Vec2>> availableBuildingSizes = new Dictionary<ZoneType, List<Vec2>>()
			{
				{ ZoneType.Residential, new List<Vec2>() { new Vec2(1, 1), new Vec2(2, 2) } },
				{ ZoneType.Commercial, new List<Vec2>() { new Vec2(1, 1), new Vec2(2, 2), new Vec2(3, 3) } },
				{ ZoneType.Industrial, new List<Vec2>() { new Vec2(1, 1), new Vec2(2, 2), new Vec2(3, 3), new Vec2(4, 4) } }
			};

		buildingPlacer = new BuildingPlacer(gameData, availableBuildingSizes);
	}

	[Test]
	public void TestPlaceBuilding()
	{
		buildingPlacer.PlaceBuilding(5, 5, ZoneType.Residential, BlockType.House, 1);

		Assert.AreEqual(BlockType.House, gameData.grid[5][5].block.type);
	}

	[Test]
	public void TestPlaceBuildingByUser()
	{
		buildingPlacer.PlaceBuildingByUser(new Vec2(5, 5), BlockType.PoliceStation);

		Assert.AreEqual(BlockType.PoliceStation, gameData.grid[5][5].block.type);
	}

	[Test]
	public void TestGetSizeForBuildingType()
	{
		Vec2 size = buildingPlacer.GetSizeForBuildingType(BlockType.PoliceStation);

		Assert.AreEqual(new Vec2(1, 1), size);
	}


}
