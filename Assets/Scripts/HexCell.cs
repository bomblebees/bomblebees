using UnityEngine;
using UnityEngine.XR;

public class HexCell : MonoBehaviour {

	public HexCoordinates coordinates;
	public GameObject hexModel;
	public float spawnX = 0;
	public float spawnZ = 0;

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
	    Instantiate(model, this.gameObject.transform);
    }
}