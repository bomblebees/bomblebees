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
    public bool isGlowing = false; // Im thinking we connect the glowing by finding samecolorneighbors in each dir

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

    // Returns a reference to the model object that is created
    public GameObject createModel(GameObject model, Vector3 position, Quaternion rotation, Transform parent)
    {
        model = Instantiate(model, position, rotation, parent);
        this.setModel(model);
        return model;
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

    public bool FindCombos(System.Action<List<HexCell>> callback, int minTilesInCombo)
    {
        bool hasAtLeastOneCombo = false;
        List<HexCell> ComboList = new List<HexCell>(15); // To reduce array-doubling
        for (var direction = 0; direction < 3; direction++)
        {
            HexDirection hexDirection = (HexDirection) direction;
            HexDirection oppositeDirection = hexDirection.Opposite();
            CollectSameColorNeighbors(this.GetNeighbor(hexDirection), hexDirection, ComboList);
            CollectSameColorNeighbors(this.GetNeighbor(oppositeDirection), oppositeDirection, ComboList);
            if (ComboList.Count >= minTilesInCombo - 1) // - 1 because of the this tile
            {
                if (!hasAtLeastOneCombo) // Stops "this" from being in callback() twice
                {
                    hasAtLeastOneCombo = true;
                }

                callback(ComboList);
            }

            ComboList.Clear();
        }
        // Kill off this tilei
        if (hasAtLeastOneCombo)
        {
            ComboList.Add(this);
            callback(ComboList);
        }

        return hasAtLeastOneCombo;
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

    public void setGlow(bool val)
    {
        this.isGlowing = val;
        if (model)
        {
            Behaviour halo = (Behaviour) this.getModel().GetComponent("Halo");
            if (halo)
                halo.enabled = val;

            // toggles exposed field of shader appropriately
                // apparently, this is not very efficient way to instance materials
                // we should look into Material Property Blocks
            this.getModel().GetComponent<Renderer>().material.SetFloat("Boolean_CC1856D2", val ? 1f : 0f);
        }
    }

    // Im thinking whenever the glow goes changes, recalculate for its neighbors too

    public bool getGlow()
    {
        return this.isGlowing;
    }

    public bool isEmpty()
    {
        return key == 'e';
    }

    // public void RecalculateGlowForSelf(int minTilesInCombo)
    // {
    //     // findcombo for self
    //     // find combos for every neighbor up to minCombo
    //     RecalculateGlowForNeighbors(2, minTilesInCombo);
    // }

    public void RecalculateGlowForNeighbors(
        System.Action<List<HexCell>> callback, int tileCountFromStarter, int minTilesInCombo, int minTilesForGlow)
    {
        if (tileCountFromStarter >= minTilesInCombo + minTilesForGlow) return; // Don't check for tiles that would have
        // been popped by a combo.
        this.setGlow(false);
        this.FindCombos(callback, minTilesForGlow);
        for (var direction = 0; direction < 6; direction++)
        {
            HexDirection hexDirection = (HexDirection) direction;
            HexCell neighbor = this.GetNeighbor(hexDirection);
            if (neighbor)
            {
                RecalculateGlowForNeighbors(
                    callback, tileCountFromStarter + 1, minTilesInCombo, minTilesForGlow);
            }
        }
    }
}