using Godot;
using System;

public partial class player : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();


	public override void _Ready()
	{
		animations = GetNode<AnimationTree>("AnimationTree");
		stateMachine = (AnimationNodeStateMachinePlayback)animations.Get("parameters/playback");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		if (Input.IsActionJustPressed("jump") && IsOnFloor())
			velocity.Y = JumpVelocity;


		Vector2 inputDir = Input.GetVector("left", "right", "forward", "backward");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
			GetNode<Node3D>("Rig").LookAt(Position + direction, Vector3.Up);
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		handle_animation(direction, delta);
		Velocity = velocity;
		MoveAndSlide();
	}


	private AnimationTree animations;
	private AnimationNodeStateMachinePlayback stateMachine;
	private bool jumping = true;
	private bool last_floor = true;
	private void handle_animation(Vector3 direction, double delta)
	{
		Vector3 velocity = Velocity;
		velocity = velocity.Lerp(direction * Speed, (float)delta);
		var vl = velocity * Transform.Basis;
		animations.Set("parameters/IWR/blend_position", new Vector2(vl.X, -vl.Z) / Speed);
		handle_jump_animation();
	}

	private void handle_jump_animation()
	{
		if (IsOnFloor() && Input.IsActionJustPressed("jump"))
		{
			jumping = true;
			animations.Set("parameters/conditions/grounded", false);
		}
		animations.Set("parameters/conditions/jumping", jumping);

		if (IsOnFloor() && !last_floor)
		{
			jumping = false;
			animations.Set("parameters/conditions/grounded", true);
		}
		last_floor = IsOnFloor();
		if (!IsOnFloor() && !jumping)
		{
			stateMachine.Travel("Jump_Idle");
			animations.Set("parameters/conditions/grounded", false);
		}
	}

}
