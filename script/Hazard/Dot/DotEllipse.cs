using Godot;
public partial class DotEllipse : Node2D
{
    // ===== 运动参数 =====
    [Export] // 椭圆圆心坐标
    public Vector2 OrbitCenter { get; set; } = new(440f, 440f);
    [Export] // 横半轴
    public float XRadius { get; set; } = 120f;
    [Export] // 纵半轴
    public float YRadius { get; set; } = 120f;
    [Export] // 周期
    public float Period { get; set; } = 2f;
    [Export] // 初始角度（角度制）
    public float InitialAngleDeg { get; set; } = 0f;
    public enum RotationEnum
    {
        Clockwise,
        Counterclockwise
    }
    [Export] // 转动方向
    public RotationEnum RotationDirection { get; set; } = RotationEnum.Counterclockwise;
    // ===== 内部变量 =====
    private double _time = 0.0;
	private bool _isValid = false;
    private float _initialAngleRad;
    private float _angularVelocity;
	private int _rotationDirection;

	// 初始化
    public override void _Ready()
    {
		if (Period <= 0.01f)
		{
			GD.PrintErr("错误：周期必须大于 0。");
			return;
		}
		if (XRadius <= 0.01f || YRadius <= 0.01f)
		{
			GD.PrintErr("错误：横半轴或纵半轴必须大于 0。");
			return;
		}
		_isValid = true;

        _angularVelocity = 2 * Mathf.Pi / Period;
        _initialAngleRad = Mathf.DegToRad(InitialAngleDeg);
		(float sinValue, float cosValue) = Mathf.SinCos(_initialAngleRad);
        Vector2 initialPosition = new(XRadius * cosValue, YRadius * sinValue);
        GlobalPosition = OrbitCenter + initialPosition;

		_rotationDirection = RotationDirection == RotationEnum.Clockwise
			? -1
			: 1;
        _angularVelocity *= _rotationDirection;
    }

	// 方法：更新坐标
	private void UpdatePosition()
	{
		float angle = (float)(_angularVelocity * _time + _initialAngleRad);
		(float sinValue, float cosValue) = Mathf.SinCos(angle);
		Vector2 distance = new(cosValue * XRadius, sinValue * YRadius);
		GlobalPosition = OrbitCenter + distance;
	}

    public override void _Process(double delta)
    {
		if (!_isValid) return;

        _time += delta;
        while (_time >= Period)
        {
            _time -= Period;
        }
		UpdatePosition();
    }
}