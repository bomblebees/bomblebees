using UnityEngine;
using UnityEngine.XR;

public class HexCell : MonoBehaviour {

	public HexCoordinates coordinates;
	public GameObject hexModel;
	public float spawnX = 0;
	public float spawnZ = 0;
	public char key = 'e';
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
}