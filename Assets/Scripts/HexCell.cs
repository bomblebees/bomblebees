using UnityEngine;
using UnityEngine.XR;

public class HexCell : MonoBehaviour
{
    public HexCoordinates coordinates;
    public GameObject model;
    public float spawnX = 0;
    public float spawnZ = 0;
    public char key = 'e'; // tile color key
    private GameObject parent;

    public Color color;
    [SerializeField] HexCell[] neighbors;

    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int) direction];
    }

    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors[(int) direction] = cell;
        cell.neighbors[(int) direction.Opposite()] = this;
    }

    public void setSpawnCoords(int x, int z)
    {
        spawnX = x;
        spawnZ = z;
    }

    public float getSpawnX()
    {
        return spawnX;
    }

    public float getSpawnZ()
    {
        return spawnZ;
    }

    public void createModel(GameObject model)
    {
        model = Instantiate(model, this.gameObject.transform);
        this.setModel(model);
    }

    public void setKey(char key)
    {
        this.key = key;
    }

    public char getKey()
    {
        return this.key;
    }

    public HexCell getThis()
    {
        return this;
    }

    public void isIn3ComboAnywhere(System.Action<HexCell, HexCell, HexCell> callback)
    {
        for (var direction = 0; direction < 5; direction++)
        {
            HexDirection hexDirection = (HexDirection) direction;
            HexCell neighbor = this.GetNeighbor(hexDirection);

            if (this.isSameColorAs(neighbor))
            {
                // check if at tail. 
                HexCell outer = neighbor.GetNeighbor(hexDirection);
                if (this.isSameColorAs(outer))
                {
                    callback(this, neighbor, outer);
                    break;
                }
                
                // check if in middle
                if (direction < 3) { // Removes redundancy
                    HexCell oppositeNeighbor = this.GetNeighbor(hexDirection.Opposite());
                    if (this.isSameColorAs(oppositeNeighbor))
                    {
                        callback(this, neighbor, oppositeNeighbor);
                        break;
                    }
                }
            }
        }
    }

    public bool isSameColorAs(HexCell cell)
    {
        if (cell == null) return false;
        return this.key == cell.getKey();
    }

    public void setModel(GameObject model)
    {
        this.model = model;
    }

    public GameObject getModel()
    {
        if (this.model == null)
        {
            Debug.LogError("Error: Model not set");
        }

        return this.model;
    }
}