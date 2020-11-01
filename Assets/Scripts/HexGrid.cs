// Possible Optimizations:
//     - Delete empty cells (requires more edge cases in the
//       Neighbor-finding methods)
//

using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{
    public Color defaultColor = Color.white;

    public HexCell cellPrefab;
    public Text cellLabelPrefab;
    public GameObject b_Hex;
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

        if (b_Hex == null) Debug.LogError("b_Hex is not defined.");

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
                // Debug.Log(i);
                CreateModelByCellKey(
                    level.getArray()[z, x],
                    CreateCell(x, z, i)
                );
                i++;
            }
        }
    }

    void CreateModelByCellKey(char key, HexCell parent)
    {
        switch (key)
        {
            case 'd':
                Instantiate(b_Hex, parent.transform);
                break;
            default:
                Instantiate(default_Hex, parent.transform);
                return;
        }
    }


    void Start()
    {
        hexMesh.Triangulate(cells);
    }

    public void ColorCell(Vector3 position, Color color)
    {
        position = transform.InverseTransformPoint(position);
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);
        int index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
        HexCell cell = cells[index];
        cell.color = color;
        hexMesh.Triangulate(cells);
    }

    //
    // CreateCell: Creates a cell at the given x & z (where x is the 
    //     vertical axis. z is the horizontal axis. These are NOT
    //     part of the axial coordinates system).
    //     
    // 
    HexCell CreateCell(int x, int z, int i)
    {
        Vector3 position;
        position.x = (x +
                      z * 0.5f // Creates a staircase-like effect
                      - z / 2) // This int division creates the zigging back every other line
                     * (HexMetrics.innerRadius * 2f);
        position.y = 0f;
        position.z = z * (HexMetrics.outerRadius * 1.5f);

        HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
        // cell.createModel();

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
}