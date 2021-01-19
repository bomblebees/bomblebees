using UnityEngine;

public static class HexMetrics
{

	public const string hexCellName = "Hex Cell";  // the file name of the hex cell prefab
	public const float outerRadius = 10f;
	public const int hexSize = 17;

	public const float innerRadius = outerRadius * 0.866025404f;

	public static Vector3[] corners = {
		new Vector3(0f, 0f, outerRadius),
		new Vector3(innerRadius, 0f, 0.5f * outerRadius),
		new Vector3(innerRadius, 0f, -0.5f * outerRadius),
		new Vector3(0f, 0f, -outerRadius),
		new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
		new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
		new Vector3(0f, 0f, outerRadius)
	};
	
	// Used to move comboObjects in direction of hex edges. Note: -30 and 330 are the same, and 30 and 390 are the same
	public static float[] edgeAngles =
	{
		30, 90, 150, 210, 270, 330
	};

	// This math needs verification  - Ari
	// Normalized (1.1/3f, 0, 1.9/3f)
	public static Vector3[] edgeDirections =
	{
		new Vector3( 11/Mathf.Sqrt(482), 0f, 19/Mathf.Sqrt(482)),     // 30 deg or  bottom right
		new Vector3(1f, 0f, 0f),        					        // 90 deg or  right
		new Vector3(11/Mathf.Sqrt(482), 0f, -19/Mathf.Sqrt(482)),    // 150 deg or  top right
		new Vector3(-11/Mathf.Sqrt(482), 0f, -19/Mathf.Sqrt(482)),   // 210 deg or  top left
		new Vector3(-1f, 0f, 0f),                                // 270 deg or left
		new Vector3(-11/Mathf.Sqrt(482), 0f, 19/Mathf.Sqrt(482)),    // 330 deg or  bottom left
	};
	
	public static string GetHexCellPrefabName()
	{
		return hexCellName;
	}
}