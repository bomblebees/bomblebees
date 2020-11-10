using System.Collections.Generic;
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

    public void isInCombo(System.Action<List<HexCell>> callback, int minTilesInCombo)
    {
        bool hasAtLeastOneCombo = false;
        List<HexCell> ComboList = new List<HexCell>(15); // To reduce array-doubling
        for (var direction = 0; direction < 3; direction++)
        {
            HexDirection hexDirection = (HexDirection) direction;
            HexDirection oppositeDirection = hexDirection.Opposite();
            CollectSameColorNeighbors(this.GetNeighbor(hexDirection), hexDirection, ComboList);
            CollectSameColorNeighbors(this.GetNeighbor(oppositeDirection), oppositeDirection, ComboList);
            if (ComboList.Count >= minTilesInCombo - 1)
            {
                if (!hasAtLeastOneCombo) // Stops "this" from being in callback() twice
                {
                    ComboList.Add(this);
                    hasAtLeastOneCombo = true;
                }
                callback(ComboList);
            }

            ComboList.Clear();
        }
    }

    public void CollectSameColorNeighbors(HexCell neighbor, HexDirection hexDirection, List<HexCell> list)
    {
        if (neighbor && this.isSameColorAs(neighbor))
        {
            list.Add(neighbor);
            CollectSameColorNeighbors(neighbor.GetNeighbor(hexDirection), hexDirection, list);
        }

        return;
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
            Debug.LogWarning("Error: hexCell model not set. Not a big deal.");
        }

        return this.model;
    }
}