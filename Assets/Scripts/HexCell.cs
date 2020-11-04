using UnityEngine;
using UnityEngine.XR;

public class HexCell : MonoBehaviour {

	public HexCoordinates coordinates;
	public GameObject hexModel;
	public float spawnX = 0;
	public float spawnZ = 0;
	public char key = 'e'; // tile color key
	private GameObject parent;

	public Color color;
	[SerializeField]
	HexCell[] neighbors;
	public HexCell GetNeighbor (HexDirection direction) {
    		return neighbors[(int)direction];
    	}
	public void SetNeighbor (HexDirection direction, HexCell cell) {
		neighbors[(int)direction] = cell;
		cell.neighbors[(int)direction.Opposite()] = this;
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

	public bool isSameColorAs(HexDirection direction)
	{
		return this.key == this.GetNeighbor(direction).getKey();
	}

	public bool isInComboBetween(HexDirection direction1, HexDirection direction2)
	{
		return this.isSameColorAs(direction1) == this.isSameColorAs(direction2);
	}

	public void getComboTiles()
	{
		if (this.isInComboBetween(HexDirection.NW, HexDirection.NW.Opposite()))
		{
			// do something with all 3 of the tiles
			Debug.Log("hi");
		}
		if (this.isInComboBetween(HexDirection.W, HexDirection.NW.Opposite()))
		{
			// do something with all 3 of the tiles
			Debug.Log("hi");
		}
		if (this.isInComboBetween(HexDirection.SW, HexDirection.NW.Opposite()))  
		{
			// do something with all 3 of the tiles
			Debug.Log("hi");
		}
	}
}