using Godot;
public partial class DotSine : DotBase
{
	// ===== 导出参数 =====
	[Export] // 世界坐标（平衡位置）
	public Vector2 Origin = new(0f, 0f);
	[Export] // 振幅
	public float Amplitude = 120f;
	[Export] // 周期
	public float Period = 3f;
	[Export] // 初相位（角度制）
	public float PhaseOffsetDeg = -90f;
	[Export] // 运动方向与世界 X 轴的夹角（角度制）
	public float AngleDeg = 0f;

	// ===== 内部变量 =====
	private Vector2 _direction; // 单位向量
	private float _angularVelocity;
	private float _phaseOffsetRad;
	private float _angleRad;
	// TODO：架构优化，从字段降级为局部变量

	protected override void Initialize()
	{
		_period = Period;
		// 计算角速度 ω = 2π / T
		if (_period <= 0.01f)
		{
			GD.PrintErr("周期必须大于 0。");
			return;
		}
		_isValid = true;
		_angularVelocity = Mathf.Tau / _period;
		// 初相位弧度
		_phaseOffsetRad = Mathf.DegToRad(PhaseOffsetDeg);
		// 角度转弧度
		_angleRad = Mathf.DegToRad(AngleDeg);
		// 单位方向向量
		(float sinValue, float cosValue) = Mathf.SinCos(_angleRad);
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