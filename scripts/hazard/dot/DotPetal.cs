using Godot;

public partial class DotPetal : DotBase
{
	// ===== 导出参数 =====
	[Export] // 平衡中心
	private Vector2 Origin { get; set; } = new(300f, 300f);
	[Export] // 花瓣数
	private int PetalCount { get; set; } = 3;
	[Export] // 振幅
	private float Amplitude { get; set; } = 100f;
	[Export] // 每瓣周期
	private float PetalPeriod { get; set; } = 2f;
	[Export] // 初相位（角度制）
	private float PhaseOffsetDeg { get; set; } = 0f;
	[Export] // 旋转角度
	private float AngleDeg { get; set; } = 0f;

	// ===== 内部变量 =====
	private Vector2 _direction; // 单位方向向量
	private float _angularVelocity;
	private float _phaseOffsetRad;

    protected override void Initialize()
    {
		if (PetalCount <= 0.01f)
		{
			GD.PrintErr("每瓣周期必须大于 0。");
			return;
		}
		if (PetalCount < 3 || PetalCount > 20)
		{
			GD.PrintErr("花瓣数过小或过大。");
			return;
		}
		_isValid = true;
		_period = PetalCount * PetalPeriod;

		// 计算角速度 ω = 2π / T
		_angularVelocity = Mathf.Tau / _period;
		// 初相位弧度
		_phaseOffsetRad = Mathf.DegToRad(PhaseOffsetDeg);
		// 旋转角弧度
		float angleRad = Mathf.DegToRad(AngleDeg);
		// 单位方向向量
		(float sinValue, float cosValue) = Mathf.SinCos(angleRad);
		_direction = new(cosValue, sinValue);

    }

    protected override void UpdatePosition()
    {
        
    }
}
