using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEngine;

public class Road : AbstractBuildingType
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // public override void SetNeighbourForPosition(Vector3Int position, Vector3Int neighbourPosition, AbstractBuildingType neighbour)
    // {
    //     base.SetNeighbourForPosition(position, neighbourPosition, neighbour);
    //     /*var numberOfRoadNeighbours = 0;
    //     foreach (var positionWithBuilding in neighbourDatasForPositions[position].NeighboursForGridPositions) 
    //     {
    //         if (positionWithBuilding.Value is Road) {
    //             numberOfRoadNeighbours++;
    //         }
    //     }

    //     Destroy(buildings[position]);
    //     buildings.Remove(position);
    //     GameObject prefabToInst;
    //     switch(numberOfRoadNeighbours) {
    //         case 0:
    //             prefabToInst = twoWayStraight;
    //             break;
    //         case 1:
    //             prefabToInst = twoWayStraight;
    //             break;
    //         case 2:
    //             prefabToInst = twoWayCurvy;
    //             break;
    //         case 3:
    //             prefabToInst = threeWay;
    //             break;
    //         case 4:
    //             prefabToInst = fourWay;
    //             break;
    //         default:
    //             throw new Exception("Anyád picsája");
    //     }

    //     var buil = Instantiate(prefabToInst);
    //     buil.transform.position = position; 
    //     buildings.Add(position, buil);*/
    // }
}
