using Godot;
public partial class DotSine : DotBase
{
	// ===== 导出参数 =====
	[Export] // 平衡中心
	public Vector2 Origin { get; set; } = new(0f, 0f);
	[Export] // 振幅
	public float Amplitude { get; set; } = 120f;
	[Export] // 周期
	public float Period { get; set; } = 3f;
	[Export] // 初相位（角度制）
	public float PhaseOffsetDeg { get; set; } = -90f;
	[Export] // 运动方向与世界 X 轴的夹角（角度制）
	public float AngleDeg { get; set; } = 0f;

	// ===== 内部变量 =====
	private Vector2 _direction; // 单位方向向量
	private float _angularVelocity;
	private float _phaseOffsetRad;

	protected override void Initialize()
	{
		_period = Period;
		if (_period <= 0.01f)
		{
			GD.PrintErr("周期必须大于 0。");
			return;
		}
		_isValid = true;

		// 计算角速度 ω = 2π / T
		_angularVelocity = Mathf.Tau / _period;
		// 初相位弧度
		_phaseOffsetRad = Mathf.DegToRad(PhaseOffsetDeg);
		// 旋转角弧度
		float angleRad = Mathf.DegToRad(AngleDeg);
		// 单位方向向量
		(float sinValue, float cosValue) = Mathf.SinCos(angleRad);
		_direction = new(cosValue, sinValue);
		// 按照初相位设置初始位置
		float initialDisplacement = Amplitude * Mathf.Sin(_phaseOffsetRad);
		GlobalPosition = Origin + _direction * initialDisplacement;
	}

	// 方法：更新坐标
	protected override void UpdatePosition()
	{
		GlobalPosition = Origin + _direction * Amplitude * Mathf.Sin((float)(_angularVelocity * _time + _phaseOffsetRad));
	}
}