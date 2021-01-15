// Performance Optimization Possibilities (POP):
//     @0 Double check everything passes by ref

using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : NetworkBehaviour
{
    /// <summary>
    /// cellPrefab: The Hex Cell prefab to be instantiated by HexGrid at runtime
    /// </summary>
    public HexCell cellPrefab;
    
    /// Debug tools
    public Text cellLabelPrefab;
    private string cellPrefabName = "Hex Cell Label";
    
    // Runtime Instantiation Tools
    public GameObject r_Hex;
    private string r_Hex_Name = "RedHex";
    public GameObject g_Hex;
    private string g_Hex_Name = "GreenHex";
    public GameObject b_Hex;
    private string b_Hex_Name = "BlueHex";
    public GameObject y_Hex;
    private string y_Hex_Name = "YellowHex";
    public GameObject p_Hex;
    private string p_Hex_Name = "PurpleHex";
    public GameObject w_Hex;
    private string w_Hex_Name = "WhiteHex";
    public GameObject default_Hex;
    private string default_Hex_Name = "EmptyHex";
    
    public int minTilesForCombo = 3;
    public int minTilesForGlow = 2;
    
    /// <summary>
    /// gridList: Stores every Hex Cell on the board
    /// </summary>
    private List<HexCell> gridList = new List<HexCell>(); 

    public bool enableCoords = false;

    // Whether the map will generate randomly on start
    public bool enableRandomGen = false;

    /// <summary>
    /// ignoreRandomGenOnE: if true, use preset map's 'e' tiles as default. Otherwise, ignore.
    /// </summary>
    public bool ignoreRandomGenOnE = true;

    // If enabled: regenerated tiles can (by random chance) form its own combos.
    // If disabled: regenerated tiles will never form combos with its neighbors
    public bool enableChainRegen = false;

    public Level1 level = new Level1();
    private int width;
    private int height;

    Canvas gridCanvas;
    HexMesh hexMesh;

    private char[] tileTypes = {'r', 'b', 'g', 'y', 'p', 'w'};
    public float tileRegenDuration = 1.5f;


    void Start()
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
            // || cellLabelPrefab == null
        )
        {
            Debug.Log("HexGrid.cs: A Hex Asset is not assigned");
        }

        if (r_Hex == null || g_Hex == null || b_Hex == null || y_Hex == null || p_Hex == null
            || w_Hex == null || default_Hex == null)
            Debug.LogError("HexGrid.cs: a Hex is not assigned.");

        if (minTilesForGlow >= minTilesForCombo)
        {
            Debug.LogError("minTilesForGlow is >= to minTilesInCombo");
        }
    }

    // getGridDimensions: used once at the start of the level
    void GetGridDimensions()
    {
        this.width = level.getWidth();
        this.height = level.getHeight();
        if (width == null || height == null) Debug.LogError("GetGridDimensions: could not get level dimensions.");

        // cells = new HexCell[height * width];
    }

    void GenerateHexGrid()
    {
        for (int z = 0, i = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                //if (!enableRandomGen)
                //{
                //    RpcCreateCell(x, z, i, level.getArray()[z, x]);
                //    i++;
                //    continue;
                //}

                //if (ignoreRandomGenOnE && level.getArray()[z, x] != 'e')
                //{
                //    char newTileKey = tileTypes[UnityEngine.Random.Range(0, tileTypes.Length)];
                //    RpcCreateCellMakeCellUnique(x, z, i, newTileKey);
                //}
                //else
                //{
                //    RpcCreateCell(x, z, i, 'e');
                //}
                CreateCell(x, z, i, level.getArray()[z, x]);
                i++;
            }
        }
    }

    [ClientRpc]
    void RpcGenerateHexGrid()
    {
        GenerateHexGrid();
    }

    [ClientRpc]
    void RpcCreateCell(int x, int z, int i, char tileKey)
    {
        CreateCell(x, z, i, tileKey);
    }

    [ClientRpc]
    void RpcCreateCellMakeCellUnique(int x, int z, int i, char tileKey)
    {
        HexCell newCell = CreateCell(x, z, i, tileKey);
        MakeCellUnique(newCell);
    }

    // MakeCellUnique: Updates a hex color to not generate a combo with its neighbors
    void MakeCellUnique(HexCell cell)
    {
        List<char> listTileTypes = new List<char>(tileTypes);

        // While a combo exists with this tile type, randomly generate a different tile type
        while (cell.FindSameColorTiles((List<HexCell> _) => { }, GetMinTilesInCombo()))
        {
            cell.DeleteModel();
            // Remove the tile type from list and generate new tile type
            listTileTypes.Remove(cell.GetKey());

            char newType = listTileTypes[UnityEngine.Random.Range(0, listTileTypes.Count)];
            cell.CreateModel(ReturnModelByCellKey(newType));
            cell.SetKey(newType);
        }
    }

    
    /// <summary>
    /// ScanListForGlow: Iterate through each tile in the list, checking whether they're
    ///     in a combo with a given minimum, and performing a callback on them.         
    /// <param name="list"></param>
    /// </summary>
    public void ScanListForGlow(List<HexCell> list)
    {
        foreach (HexCell cell in list) cell.SetGlow(false);
        foreach (HexCell cell in list)
        {
            if (cell.GetGlow() == true) continue;
            FindGlowCombos(cell); // TODO Double check this is being passed by reference
        }
    }

    /// <summary>
    /// ScanListForGlow: Iterate through each tile in the list, checking whether they're
    ///     in a combo with a given minimum, and performing a callback on them.         
    /// </summary>
    public void ScanListForGlow()
    {
        foreach (HexCell cell in gridList) cell.SetGlow(false);
        foreach (HexCell cell in gridList)
        {
            if (cell.GetGlow() == true) continue;
            FindGlowCombos(cell);
        }
    }

    /// <summary>
    /// SetListToGlow: Set every Hex Cell in given list to glow.
    /// </summary>
    /// <param name="list"></param>
    private void SetListToGlow(List<HexCell> list)
    {
        foreach (HexCell cell in list)
        {
            cell.SetGlow(true);
        }
    }
    
    private void UnSetListToGlow(List<HexCell> list)
    {
        foreach (HexCell cell in list)
        {
            cell.SetGlow(false);
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
    
    /// <summary>
    /// CreateCell: Creates a hexCell at the given x & z (where x is the 
    ///     vertical axis. z is the horizontal axis. These are NOT
    ///     part of the axial coordinates system).
    ///     Note: This is only used for in the initial board generation.
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <param name="i"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    /// </summary>
    HexCell CreateCell(int x, int z, int i, char key)
    {
        Vector3 position;
        position.x = (x +
                      z * 0.5f // Creates a staircase-like effect
                      - z / 2) // This int division creates the zigging back every other line
                     * (HexMetrics.innerRadius * 2f);
        position.y = 0f;
        position.z = z * (HexMetrics.outerRadius * 1.5f);

        HexCell cell = Instantiate<HexCell>(cellPrefab);
        gridList.Add(cell);
        
        cell.SetSpawnCoords(x, z);
        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
        cell.CreateModel(ReturnModelByCellKey(key));
        cell.SetKey(key);
        cell.SetListIndex(i);

        if (x > 0) // Skips first vertical axis
        {
            cell.SetNeighbor(HexDirection.W, gridList[i - 1]);
        }

        if (z > 0) // Skips first horizontal axis
        {
            if ((z & 1) == 0) // For every odd z
            {
                cell.SetNeighbor(HexDirection.SE, gridList[i - width]);
                if (x > 0)
                {
                    cell.SetNeighbor(HexDirection.SW, gridList[i - width - 1]);
                }
            }
            else
            {
                cell.SetNeighbor(HexDirection.SW, gridList[i - width]);
                if (x < width - 1)
                {
                    cell.SetNeighbor(HexDirection.SE, gridList[i - width + 1]);
                }
            }
        }

        return cell;
    }

    IEnumerator RegenerateCell(HexCell cell)
    {
        if (cell.GetKey() == 'e')
        {
            yield break;
        }

        cell.SetKey('e');
        Destroy(cell.GetModel());
        yield return new WaitForSeconds(tileRegenDuration);

        char newKey = tileTypes[UnityEngine.Random.Range(0, tileTypes.Length)];
        cell.SetKey(newKey);
        cell.CreateModel(ReturnModelByCellKey(newKey));

        // TODO: Add shader effects here for tile regen 

        // If chain regeneration enabled, check for new combos
        if (enableChainRegen)
        {
            if (cell.FindSameColorTiles((List<HexCell> _) => { }, GetMinTilesInCombo()))
            {
                // Wait one second, so combo not instantly executed
                yield return new WaitForSeconds(1);
                cell.FindSameColorTiles(ComboCallback, GetMinTilesInCombo());
            }
        }
        else
        {
            MakeCellUnique(cell);
        }
    }

    /// <summary>
    /// SpawnComboObjByKey: Returns a Combo Object determined by param 'key' and whose position
    /// is at param 'spawnCoords'
    /// </summary>
    /// <param name="key"></param>
    /// <param name="spawnCoords"></param>
    /// <returns></returns>
    GameObject SpawnComboObjByKey(char key, Transform spawnCoords)
    {
        var bombObjPath = Resources.Load("Prefabs/ComboObjects/Bomb Object");
        var laserObjPath = Resources.Load("Prefabs/ComboObjects/Laser Object");
        Vector3 offset = new Vector3(0f, 2f, 0f);
        GameObject result;
        switch (key)
        {
            case 'b':
                Debug.Log("Blue");
                result = (GameObject) Instantiate(laserObjPath, spawnCoords);
                result.GetComponent<LaserObject>().SetDirection(HexDirection.SW);
                break;
            case 'g':
                result = (GameObject) Instantiate(laserObjPath, spawnCoords);
                result.GetComponent<LaserObject>().SetDirection(HexDirection.W);
                break;
            case 'y':
                result = (GameObject) Instantiate(laserObjPath, spawnCoords);
                result.GetComponent<LaserObject>().SetDirection(HexDirection.SE);
                break;
            case 'r':
                result = (GameObject) Instantiate(bombObjPath, spawnCoords);
                break;
            case 'p':
                result = (GameObject) Instantiate(bombObjPath, spawnCoords);
                break;
            case 'w':
                result = (GameObject) Instantiate(bombObjPath, spawnCoords);
                break;
            default:
                result = (GameObject) Instantiate(bombObjPath, spawnCoords);
                break;
        }

        result.transform.localPosition += offset;
        return result;
    }

    /// <summary>
    /// ComboCallback: Given a list of same-color neighboring tiles, do something. Usually given as an argument to
    /// HexCell.FindSameColorTiles()
    /// </summary>
    /// <param name="list"></param>
    public void ComboCallback(List<HexCell> list)
    {
        foreach (HexCell cell in list)
        {
            var ComboObj = SpawnComboObjByKey(cell.GetKey(), cell.transform);
            // Start a coroutine that regenerates the tile
            StartCoroutine(RegenerateCell(cell));
        }
    }

    /// <summary>
    /// Swaps the given hex with a new hex of key heldKey
    /// @param modelHit - the hex tile we want to swap
    /// @param heldKey - the color key we want the hex to be
    /// @return char - the original color key of the hex before swapping occurs
    /// </summary>
    public char SwapHexAndKey(GameObject modelHit, char heldKey)
    {
        Debug.Log("swapped");
        char tempKey = modelHit.GetComponentInParent<HexCell>().GetKey();
        int cellIdx = modelHit.GetComponentInParent<HexCell>().GetThis().getListIndex();

        CmdSwapHexAndKey(cellIdx, heldKey);

        return (tempKey);
    }

    [Command(ignoreAuthority = true)]
    void CmdSwapHexAndKey(int cellIdx, char heldKey)
    {
        RpcSwapHexAndKey(cellIdx, heldKey);
    }

    [ClientRpc]
    public void RpcSwapHexAndKey(int cellIdx, char heldKey)
    {
        HexCell cell = gridList[cellIdx];

        cell.DeleteModel();
        cell.CreateModel(this.ReturnModelByCellKey(heldKey));
        cell.SetKey(heldKey);
        if (cell.FindSameColorTiles(this.ComboCallback, this.GetMinTilesInCombo()) == true)
        {
            // Unoptimized Scan TODO optimize it
            this.ScanListForGlow();
        }
        else
        {
            // Optimized scan
            this.RecalculateGlowForNonCombo(cell);
        }
    }


    public int GetMinTilesInCombo()
    {
        return minTilesForCombo;
    }

    public int GetMinTilesForGlow()
    {
        return minTilesForGlow;
    }

    public bool FindGlowCombos(HexCell cell)
    {
        return cell.FindSameColorTiles(this.SetListToGlow, this.minTilesForGlow);
    }

    /* RecalculateGlowForNonCombo: Recalculates isGlowing for a newly-swapped cell and for other cells
                                   in each direction, minTilesInCombo-times. This function 
                                   is really just a slight performance optimization over
                                   rescanning whole grid. */
    public void RecalculateGlowForNonCombo(HexCell swappedCell)
    {
        swappedCell.SetGlow(false);
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
        if (count >= this.minTilesForCombo) return;

        cell.SetGlow(false);
        FindGlowCombos(cell);
        HexCell neighbor = cell.GetNeighbor(direction);
        if (neighbor) RecalculateGlowInDirection(neighbor, direction, count + 1);
    }
}