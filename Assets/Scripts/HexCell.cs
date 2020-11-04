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

	public bool isInTailCombo(HexDirection direction)
	{
		HexCell firstNeighbor = this.GetNeighbor(direction);
		HexCell secondNeighbor = firstNeighbor.GetNeighbor(direction);
		return this.getKey() == firstNeighbor.getKey() 
		       && firstNeighbor.getKey() == secondNeighbor.getKey();
	}

	// public void isInMiddleComboAnywhere(System.Action callback)
	public void isIn3ComboAnywhere()
	{
		for (var direction = 0; direction < 5; direction++)
		{
			HexDirection hexDirection = (HexDirection) direction;
			HexCell neighbor = this.GetNeighbor(hexDirection);
			
			if (this.isSameColorAs(neighbor))
			{
				// check if in middle
				// TODO remove redundancy by only performing this for 3
				HexCell oppositeNeighbor = this.GetNeighbor(hexDirection.Opposite());
				if (this.isSameColorAs(oppositeNeighbor))
				{
					// do smth
					// callback();
					Debug.Log("middle");
				}

				// check if at tail. 
				HexCell outer = neighbor.GetNeighbor(hexDirection);
				if (this.isSameColorAs(outer))
				{
					// do smth
					Debug.Log("outer");
				}
			}
		}
	}

	public bool isSameColorAs(HexCell cell)
	{
		if (cell == null) return false;
		return this.key == cell.getKey();
	} 
	// public bool isSameColorAsOpposite(HexDirection direction)
	// {
	// 	return this.key == this.GetNeighbor(direction.Opposite()).getKey();
	// } 
	//
	// public bool isSameColorAsAdjacent(HexDirection direction)
	// {
	// 	return this.key == this.GetNeighbor(direction).getKey();
	// }
	//
	// public HexCell getSecondaryNeighbor(HexDirection direction)
	// {
	//
	// 	return this.GetNeighbor(direction).GetNeighbor(direction);
	// }
}