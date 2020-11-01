using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{

    public Color defaultColor = Color.white;

    public HexCell cellPrefab;
    public Text cellLabelPrefab;

    HexCell[] cells;
    public Level1 level = new Level1();
    public int width = 0;  
    public int height = 0; 

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
        this.width = level.getWidth();
        this.height = level.getHeight();
        this.gridCanvas = GetComponentInChildren<Canvas>();
        this.hexMesh = GetComponentInChildren<HexMesh>();

        cells = new HexCell[height * width];

        for (int z = 0, i = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                if (level.getArray()[z, x] == 'b')
                {
                    CreateCell(x, z, i);
                }
                else
                {
                    
                }
                i++;
            }
        }
    }

    void CreateModelByCellKey(char key, HexCell parent)
    {
        switch (key)
        {
            case 'd':
                // Instantiate(BaseHex, parent.transform, Quaternion.identity, parent.transform);
                // I think we create a new prefab BaseHex, which is a subclass ef HexCell
                Instantiate(BaseHex, parent.transform);
                // Instantiate hexCell type, which is a hexCell subclass
                // Using Instantiates argument param
            default:
                Instantiate(EmptyHex, parent.transform);
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
    void CreateCell(int x, int z, int i, char key)
    {
        Vector3 position;
        position.x = (x + 
                      z * 0.5f  // Creates a staircase-like effect
                      - z / 2)  // This int division creates the zigging back every other line
                     * (HexMetrics.innerRadius * 2f);
        position.y = 0f;
        position.z = z * (HexMetrics.outerRadius * 1.5f);

        HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
        CreateModelByCellKey(key, cell);
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

    }
}