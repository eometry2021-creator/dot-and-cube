using Godot;
public partial class DotSine : DotBase
{
	// ===== 导出参数 =====
	[Export] // 平衡中心
	public Vector2 Center { get; set; } = new(200f, 200f);
	[Export] // 振幅
	public float Amplitude { get; set; } = 120f;
	[Export] // 周期
	public float Period { get; set; } = 3f;
	[Export] // 初相位（角度制）
	public float PhaseOffsetDeg { get; set; } = -90f;
	[Export] // 旋转角度（角度制）
	public float RotationDeg { get; set; } = 0f;

	// ===== 内部变量 =====
	private Vector2 _motionAxis;    // 方向向量（乘以振幅）
	private float _angularVelocity; // 角速度
	private float _phaseOffsetRad;  // 初相位（弧度制）

	protected override void Initialize()
	{
		if (Period <= 0.01f)
		{
			GD.PrintErr("周期必须大于 0。");
			return;
		}
		_isValid = true;
		_period = Period;

		// 计算角速度 ω = 2π / T
		_angularVelocity = Mathf.Tau / _period;
		// 初相位弧度
		_phaseOffsetRad = Mathf.DegToRad(PhaseOffsetDeg);
		// 旋转角弧度s
		float rotationRad = Mathf.DegToRad(RotationDeg);
		// 方向向量
		(float sinValue, float cosValue) = Mathf.SinCos(rotationRad);
		_motionAxis = new(cosValue, sinValue);
		_motionAxis *= Amplitude;
		// 按照初相位设置初始位置
		float initialRotation = Mathf.Sin(_phaseOffsetRad);
		GlobalPosition = Center + _motionAxis * initialRotation;
	}

	// 方法：更新坐标
	protected override void UpdatePosition()
	{
		GlobalPosition = Center + _motionAxis * Mathf.Sin((float)(_angularVelocity * _time + _phaseOffsetRad));
	}
}