using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    private const int STRAIGHT = 10;
    private const int DIAGONAL = 14;
    private Grid<PathNode> grid;
    private List<PathNode> openList;
    private List<PathNode> closeList;

    public static PathFinding Instance { get; private set; }
    public PathFinding(int width, int height, int length, int cellsize,Vector3 originPoint) {
        Instance = this;
        grid = new Grid<PathNode>(width, height, length, 10f, originPoint, (Grid<PathNode> g, int x, int y, int z) => new PathNode(g, x, y, z));
    }

    public Grid<PathNode> GetGrid() {
        return grid;
    }
    public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition) {
        grid.GetGridPosition(startWorldPosition, out int startX, out int startY, out int startZ);
        grid.GetGridPosition(startWorldPosition, out int endX, out int endY, out int endZ);

        List<PathNode> path = FindPath(startX, startY, startZ, endX, endY, endZ);
        if (path == null) {
            return null;
        }
        else {
            List<Vector3> vectorPath = new List<Vector3>();
            foreach (PathNode pathNode in path) {
                vectorPath.Add(new Vector3(pathNode.x, pathNode.y, pathNode.z) * grid.getCellSize() + Vector3.one * grid.getCellSize() * .5f);
            }
            return vectorPath;
        }
        
    }
    public List<PathNode> FindPath(int startX, int startY, int startZ, int endX, int endY, int endZ) {

        PathNode startNode = grid.GetGridObject(startX,startY,startZ);
        PathNode endNode = grid.GetGridObject(endX, endY, endZ);

        //Debug.Log("startNode : " + startNode + " endNode : " + endNode);

        //To make sure I have a start and an end
        if(startNode == null || endNode == null) {
            Debug.Log("There is no StartNode and endNode");
        }
        openList = new List<PathNode> { startNode };
        closeList = new List<PathNode>();

        //Initializing the grid values
        for(int x = 0; x < grid.GetWidth(); x++){
            for(int y = 0; y < grid.GetHeight(); y++){
                for(int z = 0; z < grid.GetLength(); z++){
                    PathNode pathNode = grid.GetGridObject(x, y, z);
                    pathNode.gCost = int.MaxValue;
                    pathNode.CalculateFCost();
                    pathNode.cameFromNode = null;
                }
            }
        }
        startNode.gCost = 0;
        startNode.hCost = fastestWayCost(startNode, endNode);
        startNode.CalculateFCost();

        while(openList.Count > 0) {
            PathNode currentNode = GetLowestFCostNode(openList);
            if (currentNode == endNode) {
                return CalculatePath(endNode);
            }
            else
            {
                openList.Remove(currentNode);
                closeList.Add(currentNode);

                foreach (PathNode neighborNode in GetNeighborList(currentNode))
                {
                    if (closeList.Contains(neighborNode)) continue;

                    int tentativeGCost = currentNode.gCost + fastestWayCost(currentNode, neighborNode);
                    if (tentativeGCost < neighborNode.gCost)
                    {
                        neighborNode.cameFromNode = currentNode;
                        neighborNode.gCost = tentativeGCost;
                        neighborNode.hCost = fastestWayCost(neighborNode, endNode);
                        neighborNode.CalculateFCost();

                        if (!openList.Contains(neighborNode))
                        {
                            openList.Add(neighborNode);
                        }
                    }
                }
           
            }
        }
        //out of nodes on the openList
        return null;
    }


    private List<PathNode> GetNeighborList(PathNode currentNode)
    {
        List<PathNode> neighborList = new List<PathNode>();

        if(currentNode.z - 1 >= 0)
        {
            if (currentNode.x - 1 >= 0)
            {
                // Front Left
                neighborList.Add(GetNode(currentNode.x - 1, currentNode.y, currentNode.z - 1));
                // Front Down
                if (currentNode.y - 1 >= 0) 
                    neighborList.Add(GetNode(currentNode.x - 1, currentNode.y - 1, currentNode.z - 1));
                // Front Up
                if (currentNode.y + 1 < grid.GetHeight()) 
                    neighborList.Add(GetNode(currentNode.x - 1, currentNode.y + 1, currentNode.z - 1));
            }

            if (currentNode.x + 1 < grid.GetWidth())
            {
                // Front Right
                neighborList.Add(GetNode(currentNode.x + 1, currentNode.y, currentNode.z - 1));
                // Front Right Down
                if (currentNode.y - 1 >= 0) neighborList.Add(GetNode(currentNode.x + 1, currentNode.y - 1, currentNode.z - 1));
                // Front Right Up
                if (currentNode.y + 1 < grid.GetHeight()) neighborList.Add(GetNode(currentNode.x + 1, currentNode.y + 1, currentNode.z - 1));
            }

            //Front
            if (currentNode.z - 1 < grid.GetHeight()) neighborList.Add(GetNode(currentNode.x, currentNode.y, currentNode.z - 1));
            // Front Down
            if (currentNode.y - 1 >= 0) neighborList.Add(GetNode(currentNode.x, currentNode.y - 1, currentNode.z - 1));
            // Front Up
            if (currentNode.y + 1 < grid.GetHeight()) neighborList.Add(GetNode(currentNode.x, currentNode.y + 1, currentNode.z - 1));
        }

        if (currentNode.x - 1 >= 0)
        {
            // Left
            neighborList.Add(GetNode(currentNode.x - 1, currentNode.y,currentNode.z));
            // Left Down
            if (currentNode.y - 1 >= 0) neighborList.Add(GetNode(currentNode.x - 1, currentNode.y - 1,currentNode.z));
            // Left Up
            if (currentNode.y + 1 < grid.GetHeight()) neighborList.Add(GetNode(currentNode.x - 1, currentNode.y + 1, currentNode.z));
        }

        if (currentNode.x + 1 < grid.GetWidth())
        {
            // Right
            neighborList.Add(GetNode(currentNode.x + 1, currentNode.y, currentNode.z));
            // Right Down
            if (currentNode.y - 1 >= 0) neighborList.Add(GetNode(currentNode.x + 1, currentNode.y - 1, currentNode.z));
            // Right Up
            if (currentNode.y + 1 < grid.GetHeight()) neighborList.Add(GetNode(currentNode.x + 1, currentNode.y + 1, currentNode.z));
        }

        // Down
        if (currentNode.y - 1 >= 0) neighborList.Add(GetNode(currentNode.x, currentNode.y - 1, currentNode.z));
        // Up
        if (currentNode.y + 1 < grid.GetHeight()) neighborList.Add(GetNode(currentNode.x, currentNode.y + 1, currentNode.z));
        

        if (currentNode.z + 1 >= 0)
        {
            if (currentNode.x - 1 >= 0)
            {
                // behind Left
                neighborList.Add(GetNode(currentNode.x - 1, currentNode.y, currentNode.z + 1));
                // behind Down
                if (currentNode.y - 1 >= 0) neighborList.Add(GetNode(currentNode.x - 1, currentNode.y - 1, currentNode.z + 1));
                // behind Up
                if (currentNode.y + 1 < grid.GetHeight()) neighborList.Add(GetNode(currentNode.x - 1, currentNode.y + 1, currentNode.z + 1));
            }

            if (currentNode.x + 1 < grid.GetWidth())
            {
                // behind Right
                neighborList.Add(GetNode(currentNode.x + 1, currentNode.y, currentNode.z + 1));
                // behind Right Down
                if (currentNode.y - 1 >= 0) neighborList.Add(GetNode(currentNode.x + 1, currentNode.y - 1, currentNode.z + 1));
                // behind Right Up
                if (currentNode.y + 1 < grid.GetHeight()) neighborList.Add(GetNode(currentNode.x + 1, currentNode.y + 1, currentNode.z + 1));
            }

            //behind
            if (currentNode.z - 1 < grid.GetHeight()) neighborList.Add(GetNode(currentNode.x, currentNode.y, currentNode.z + 1));
            // behind Down
            if (currentNode.y - 1 >= 0) neighborList.Add(GetNode(currentNode.x, currentNode.y - 1, currentNode.z + 1));
            // behind Up
            if (currentNode.y + 1 < grid.GetHeight()) neighborList.Add(GetNode(currentNode.x, currentNode.y + 1, currentNode.z + 1));

            
        }
        return neighborList;
    }

    public PathNode GetNode(int x, int y, int z)
    {
        return grid.GetGridObject(x, y, z);
    }

    private List<PathNode> CalculatePath(PathNode endNode) {
        List<PathNode> path = new List<PathNode>();
        path.Add(endNode);
        //Debug.Log("endNode : " + endNode);
        PathNode currentNode = endNode;
        while(currentNode.cameFromNode != null) {
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();
        return path;
    }
    private int fastestWayCost(PathNode start, PathNode end) {

        //Je calcule la valeur absolue entre le noeud au début et le noeud à la fin.
        int xLenght = Mathf.Abs(start.x - end.x);
        int yLenght = Mathf.Abs(start.y - end.y);
        int zLenght = Mathf.Abs(start.z - end.z);

        //Je calcule le nombre de node que je vais devoir traverser en ligne droite.
        int straightLineValue = Mathf.Abs(xLenght - yLenght) + Mathf.Abs(xLenght - zLenght) + Mathf.Abs(yLenght - zLenght);

        //Je calcule le le nombre de node que je vais devoir traverser en diagonaler.
        int diagonalValue = Mathf.Min(xLenght, yLenght) + Mathf.Min(xLenght, zLenght) + Mathf.Min(yLenght, zLenght);

        //Je calcule le coût du chemin au complet
        return (DIAGONAL * diagonalValue) + (STRAIGHT * straightLineValue);
    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList) {
        PathNode lowestFCostNode = pathNodeList[0];
        for(int i = 1; i < pathNodeList.Count;i++){
            if(pathNodeList[i].fCost < lowestFCostNode.fCost) {
                lowestFCostNode = pathNodeList[i];
            }
        }
        return lowestFCostNode;
    }
}
