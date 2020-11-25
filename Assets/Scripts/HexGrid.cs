// Performance Optimization Possibilities (POP):
//     @0 Double check everything passes by ref
//     @1 Instead of scanning whole grid, scan the tiles adjacent to each
//         popped tile up to minTilesToCombo per direction.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{
    public HexCell cellPrefab;
    public Text cellLabelPrefab;
    private string cellPrefabName = "Hex Cell Label";
    public GameObject r_Hex; private string r_Hex_Name = "RedHex";
    public GameObject g_Hex; private string g_Hex_Name = "GreenHex"; 
    public GameObject b_Hex; private string b_Hex_Name = "BlueHex";
    public GameObject y_Hex; private string y_Hex_Name = "YellowHex";
    public GameObject p_Hex; private string p_Hex_Name = "PurpleHex";
    public GameObject w_Hex; private string w_Hex_Name = "WhiteHex";
    public GameObject default_Hex; private string default_Hex_Name = "EmptyHex";
    [SerializeField] public int minTilesInCombo = 3;
    [SerializeField] public int minTilesForGlow = 2;
    private List<HexCell> gridList = new List<HexCell>();

    public bool enableCoords = false;

    // Whether the map will generate randomly on start
    public bool enableRandomGen = false;

    // If enabled: regenerated tiles can (by random chance) form its own combos.
    // If disabled: regenerated tiles will never form combos with its neighbors
    public bool enableChainRegen = false;

    HexCell[] cells;
    public Level1 level = new Level1();
    public RandomLevelGeneration randLevel = new RandomLevelGeneration();
    public int width = 19;
    public int height = 11;

    Canvas gridCanvas;
    HexMesh hexMesh;

    private char[] tileTypes = { 'r', 'b', 'g', 'y', 'p', 'w' };
    public float tileRegenDuration = 1.5f;
    

    void Awake()
    {
        LinkAssets();
        CheckErrors();
        GetGridDimensions();

        this.gridCanvas = GetComponentInChildren<Canvas>();
        this.hexMesh = GetComponentInChildren<HexMesh>();

        GenerateHexGrid();
        ScanListForGlow(gridList);
    }

    private void LinkAssets()
    {
        // cellPrefab = Resources.Load<HexCell>(String.Concat("Prefabs/", HexMetrics.GetHexCellPrefabName())); 
        
        
        // r_Hex = Resources.Load<GameObject>(String.Concat("Prefabs/Hexes/",r_Hex_Name));
        // g_Hex = Resources.Load<GameObject>(String.Concat("Prefabs/Hexes/",g_Hex_Name));
        // b_Hex = Resources.Load<GameObject>(String.Concat("Prefabs/Hexes/",b_Hex_Name));
        // y_Hex = Resources.Load<GameObject>(String.Concat("Prefabs/Hexes/",y_Hex_Name));
        // p_Hex = Resources.Load<GameObject>(String.Concat("Prefabs/Hexes/",p_Hex_Name));
        // w_Hex = Resources.Load<GameObject>(String.Concat("Prefabs/Hexes/",w_Hex_Name));
        // default_Hex = Resources.Load<GameObject>(String.Concat("Prefabs/Hexes/",default_Hex_Name));
        
        // Development
        // cellLabelPrefab = Resources.Load<Text>(String.Concat("/Hexes/",cellPrefabName));
    }

    void CheckErrors()
    {
        if (level == null
            || cellPrefab == null
            // || cellLabelPrefab == null
        )
        {
            Debug.Log("HexGrid.cs: A Hex Asset is not assigned");
        }

        if (r_Hex == null || g_Hex == null || b_Hex == null || y_Hex == null || p_Hex == null
        || w_Hex == null || default_Hex == null)
            Debug.LogError("HexGrid.cs: a Hex is not assigned.");

        if (minTilesForGlow >= minTilesInCombo)
        {
            Debug.LogError("minTilesForGlow is >= to minTilesInCombo");
        }
    }

    // getGridDimensions: used once at the start of the level
    void GetGridDimensions()
    {
        // Use preset level if random gen disabled
        if (!enableRandomGen)
        {
            this.width = level.getWidth();
            this.height = level.getHeight();
        }

        cells = new HexCell[height * width];
    }

    void GenerateHexGrid()
    {
        for (int z = 0, i = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                if (!enableRandomGen)
                {
                    this.gridList.Add(CreateCell(x, z, i, level.getArray()[z,x]));
                    i++;
                    continue;
                }

                char randomType = tileTypes[UnityEngine.Random.Range(0, tileTypes.Length)];
                HexCell newCell = CreateCell(x, z, i, randomType);

                MakeCellUnique(newCell);
   
                this.gridList.Add(newCell);
                i++;
            }
        }
    }

    // MakeCellUnique: Updates a hex color to not generate a combo with its neighbors
    void MakeCellUnique(HexCell cell)
    {
        List<char> listTileTypes = new List<char>(tileTypes);

        // While a combo exists with this tile type, randomly generate a different tile type
        while (cell.FindCombos((List<HexCell> _) => { }, GetMinTilesInCombo()))
        {
            cell.deleteModel();
            // Remove the tile type from list and generate new tile type
            listTileTypes.Remove(cell.getKey());

            char newType = listTileTypes[UnityEngine.Random.Range(0, listTileTypes.Count)];
            cell.createModel(ReturnModelByCellKey(newType));
            cell.setKey(newType);
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

    public GameObject ReturnModelByCellKey(char key)
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
            case 'p':
                return p_Hex;
            case 'w':
                return w_Hex;
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

    IEnumerator RegenerateCell(HexCell cell)
    {
        if (cell.getKey() == 'e')
        {
            yield break;
        }
        cell.setKey('e');
        Destroy(cell.getModel());
        yield return new WaitForSeconds(tileRegenDuration);

        char newKey = tileTypes[UnityEngine.Random.Range(0, tileTypes.Length)];
        cell.setKey(newKey);
        cell.createModel(ReturnModelByCellKey(newKey));

        // TODO: Add shader effects here for tile regen 

        // If chain regeneration enabled, check for new combos
        if (enableChainRegen)
        {
            if (cell.FindCombos((List<HexCell> _) => { }, GetMinTilesInCombo()))
            {
                // Wait one second, so combo not instantly executed
                yield return new WaitForSeconds(1);
                cell.FindCombos(ComboCallback, GetMinTilesInCombo());
            }
        } else
        {
            MakeCellUnique(cell);
        }
    }

    public void ComboCallback(List<HexCell> list)
    {
        foreach (HexCell cell in list)
        {
            // Start a coroutine that regenerates the tile
            StartCoroutine(RegenerateCell(cell));
            // cell.setGlow(false);
        }

        // ScanListForGlow(list);  // @1: instead of scanning whole grid, scan the tiles adjacent to each
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
            0f); 
        parent.createModel(this.ReturnModelByCellKey(heldKey));
        parent.setKey(heldKey);
        if (parent.FindCombos(this.ComboCallback, this.GetMinTilesInCombo()) == true)
        {
            // Unoptimized Scan TODO optimize it
            this.ScanListForGlow();
        } else
        {
            // Optimized scan
            this.RecalculateGlowForNonCombo(parent);
        }
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