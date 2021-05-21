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
		ShowInfo,
		ChooseNextBomb,
		ChoosePreviousBomb,
		ChooseBombleBomb,
		ChooseHoneyBomb,
		ChooseLaserBeem,
		ChoosePlasmaBall;

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
		ShowInfo = CreatePlayerAction("Show Info");
		ChooseNextBomb = CreatePlayerAction("Next Bomb");
		ChoosePreviousBomb = CreatePlayerAction("Previous Bomb");
		ChooseBombleBomb = CreatePlayerAction("Bomble Bomb");
		ChooseHoneyBomb = CreatePlayerAction("Honey Bomb");
		ChooseLaserBeem = CreatePlayerAction("Laser Beem");
		ChoosePlasmaBall = CreatePlayerAction("Plasma Ball");
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
		actions.ShowInfo.AddDefaultBinding(Key.Tab);
		actions.ChooseNextBomb.AddDefaultBinding(Key.E);
		actions.ChoosePreviousBomb.AddDefaultBinding(Key.Q);
		actions.ChooseBombleBomb.AddDefaultBinding(Key.Key1);
		actions.ChooseHoneyBomb.AddDefaultBinding(Key.Key2);
		actions.ChooseLaserBeem.AddDefaultBinding(Key.Key3);
		actions.ChoosePlasmaBall.AddDefaultBinding(Key.Key4);

		return actions;
	}
}
