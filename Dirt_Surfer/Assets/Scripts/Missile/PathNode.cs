using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    private Grid<PathNode> grid;
    public int x;
    public int y;
    public int z;

    public int gCost;
    public int hCost;
    public int fCost;

    public PathNode cameFromNode;
    public PathNode(Grid<PathNode> grid, int x, int y, int z){
        this.grid = grid;
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public override string ToString(){
        return "";
        //return x + "," + y +"," + z;
    }
    public void CalculateFCost(){
        fCost = gCost + hCost;
    }
}
