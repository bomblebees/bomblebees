// Possible Optimizations:
//     - Delete empty cells (requires more edge cases in the
//       Neighbor-finding methods)
//
// TODO:
//     - return level array by ref, when using level.getArray()
//     - When raycast collides with tile model, get the parent hexcell object,
//       have it delete the model, and then do createModel by passing in the key in the held slot, and take that key into nlot

using System;
using System.Collections;
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
    public bool enableCoords = false;
    public char heldKey = 'r';

    HexCell[] cells;
    public Level1 level = new Level1();
    private int width = 0;
    private int height = 0;

    Canvas gridCanvas;
    HexMesh hexMesh;

    void Awake()
    {
        checkErrors();
        getGridDimensions();
        
        this.gridCanvas = GetComponentInChildren<Canvas>();
        this.hexMesh = GetComponentInChildren<HexMesh>();

        generateHexGrid();
    }

    void checkErrors()
    {
        if (level == null
            || cellPrefab == null
            || cellLabelPrefab == null
        ) {
            Debug.Log("hexArray not assigned");
        }

        if (r_Hex == null || g_Hex == null || b_Hex == null || y_Hex == null)
            Debug.LogError("Error: a Hex is not assigned.");
    }

    // getGridDimensions: used once at the start of the level
    void getGridDimensions()
    {
        this.width = level.getWidth();
        this.height = level.getHeight();
        cells = new HexCell[height * width];
    }

    void generateHexGrid()
    {
        for (int z = 0, i = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                CreateCell(
                    x,
                    z,
                    i,
                    level.getArray()[z, x]
                );
                i++;
            }
        }
    }

    GameObject returnModelByCellKey(char key)
    {
        switch (key)
        {
            case 'r':
                return r_Hex;
                break;
            case 'g':
                return g_Hex;
                break;
            case 'b':
                return b_Hex;
                break;
            case 'y':
                return y_Hex;
                break;
            default:
                return default_Hex;
        }
    }


    void Start()
    {
        hexMesh.Triangulate(cells);
    }

    private void Update()
    {
        if (
            Input.GetMouseButtonDown(0) &&
            !EventSystem.current.IsPointerOverGameObject()
        )
        {
            HandleInput();
        }
    }

    //
    // CreateCell: Creates a cell at the given x & z (where x is the 
    //     vertical axis. z is the horizontal axis. These are NOT
    //     part of the axial coordinates system).
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
        cell.createModel(returnModelByCellKey(key));
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

    void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        int tilesLayerMask = 8;

        // TODO change distance
        if ((Physics.Raycast(inputRay, out hit, 500f, 1 << LayerMask.NameToLayer("BaseTiles")) ))
        {
            GameObject modelHit = hit.transform.gameObject;
            Debug.Log("found color: " +(modelHit.GetComponentInParent<HexCell>().getKey()));
            HexCell hexCell = modelHit.GetComponentInParent<HexCell>();
            SwapHex(modelHit); // A coroutine because mouse activates quickly.

            hexCell.isIn3ComboAnywhere(RegenerateTiles);
        }
    }

    void RegenerateTiles(HexCell tile1, HexCell tile2, HexCell tile3)
    {
        Destroy(tile1.getModel());
        Destroy(tile2.getModel());
        Destroy(tile3.getModel());
        // Do effects here
    }

    void JustSayHi()
    {
        Debug.Log("Hi this is a test function");
    }

    void SwapHex(GameObject modelHit)
    {
        Debug.Log("swapped");
            char tempKey = modelHit.GetComponentInParent<HexCell>().getKey();
            HexCell parent = modelHit.GetComponentInParent<HexCell>().getThis(); // The parent of the model
            Destroy(modelHit , 0f); // TODO this doesn't update model to be null but i don't think it matters. Maybe create a HexCell.destroyModel()
            parent.createModel(this.returnModelByCellKey(heldKey));
            parent.setKey(this.heldKey);
            this.heldKey = tempKey;
    }
}