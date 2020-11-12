﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Pathfinding;

public class DragObject : MonoBehaviour
{
    [SerializeField]
    private GameObject villagerObject;

    public Villager villager;

    private Vector3 mOffset;
    private float mZCoord;

    [SerializeField]
    private Tilemap uniqueMap;

    [SerializeField]
    private TileBase farmTile;

    private FarmTile farmTileObject;

    [SerializeField]
    private GameObject farmMap;

    [SerializeField]
    private int farmFood;

    [SerializeField]
    private GameObject waypoint;

    private GameObject currentFarm;

    [SerializeField]
    private GameObject[] campFire;

    void Awake()
    {
        villager = GetComponent<Villager>();
    }

    void OnMouseDown()
    {
        mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        mOffset = gameObject.transform.position - GetMouseWorldPos();        

        // If the villager is interrupted during transportation, give the farm its food back and set the villager to idle
        if (villager.IsFarming)
        {
            // Destroy food in the characters hands
                // Fade transperency to 0 then destroy
            // currentFarm.GetComponent<FarmTileScript>().farmIncome = farmFood;
            // farmFood = 0;
            villager.ChangeState(Villager.States.Idle);            
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;

        mousePoint.z = mZCoord;

        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    void OnMouseDrag()
    {
        transform.position = GetMouseWorldPos() + mOffset;
    }

    void OnMouseUp()
    {
        transform.position = UtilityHelper.SnapToGrid(transform.position);

        TileBase tileDroppedOn = uniqueMap.GetTile(new Vector3Int((int)transform.position.x, (int)transform.position.y, 0));

        FarmTile currentTile = uniqueMap.GetTile(new Vector3Int((int)transform.position.x, (int)transform.position.y, 0)) as FarmTile;



        if (tileDroppedOn == farmTile)
        {           
            // Set villager current task to "Gathering food from a farm"
            villager.ChangeState(Villager.States.Farming);
            // Play gathering food animation
            // Instantiate food in the characters hands            

            // Debug.Log(currentTile.farm.GetComponent<FarmTileScript>().farmIncome); // This gives 0 not sure why

            // Bad loop that seems like it could be replaced, need a way to get whatever object is at that position
            // instead of looping through every object on that layer checking the position
            foreach (Transform farm in farmMap.transform)
            {
                if (transform.position == farm.transform.position)
                {
                    currentFarm = farm.gameObject; 

                    // Store the food amount incase we need to revert it
                    farmFood = farm.GetComponent<FarmTileScript>().farmIncome;

                    // Set farm income to 0
                    farm.GetComponent<FarmTileScript>().farmIncome = 0;
                }
            }

            // Set villager waypoint to their house or food storage area
            waypoint.transform.position = new Vector3(500, 500, 0);

            int maxFood = 100; // This will be a global

            if ((villager.villagerFood + farmFood) > 100)
            {
                farmFood = farmFood - (maxFood - villager.villagerFood);
                villager.villagerFood = maxFood;
            }
            else
            {
                villager.villagerFood += farmFood;
                farmFood = 0;
            }
        }
    }

    void FixedUpdate()
    {
        if (villager.IsIdle)
        {
            // Select a random location to walk to (start a task if something is within range), else wait a random amount of seconds (looking around) and then move to a new spot
            
            /*
                If a villager is idle but is holding more than 0 food
                Resume pathing to food destination to drop it off
            */
        }
        else if (villager.IsFarming)
        {
            Debug.Log(waypoint.transform.position == new Vector3(0, 0, 0));
            Debug.Log(Vector3.Distance(gameObject.transform.localPosition, waypoint.transform.localPosition) < 1);
            Debug.Log(villager.villagerFood > 0);

            if ((Vector3.Distance(gameObject.transform.localPosition, waypoint.transform.localPosition) < 1) && villager.villagerFood > 0) // A house tile
            {
                Debug.Log("Collected: " + villager.villagerFood);
                
                // Destroy food in the characters hands
                GlobalVariables.Variables.playerMoney += villager.villagerFood;
                villager.villagerFood = 0;
                GlobalVariables.Variables.playerMoneyText.text = "Money: " + GlobalVariables.Variables.playerMoney; // Needs better system to update whenever money amount changes
                
                villager.ChangeState(Villager.States.Idle);
            }
        }
        else if (villager.IsCampfire)
        {
            // Find nearest campfire and gather for warmth (stop when within a certain radius and play hand warming animation)
            for (int i = 0; i < campFire.Length; i++)
            {  
                if ((Vector3.Distance(campFire[i].transform.localPosition, gameObject.transform.position) < 2.5f))
                {
                    waypoint.transform.position = gameObject.transform.position;
                }
            }
        }
        else if (villager.IsSwimming)
        {
            // Villager swims to the closest shore block (even if it strands them)
        }
        else
        {
            villager.ChangeState(Villager.States.Idle);
        }




        /*******************************************************/
        // Debug keys
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log(villager.state);
        }

        if (Input.GetKeyDown("space"))
        {
            villager.ChangeState(Villager.States.Campfire);

            float closestCampFireDist = 1000000;
            int closestCampFire = 0;

            for (int i = 0; i < campFire.Length; i++)
            {  
                float dist = Vector3.Distance(campFire[i].transform.position, gameObject.transform.position);

                if (dist < closestCampFireDist)
                {
                    closestCampFireDist = dist;
                    closestCampFire = i;
                }
            }

            waypoint.transform.position = campFire[closestCampFire].transform.position;
        }

    }

    

}
