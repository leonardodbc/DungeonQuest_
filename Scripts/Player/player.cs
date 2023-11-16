using Godot;
using System;

public partial class player : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float Acceleration = 4.0f;
	public const float JumpVelocity = 7.5f;
	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = 2.5f + ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _Ready()
	{
		Model = GetNode<Node3D>("Rig");
		animations = GetNode<AnimationTree>("AnimationTree");
		stateMachine = (AnimationNodeStateMachinePlayback)animations.Get("parameters/playback");
		spring_arm = GetNode<SpringArm3D>("SpringArm3D");
	}
	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;
		velocity.Y += -gravity * (float)delta;
		velocity = get_move_input(delta);

		if (GlobalPosition.Y < -10)
			GlobalPosition = new Vector3(0, 0, 0);

		velocity = handle_jump(velocity, delta);
		Velocity = velocity;
		MoveAndSlide();
	}


	private SpringArm3D spring_arm;
	private Vector3 get_move_input(double delta)
	{
		Vector3 velocity = Velocity;
		float vy = Velocity.Y;
		velocity.Y = 0;
		Vector2 input = Input.GetVector("left", "right", "forward", "backward");
		Vector3 dir = new Vector3(-input.Y, 0, input.X).Rotated(Vector3.Up, spring_arm.Rotation.Y);

		velocity = velocity.Lerp(dir * Speed, Acceleration * (float)delta);
		handle_animation(velocity);

		velocity.Y = vy;
		return velocity;
	}

	private bool can_jump = false;
	private Vector3 handle_jump(Vector3 velocity, double delta)
	{
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
			can_jump = true;
		}
		if (Input.IsActionJustPressed("jump") && can_jump && !IsOnFloor())
		{
			can_jump = false;
			velocity.Y = JumpVelocity;
			stateMachine.Travel("Jump_Start");
			animations.Set("parameters/conditions/grounded", false);
		}
		return velocity;
	}



	private Node3D Model;
	private AnimationTree animations;
	private AnimationNodeStateMachinePlayback stateMachine;
	private void handle_animation(Vector3 velocity)
	{
		var vl = velocity * Model.Transform.Basis;
		animations.Set("parameters/IWR/blend_position", new Vector2(vl.Z, vl.X) / Speed);
		handle_jump_animation();
	}


	private bool jumping = true;
	private bool last_floor = true;
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
