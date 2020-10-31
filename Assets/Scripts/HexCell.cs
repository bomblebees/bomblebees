﻿using UnityEngine;

public class HexCell : MonoBehaviour {

	public HexCoordinates coordinates;
	public GameObject hexModel;

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

	public void createModel()
	{
		Instantiate(hexModel, this.gameObject.transform.position, Quaternion.identity);
	}
	
}