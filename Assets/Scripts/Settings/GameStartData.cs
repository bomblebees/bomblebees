public struct GameStartData
{
	public GameStartData(int numLives, int maxHP)
	{
		NumLives = numLives;
		MaxHP = maxHP;
	}
	public int NumLives { get; set; }
	public int MaxHP { get; set; }
}
