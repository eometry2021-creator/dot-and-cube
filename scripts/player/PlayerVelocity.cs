using Godot;

public partial class PlayerVelocity : CharacterBody2D
{
	[Export] // 初始生成坐标
	public Vector2 SpawnPoint = new(384f, 384f);
	private float speed = 275.0f;

    public override void _Ready()
    {
        GlobalPosition = SpawnPoint;
		ZIndex = 255;
    }

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Vector2.Zero;

		if (Input.IsActionPressed("ui_left"))  velocity.X -= 1;
		if (Input.IsActionPressed("ui_right")) velocity.X += 1;
		if (Input.IsActionPressed("ui_up"))    velocity.Y -= 1;
		if (Input.IsActionPressed("ui_down"))  velocity.Y += 1;

		if (velocity != Vector2.Zero)
		{
			velocity = velocity.Normalized() * speed;
		}

		Velocity = velocity;

		MoveAndSlide();
	}
}