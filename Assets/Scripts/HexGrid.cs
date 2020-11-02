// Possible Optimizations:
//     - Delete empty cells (requires more edge cases in the
//       Neighbor-finding methods)
//
// TODO:
//     - When raycast collides with tile model, get the parent hexcell object,
//       have it delete the model, and then do createModel by passing in the key in the held slot, and take that key into nlot

using System;
using UnityEngine;
using UnityEngine.EventSystems;
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

    HexCell[] cells;
    public Level1 level = new Level1();
    private int width = 0;
    private int height = 0;

    Canvas gridCanvas;
    HexMesh hexMesh;

    void Awake()
    {
        if (level == null
            || cellPrefab == null
            || cellLabelPrefab == null
        )
        {
            Debug.Log("hexArray not assigned");
            Application.Quit();
        }

        if (r_Hex == null || g_Hex == null || b_Hex == null || y_Hex == null) 
            Debug.LogError("Error: a Hex is not assigned.");

        this.width = level.getWidth();
        this.height = level.getHeight();
        this.gridCanvas = GetComponentInChildren<Canvas>();
        this.hexMesh = GetComponentInChildren<HexMesh>();

        cells = new HexCell[height * width];

        int counter = 0;
        for (int z = 0, i = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                CreateCell(x, z, i, 
                    returnModelByCellKey(level.getArray()[z, x])
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
			Input.GetMouseButton(0) &&
			!EventSystem.current.IsPointerOverGameObject()
		) {
			HandleInput();
		}

    }

    //
    // CreateCell: Creates a cell at the given x & z (where x is the 
    //     vertical axis. z is the horizontal axis. These are NOT
    //     part of the axial coordinates system).
    //     
    // 
    HexCell CreateCell(int x, int z, int i, GameObject model)
    {
        Vector3 position;
        position.x = (x +
                      z * 0.5f // Creates a staircase-like effect
                      - z / 2) // This int division creates the zigging back every other line
                     * (HexMetrics.innerRadius * 2f);
        position.y = 0f;
        position.z = z * (HexMetrics.outerRadius * 1.5f);

        HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
        cell.setSpawnCoords(x,z);
        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
        cell.createModel(model);

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

        return cell;
    }
    void HandleInput () {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit)) {
            Debug.Log(hit.point);
        }
    }
}