using Godot;
public partial class ObstacleSine : Node2D
{
	// ===== 运动参数 =====
	[Export] // 世界坐标（平衡位置）
	public Vector2 Origin = new(0f, 0f);
	[Export] // 振幅
	public float Amplitude = 120f;
	[Export] // 周期
	public float Period = Mathf.Pi;
	[Export] // 初相位（角度制）
	public float PhaseOffsetDeg = -90f;
	[Export] // 运动方向与世界 X 轴的夹角（角度制）
	public float AngleDeg = 0f;
	// ===== 内部变量 =====
	private double _time = 0.0;
	private Vector2 _direction; // 单位向量
	private float _angularVelocity; // 角速度
	private float _phaseOffsetRad;
	private float _angleRad;

	// 初始化
	public override void _Ready()
	{
		// 计算角速度 ω = 2π / T
		_angularVelocity = Period > 0.01f ? 2f * Mathf.Pi / Period : 0f; // 防止除以 0
		// 初相位弧度
		_phaseOffsetRad = Mathf.DegToRad(PhaseOffsetDeg);
		// 角度转弧度
		_angleRad = Mathf.DegToRad(AngleDeg);
		// 单位方向向量
		_direction = new(Mathf.Cos(_angleRad), Mathf.Sin(_angleRad));
		// 按照初相位设置初始位置
		float initialDisplacement = Amplitude * Mathf.Sin(_phaseOffsetRad);
		GlobalPosition = Origin + _direction * initialDisplacement;
	}

	// 方法：更新坐标
	private void UpdatePosition()
	{
		GlobalPosition = Origin + _direction * Amplitude * Mathf.Sin((float)(_angularVelocity * _time + _phaseOffsetRad));
	}

	public override void _Process(double delta)
	{
		_time += delta;
		// 让时间永远锁在 [0, Period) 的区间内，以防 _time 无限增长导致浮点精度下降
		while (_time >= Period)
		{
			_time -= Period;
		}
		UpdatePosition();
	}
}
