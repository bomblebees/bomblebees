using InControl;


// This represents some fictional actions which will appear in the bindings menu,
// and are the controls that the player will be able to customize.
public class GameActions : PlayerActionSet
{
	// public readonly PlayerAction Up;
	// public readonly PlayerAction Down;
	// public readonly PlayerAction Left;
	// public readonly PlayerAction Right;
	// public readonly PlayerTwoAxisAction Move;

	public readonly PlayerAction 
		Swap, 
		Place, 
		Spin,
		NextBomb,
		PreviousBomb,
		BombleBomb,
		HoneyBomb,
		LaserBeem,
		PlasmaBall;

	public GameActions()
	{
		// Up = CreatePlayerAction("Move Up");
		// Down = CreatePlayerAction("Move Down");
		// Left = CreatePlayerAction("Move Left");
		// Right = CreatePlayerAction("Move Right");
		// Move = CreateTwoAxisPlayerAction( Left, Right, Down, Up );

		Swap = CreatePlayerAction("Swap");
		Place = CreatePlayerAction("Place");
		Spin = CreatePlayerAction("Spin");
		NextBomb = CreatePlayerAction("Next Bomb");
		PreviousBomb = CreatePlayerAction("Previous Bomb");
		BombleBomb = CreatePlayerAction("Bomble Bomb");
		HoneyBomb = CreatePlayerAction("Honey Bomb");
		LaserBeem = CreatePlayerAction("Laser Beem");
		PlasmaBall = CreatePlayerAction("Plasma Ball");
	}


	public static GameActions CreateWithDefaultBindings()
	{
		var actions = new GameActions();

		// actions.Up.AddDefaultBinding( Key.UpArrow );
		// actions.Down.AddDefaultBinding( Key.DownArrow );
		// actions.Left.AddDefaultBinding( Key.LeftArrow );
		// actions.Right.AddDefaultBinding( Key.RightArrow );

		actions.Swap.AddDefaultBinding(Key.Space);
		actions.Place.AddDefaultBinding(Key.J);
		actions.Spin.AddDefaultBinding(Key.K);
		actions.NextBomb.AddDefaultBinding(Key.E);
		actions.PreviousBomb.AddDefaultBinding(Key.Q);
		actions.BombleBomb.AddDefaultBinding(Key.Key1);
		actions.HoneyBomb.AddDefaultBinding(Key.Key2);
		actions.LaserBeem.AddDefaultBinding(Key.Key3);
		actions.PlasmaBall.AddDefaultBinding(Key.Key4);

		return actions;
	}
}
