// Performance Optimization Possibilities (POP):
//     @0 Double check everything passes by ref
//     @1 Instead of scanning whole grid, scan the tiles adjacent to each
//         popped tile up to minTilesToCombo per direction.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{
    public HexCell cellPrefab;
    public Text cellLabelPrefab;
    public GameObject r_Hex;
    public GameObject g_Hex;
    public GameObject b_Hex;
    public GameObject y_Hex;
    public GameObject default_Hex;
    [SerializeField] public int minTilesInCombo = 3;
    [SerializeField] public int minTilesForGlow = 2;
    public bool enableCoords = false;
    public bool enableRandomGen = false;
    private List<HexCell> gridList = new List<HexCell>();

    HexCell[] cells;
    public Level1 level = new Level1();
    public RandomLevelGeneration randLevel = new RandomLevelGeneration();
    private int width = 0;
    private int height = 0;

    Canvas gridCanvas;
    HexMesh hexMesh;

    void Awake()
    {
        CheckErrors();
        GetGridDimensions();

        this.gridCanvas = GetComponentInChildren<Canvas>();
        this.hexMesh = GetComponentInChildren<HexMesh>();

        GenerateHexGrid();
        ScanListForGlow(gridList);
    }

    void CheckErrors()
    {
        if (level == null
            || cellPrefab == null
            || cellLabelPrefab == null
        )
        {
            Debug.Log("hexArray not assigned");
        }

        if (r_Hex == null || g_Hex == null || b_Hex == null || y_Hex == null)
            Debug.LogError("Error: a Hex is not assigned.");

        if (minTilesForGlow >= minTilesInCombo)
        {
            Debug.LogError("minTilesForGlow is >= to minTilesInCombo");
        }
    }

    // getGridDimensions: used once at the start of the level
    void GetGridDimensions()
    {
        // If random generation is enabled, use its width/height instead
        if (enableRandomGen)
        {
            this.width = randLevel.getWidth();
            this.height = randLevel.getHeight();
        }
        else
        {
            this.width = level.getWidth();
            this.height = level.getHeight();
        }

        cells = new HexCell[height * width];
    }

    void GenerateHexGrid()
    {
        char[,] testLevel = level.getArray();

        // If random generation is enabled, generate a random array
        if (enableRandomGen)
        {
            testLevel = randLevel.generateArray();
        }

        for (int z = 0, i = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                gridList.Add(
                    CreateCell(
                        x,
                        z,
                        i,
                        testLevel[z, x]
                    ));
                i++;
            }
        }
    }

    // ScanListForGlow: Iterate through each tile in the list, checking whether they're
    //     in a combo with a given minimum, and performing a callback on them.
    public void ScanListForGlow(List<HexCell> list)
    {
        foreach (HexCell cell in list) cell.setGlow(false);
        foreach (HexCell cell in list)
        {
            if (cell.getGlow() == true) continue;
            FindGlowCombos(cell);  // TODO Double check this is being passed by reference
        }
    }
    
    public void ScanListForGlow()
    {
        foreach (HexCell cell in gridList) cell.setGlow(false);
        foreach (HexCell cell in gridList)
        {
            if (cell.getGlow() == true) continue;
            FindGlowCombos(cell);
        }
    }

    private void SetListToGlow(List<HexCell> list)
    {
        foreach (HexCell cell in list)
        {
            cell.setGlow(true);
        }
    }

    private void UnSetListToGlow(List<HexCell> list)
    {
        foreach (HexCell cell in list)
        {
            cell.setGlow(false);
        }
    }

    GameObject ReturnModelByCellKey(char key)
    {
        switch (key)
        {
            case 'r':
                return r_Hex;
            case 'g':
                return g_Hex;
            case 'b':
                return b_Hex;
            case 'y':
                return y_Hex;
            default:
                return default_Hex;
        }
    }


    void Start()
    {
        hexMesh.Triangulate(cells);
    }

    //
    // CreateCell: Creates a hexCell at the given x & z (where x is the 
    //     vertical axis. z is the horizontal axis. These are NOT
    //     part of the axial coordinates system).
    //     Note: This is only used for in the initial board generation.
    //     
    // 
    HexCell CreateCell(int x, int z, int i, char key)
    {
        Vector3 position;
        position.x = (x +
                      z * 0.5f // Creates a staircase-like effect
                      - z / 2) // This int division creates the zigging back every other line
                     * (HexMetrics.innerRadius * 2f);
        position.y = 0f;
        position.z = z * (HexMetrics.outerRadius * 1.5f);

        HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
        cell.setSpawnCoords(x, z);
        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
        cell.createModel(ReturnModelByCellKey(key));
        cell.setKey(key);

        if (x > 0) // Skips first vertical axis
        {
            cell.SetNeighbor(HexDirection.W, cells[i - 1]);
        }

        if (z > 0) // Skips first horizontal axis
        {
            if ((z & 1) == 0) // For every odd z
            {
                cell.SetNeighbor(HexDirection.SE, cells[i - width]);
                if (x > 0)
                {
                    cell.SetNeighbor(HexDirection.SW, cells[i - width - 1]);
                }
            }
            else
            {
                cell.SetNeighbor(HexDirection.SW, cells[i - width]);
                if (x < width - 1)
                {
                    cell.SetNeighbor(HexDirection.SE, cells[i - width + 1]);
                }
            }
        }

        Text label = Instantiate<Text>(cellLabelPrefab);
        label.rectTransform.SetParent(gridCanvas.transform, false);
        label.rectTransform.anchoredPosition =
            new Vector2(position.x, position.z);
        label.text = cell.coordinates.ToStringOnSeparateLines();
        if (!enableCoords) label.enabled = false;
        return cell;
    }

    public void ComboCallback(List<HexCell> list)
    {
        foreach (HexCell cell in list)
        {
            Destroy(cell.getModel());
            cell.setKey('e');  // Temp, remove when tile regeneration is implemented
            cell.setGlow(false);
        }

        ScanListForGlow(list);  // @1: instead of scanning whole grid, scan the tiles adjacent to each
                                //     popped tile up to minTilesToCombo per direction.
    }

    /*  Swaps the given hex with a new hex of key heldKey
        @param modelHit - the hex tile we want to swap
        @param heldKey - the color key we want the hex to be
        @return char - the original color key of the hex before swapping occurs
    */
    public char SwapHexAndKey(GameObject modelHit, char heldKey)
    {
        Debug.Log("swapped");
        char tempKey = modelHit.GetComponentInParent<HexCell>().getKey();
        HexCell parent = modelHit.GetComponentInParent<HexCell>().getThis(); // The parent of the model
        Destroy(modelHit,
            0f); // TODO this doesn't update model to be null but i don't think it matters. Maybe create a HexCell.destroyModel()
        parent.createModel(this.ReturnModelByCellKey(heldKey));
        parent.setKey(heldKey);
        return (tempKey);
    }

    public int GetMinTilesInCombo()
    {
        return minTilesInCombo;
    }

    public int GetMinTilesForGlow()
    {
        return minTilesForGlow;
    }

    public bool FindGlowCombos(HexCell cell)
    {
        return cell.FindCombos(this.SetListToGlow, this.minTilesForGlow);
    }

    /* RecalculateGlowForNonCombo: Recalculates isGlowing for a newly-swapped cell and for other cells
                                   in each direction, minTilesInCombo-times. This function 
                                   is really just a slight performance optimization over
                                   rescanning whole grid. */
    public void RecalculateGlowForNonCombo(HexCell swappedCell)
    {
        swappedCell.setGlow(false);
        FindGlowCombos(swappedCell);
        for (var direction = 0; direction < 6; direction++)
        {
            HexDirection hexDirection = (HexDirection) direction;
            HexCell neighbor = swappedCell.GetNeighbor(hexDirection);
            if (neighbor)
            {
                RecalculateGlowInDirection(neighbor, hexDirection, 1);
            }
        }
    }
    
    // RecalculateGlowInDirection: Recursive call for RecalculateGlowForNonCombo.
    public void RecalculateGlowInDirection(HexCell cell, HexDirection direction, int count)
    {
        if (count >= this.minTilesInCombo) return;
        
        cell.setGlow(false);
        FindGlowCombos(cell);
        HexCell neighbor = cell.GetNeighbor(direction);
        if (neighbor) RecalculateGlowInDirection(neighbor, direction, count+1);
    }
}