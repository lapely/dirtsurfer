/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class Grid<TGridObject> {

    private int xWidth;
    private int yHeight;
    private int zLength;
    private float cellSize;
    private Vector3 originPosition;
    private TGridObject[,,] gridArray;
    private GameObject gridParent;

    public Grid(int width, int height, int length, float cellSize, Vector3 originPosition, Func<Grid<TGridObject>, int, int, int, TGridObject> createGridObject) {
        //Constructeur de grid
        this.xWidth = width;
        this.yHeight = height;
        this.zLength = length;
        this.cellSize = cellSize;
        this.originPosition = originPosition;
        gridArray = new TGridObject[xWidth, yHeight, zLength];

        TextMesh[,,] debugTextArray = new TextMesh[(int)width, (int)height, (int)length];

        for (int x = 0; x < gridArray.GetLength(0); x++) {
            for (int y = 0; y < gridArray.GetLength(1); y++) {
                for (int z = 0; z < gridArray.GetLength(2); z++) {
                    gridArray[x, y, z] = createGridObject(this, x, y, z);
                    debugTextArray[x, y, z] = UtilsClass.CreateWorldText(gridArray[x, y, z].ToString(), null, GetWorldPosition(x, y, z) + new Vector3(cellSize, cellSize, cellSize) * .5f, 10, Color.white, TextAnchor.MiddleCenter);
                    gridParent = GameObject.Find("GridParent");
                    debugTextArray[x, y, z].transform.parent = gridParent.transform;
                    Debug.DrawLine(GetWorldPosition(x, y, z), GetWorldPosition(x + 1, y, z), Color.white, 100f);
                    Debug.DrawLine(GetWorldPosition(x, y, z), GetWorldPosition(x, y + 1, z), Color.white, 100f);
                    Debug.DrawLine(GetWorldPosition(x, y, z), GetWorldPosition(x, y, z + 1), Color.white, 100f);
                }
            }
        } 
        Debug.DrawLine(GetWorldPosition(0, yHeight, zLength), GetWorldPosition(xWidth, yHeight, zLength), Color.white, 100f);
        Debug.DrawLine(GetWorldPosition(xWidth, 0, zLength), GetWorldPosition(xWidth, yHeight, zLength), Color.white, 100f);
        Debug.DrawLine(GetWorldPosition(xWidth, yHeight, 0), GetWorldPosition(xWidth, yHeight, zLength), Color.white, 100f);
        
    }

    public Vector3 GetWorldPosition(int x, int y, int z) {
        return new Vector3(x, y, z) * cellSize + originPosition;
    }

    public float GetWidth() {
        return xWidth;
    }
    public float GetHeight() {
        return yHeight;
    }
    public float GetLength() {
        return zLength;
    }
    public float getCellSize() {
        return cellSize;
    }

    public void GetGridPosition(Vector3 worldPosition,out int x, out int y, out int z) {
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize);
        z = Mathf.FloorToInt((worldPosition - originPosition).z / cellSize);
    }

    public void SetValue(int x, int y, int z, TGridObject value)
    {
        if (x >= 0 && y >= 0 && z >= 0 && x < xWidth && y < yHeight && z < zLength)
            gridArray[x, y, z] = value;
    }
    public void SetValue(Vector3 worldPosition, TGridObject value) {
        int x, y, z;
        GetGridPosition(worldPosition, out x, out y, out z);
        SetValue(x, y, z, value);
    }

    public TGridObject GetGridObject(int x, int y, int z){
            return gridArray[x, y, z];
    }
    public TGridObject GetValue(Vector3 worldPosition){
        int x, y, z;
        GetGridPosition(worldPosition,out x,out y,out z);
        return GetGridObject(x, y, z);
    }
}
