// Performance Optimization Possibilities (POP):
//     @0 Double check everything passes by ref

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    public GameObject wall_Hex;
    private string wall_Hex_Name = "WallHex";
    public GameObject slow_Hex;
    private string slow_Hex_Name = "SlowHex";
    public GameObject danger_Hex;
    private string danger_Hex_Name = "DangerHex";
    public GameObject hidden_Hex;
    private string hidden_Hex_Name = "HiddenHex";
    public GameObject terrain_Hex;
    private string terrain_Hex_Name = "TerrainHex";
    public GameObject default_Hex;
    private string default_Hex_Name = "EmptyHex";
    static char slow_Hex_char = '$';
    static char danger_Hex_char = '#';
    static char hidden_Hex_char = 'i';
    static char terrain_Hex_char = 't';
    static char default_Hex_char = 'e';

    public int minTilesForCombo = 3;
    public int minTilesForGlow = 2;

    public bool enableRings = false;
    public int[] phasePlayersAlive;
    public int[] phaseDelays;
    private float phaseTimeElapsed = 0;
    public int phaseCurrent = 0;
    public int phaseTotal;
    public int aliveCount;
    public RoundManager roundManager;

    [Header("Grid Settings")]
    /// <summary>
    /// gridList: Stores every Hex Cell on the board
    /// </summary>
    public List<HexCell> gridList = new List<HexCell>();

    // A network-synchronized list of colors of every hex cell on the board
    // Should represent gridList in the form of colors
    public SyncList<char> colorGridList = new SyncList<char>();

    public bool enableCoords = false;

    // Whether the map will generate randomly on start
    public bool enableRandomGen = false;

    // Seed for random generation
    //public int randomSeed = 1234; //unused

    /// <summary>
    /// ignoreRandomGenOnE: if true, use preset map's 'e' tiles as default. Otherwise, ignore.
    /// </summary>
    public bool ignoreRandomGenOnE = true;

    // If enabled: regenerated tiles can (by random chance) form its own combos.
    // If disabled: regenerated tiles will never form combos with its neighbors
    public bool enableChainRegen = false;

    public Level1 level = new Level1();

    // [SerializeField] private int width = 19;
    // [SerializeField] private int height = 17;
    [SerializeField] private int width = 17;
    [SerializeField] private int height = 10;

    Canvas gridCanvas;
    HexMesh hexMesh;

    [SerializeField] private char[] tileTypes;
    public float tileRegenDuration = 1.5f;

    private EventManager eventManager;

    void Start()
    {
        //UnityEngine.Random.InitState(randomSeed); //unused
        CheckErrors();
        GetGridDimensions();

        this.gridCanvas = GetComponentInChildren<Canvas>();
        this.hexMesh = GetComponentInChildren<HexMesh>();

        GenerateHexGrid();

        CreateHexGridModels(); // When dedicated server is introduced, dont need to create models on server

        //ScanListForGlow(gridList); // temp unused (should happen client side)
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        colorGridList.Callback += OnColorGridListChange;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        // Event manager singleton
        eventManager = EventManager.Singleton;
        if (eventManager == null) Debug.LogError("Cannot find Singleton: EventManager");
    }

    public void Update()
    {

        if (enableRings) HandleRingPhases(phaseDelays, phasePlayersAlive);
    }

    // Creates the models for cells in gridList with the colors saved in colorGridList
    void CreateHexGridModels()
    {
        // verify that gridList is synced with colorGridList
        if (gridList.Count != colorGridList.Count)
        {
            Debug.LogError("GridList is not synced with colorGridList!");
            return;
        }

        for (int i = 0; i < colorGridList.Count; i++)
        {
            // Create the model based on 
            char syncedKey = colorGridList[i];
            gridList[i].CreateModel(ReturnModelByCellKey(syncedKey)); // create model based on synced list
            gridList[i].SetKey(syncedKey); // not necessary, but for safety?
        }
    }

    // Hook that is called when the state of colorGridList changes
    void OnColorGridListChange(SyncList<char>.Operation op, int idx, char oldColor, char newColor)
    {
        HexCell cell = gridList[idx];
        cell.SetKey(newColor);
        cell.CreateModel(this.ReturnModelByCellKey(newColor));
    }

    void CheckErrors()
    {
        if (level == null
            || cellPrefab == null
            // || cellLabelPrefab == null
        )
        {
            Debug.LogWarning("HexGrid.cs: A Hex Asset is not assigned");
        }

        if (r_Hex == null || g_Hex == null || b_Hex == null || y_Hex == null || p_Hex == null
            || w_Hex == null || default_Hex == null)
            Debug.LogError("HexGrid.cs: a Hex is not assigned.");

        if (minTilesForGlow >= minTilesForCombo)
        {
            Debug.LogError("minTilesForGlow is >= to minTilesInCombo");
        }

        if (phaseDelays.Length != phaseTotal || phasePlayersAlive.Length != phaseTotal)
        {
            Debug.LogError("The length of phaseDelays and/or phasePlayerDeathMin aren't equal to phaseCount.");
        }
    }

    // getGridDimensions: used once at the start of the level
    void GetGridDimensions()
    {
        if (!enableRandomGen)
        {
            this.width = level.getWidth();
            this.height = level.getHeight();
        }

        if (width == null || height == null) Debug.LogError("GetGridDimensions: could not get level dimensions.");

        // cells = new HexCell[height * width];
    }

    void GenerateHexGrid()
    {
        for (int z = 0, i = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                CreateCell(x, z, i);
                i++;
            }
        }
    }

    // MakeCellUnique: Updates a hex color to not generate a combo with its neighbors
    //      returns true if cell is updated, false otherwise
    bool MakeCellUnique(int idx)
    {
        // Get a reference to the cell
        HexCell cell = gridList[idx];

        List<char> nonComboTileTypes = new List<char>(tileTypes); // tile types that wont form a combo
        List<HexCell> comboCellsList = cell.FindSameColorTiles(GetMinTilesInCombo());

        // If cells exist in comboCellsList, then it forms a combo
        if (comboCellsList.Count > 0)
        {
            // Remove the the key formed by the cells from list of non combo tile types
            foreach (HexCell c in comboCellsList)
            {
                nonComboTileTypes.Remove(c.GetKey());
            }

            // Update the cell model 
            char newType = nonComboTileTypes[UnityEngine.Random.Range(0, nonComboTileTypes.Count)];
            cell.SetKey(newType);
            cell.CreateModel(ReturnModelByCellKey(newType));
            if (isServer) colorGridList[idx] = newType;

            return true;
        }

        return false;
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
            case 't':
                return terrain_Hex;
            case '-':
                return wall_Hex;
            case '#':
                return danger_Hex;
            case '$':
                return slow_Hex;
            case 'i':
                return hidden_Hex;
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
    HexCell CreateCell(int x, int z, int i)
    {
        Vector3 position;
        position.x = (x +
                      z * 0.5f // Creates a staircase-like effect
                      - z / 2) // This int division creates the zigging back every other line
                     * (HexMetrics.innerRadius * 2f);
        position.y = 0f;
        position.z = z * (HexMetrics.outerRadius * 1.5f);

        // Instantiate cell
        HexCell cell = Instantiate<HexCell>(cellPrefab);
        gridList.Add(cell);

        // Set coords and index in gridList
        cell.SetListIndex(i);
        cell.SetSpawnCoords(x, z);

        // Set spawn position
        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

        // Set cell neighbors
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


        // Set cell (color) and spawn model
        char key = tileTypes[UnityEngine.Random.Range(0, tileTypes.Length)]; // default random generated


        if (!enableRandomGen)
        {
            // If random gen disabled, grab key from level file
            key = level.getArray()[z, x];
        }

        var c = level.getArray()[z, x];
        if (ignoreRandomGenOnE && (level.getArray()[z, x] == GetDangerTileChar() ||
                                   level.getArray()[z, x] == GetEmptyTileChar() ||
                                   level.getArray()[z, x] == GetSlowTileChar() ||
                                   level.getArray()[z, x] == GetHiddenHexChar()))
        {
            key = c;
        }

        cell.SetKey(key);
        if (isServer) colorGridList.Add(key);

        // Update the cell if it makes a combo
        MakeCellUnique(i);

        return cell;
    }

    [Server]
    IEnumerator RegenerateCell(HexCell cell)
    {
        var ignoreKeys = HexCell.ignoreKeys;
        if (ignoreKeys.Contains(cell.GetKey()))
        {
            yield break;
        }

        cell.SetKey('e');
        cell.CreateModel(ReturnModelByCellKey('e'));
        colorGridList[cell.getListIndex()] = 'e';
        yield return new WaitForSeconds(tileRegenDuration);

        char newKey = tileTypes[UnityEngine.Random.Range(0, tileTypes.Length)];
        while (newKey == 'w') newKey = tileTypes[UnityEngine.Random.Range(0, tileTypes.Length)];

        cell.SetKey(newKey);
        cell.CreateModel(ReturnModelByCellKey(newKey));
        colorGridList[cell.getListIndex()] = newKey;

        // TODO: Add shader effects here for tile regen 

        // If chain regeneration enabled, check for new combos
        if (enableChainRegen)
        {
            if (cell.FindSameColorTiles(GetMinTilesInCombo()).Count != 0)
            {
                // Wait one second, so combo not instantly executed
                yield return new WaitForSeconds(1);
                List<HexCell> comboCells = cell.FindSameColorTiles(GetMinTilesInCombo());
                ComboCallback(comboCells);
                //cell.FindSameColorTiles(ComboCallback, GetMinTilesInCombo());
            }
        }
        else
        {
            MakeCellUnique(cell.getListIndex());
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
                result = (GameObject) Instantiate(laserObjPath, spawnCoords);
                // result.GetComponent<LaserObject>().SetDirection(HexDirection.SW);
                break;
            case 'g':
                result = (GameObject) Instantiate(laserObjPath, spawnCoords);
                // result.GetComponent<LaserObject>().SetDirection(HexDirection.W);
                break;
            case 'y':
                result = (GameObject) Instantiate(laserObjPath, spawnCoords);
                // result.GetComponent<LaserObject>().SetDirection(HexDirection.SE);
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
            //var ComboObj = SpawnComboObjByKey(cell.GetKey(), cell.transform);
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
    //[Command(ignoreAuthority = true)]
    [Server]
    public void SwapHexAndKey(int cellIdx, char heldKey, NetworkIdentity player)
    {
        HexCell cell = gridList[cellIdx];

        cell.CreateModel(this.ReturnModelByCellKey(heldKey)); // updates on the server!
        cell.SetKey(heldKey);

        char oldKey = colorGridList[cellIdx];
        colorGridList[cellIdx] = heldKey;

        List<HexCell> comboCells = cell.FindSameColorTiles(GetMinTilesInCombo());

        if (comboCells.Count > 0)
        {
            eventManager.OnPlayerSwap(heldKey, oldKey, true, player.gameObject);
            player.GetComponent<Player>().AddItemCombo(heldKey);
        }
        else
        {
            eventManager.OnPlayerSwap(heldKey, oldKey, false, player.gameObject);
        }

        ComboCallback(comboCells);

        //RpcSwapHexAndKey(cellIdx, heldKey);
    }

    //// Sync new swapped models for all clients
    //[ClientRpc]
    //public void RpcSwapHexAndKey(int cellIdx, char heldKey)
    //{
    //    HexCell cell = gridList[cellIdx];
    //    cell.DeleteModel();
    //    cell.CreateModel(this.ReturnModelByCellKey(heldKey));
    //    cell.SetKey(heldKey);

    //    List<HexCell> comboCells = cell.FindSameColorTiles(GetMinTilesInCombo());
    //    ComboCallback(comboCells);
    //}


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
        List<HexCell> comboCells = cell.FindSameColorTiles(GetMinTilesInCombo());
        SetListToGlow(comboCells);
        return comboCells.Count != 0;
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

    public static char GetSlowTileChar()
    {
        return slow_Hex_char;
    }

    public static char GetDangerTileChar()
    {
        return danger_Hex_char;
    }

    public static char GetEmptyTileChar()
    {
        return default_Hex_char;
    }
    
    
    public static char GetHiddenHexChar()
    {
        return hidden_Hex_char;
    }

    public static char GetTerrainHexChar()
    {
        return terrain_Hex_char;
    }

    public GameObject test;

    
    public void GrowRing()
    {
        char[] slowSpawns = new char[] {GetDangerTileChar(), GetHiddenHexChar()};
        char[] preDangers = new char[] {GetSlowTileChar()};
        char[] unchanging = new char[] {GetHiddenHexChar(), GetDangerTileChar()};
        char[] allmisc = new char[] {GetHiddenHexChar(), GetDangerTileChar(), GetSlowTileChar()};
        List<HexCell> toSlow = new List<HexCell>();
        List<HexCell> toDanger = new List<HexCell>();
        foreach (var cell in gridList)
        {
            if (!cell) continue;
            if (preDangers.Contains(cell.GetKey()))
            {
                toDanger.Add(cell);
            }
            else if (
                (cell.HasNeighborOf(slowSpawns) || (cell.GetKey() == GetEmptyTileChar() && cell.HasNeighborOf(allmisc) || cell.emptyNeighbors > 0))
                     && !unchanging.Contains(cell.GetKey()))
            {
                toSlow.Add(cell);
            }
        }

        foreach (var cell in toDanger)
        {
            ChangeCell(cell, GetDangerTileChar());
        }

        foreach (var cell in toSlow)
        {
            
            ChangeCell(cell, GetSlowTileChar());
        }
    }

    void ChangeCell(HexCell cell, char key)
    {
        var cellIdx = cell.getListIndex();

        cell.CreateModel(this.ReturnModelByCellKey(key)); // updates on the server!
        cell.SetKey(key);

        char oldKey = colorGridList[cellIdx];
        colorGridList[cellIdx] = key;
    }

    void SetRoundManager(RoundManager rm)
    {
        roundManager = rm;
    }
    
    [Server]
    // TODO make sure every player sees change in ring size
    void HandleRingPhases(int[] delays, int[] playersAlive)
    {
        bool goNextPhase = false;
        phaseTimeElapsed += Time.deltaTime;
        if (phaseCurrent < phaseTotal)
        {
            // check conditions
            if (phaseTimeElapsed > delays[phaseCurrent])
            {
                goNextPhase = true;
            }
            else if (aliveCount != -1 && aliveCount <= phasePlayersAlive[phaseCurrent])
            {
                goNextPhase = true;
            }
        }

        if (goNextPhase)
        {
            GrowRing();
            phaseCurrent++;
            phaseTimeElapsed = 0f;
        }
    }

    public void SetAliveCount(int newAlive)
    {
        aliveCount = newAlive;
    }
    
}