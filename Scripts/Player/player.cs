using Godot;
using System;

public partial class player : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float Acceleration = 4.0f;
	public const float JumpVelocity = 7.5f;
	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = 2.5f + ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public float MouseSensitive = 0.0005f;
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
		velocity = GetMoveInput(delta);

		if (GlobalPosition.Y < -10)
			GlobalPosition = new Vector3(0, 0, 0);

		velocity = HandleJump(velocity, delta);
		Velocity = velocity;
		MoveAndSlide();
	}


	private SpringArm3D spring_arm;
	private Vector3 GetMoveInput(double delta)
	{
		Vector3 velocity = Velocity;
		float vy = Velocity.Y;
		velocity.Y = 0;
		Vector2 input = Input.GetVector("left", "right", "forward", "backward");
		Vector3 dir = new Vector3(input.X, 0, input.Y).Rotated(Vector3.Up, Rotation.Y);

		velocity = velocity.Lerp(dir * Speed, Acceleration * (float)delta);
		HandleAnimation(velocity);

		velocity.Y = vy;
		return velocity;
	}

	private bool can_jump = false;
	private Vector3 HandleJump(Vector3 velocity, double delta)
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
	private float _rotationX = 0f;


	public override void _Input(InputEvent @event)
	{
		if (!Input.IsMouseButtonPressed(MouseButton.Right)) return;
		if (@event is InputEventMouseMotion mouseMotion)
		{
			// modify accumulated mouse rotation
			_rotationX += mouseMotion.Relative.X * MouseSensitive;

			// reset rotation
			Transform3D transform = Transform;
			transform.Basis = Basis.Identity;
			Transform = transform;

			var rotation = Rotation;
			rotation.Y -= mouseMotion.Relative.X * MouseSensitive;
			Rotation = rotation;

			Rotate(Vector3.Up, _rotationX * -1);
		}
	}

	private Node3D Model;
	private AnimationTree animations;
	private AnimationNodeStateMachinePlayback stateMachine;
	private void HandleAnimation(Vector3 velocity)
	{
		var vl = velocity * Model.Transform.Basis;
		animations.Set("parameters/IWR/blend_position", new Vector2(vl.Z, vl.X) / Speed);
		HandleJumpAnimation();
	}


	private bool jumping = true;
	private bool last_floor = true;
	private void HandleJumpAnimation()
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
