using Godot;
using System;

public partial class CoinCounter : Label
{

	private int Coins;
	public override void _Ready()
	{
		Coins = 0;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Text = Coins.ToString();
	}

	public void IncreaseCount()
	{
		Coins += 1;
	}
}
