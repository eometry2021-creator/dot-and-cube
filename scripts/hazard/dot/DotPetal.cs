using Godot;

public partial class DotPetal : DotBase
{
	// ===== 导出参数 =====
	[Export] // 花朵中心
	public Vector2 Center { get; set; } = new(500f, 500f);
	[Export(PropertyHint.Range, "3, 20")] // 花瓣数
	public int PetalCount { get; set; } = 3;
	[Export] // 最大半径
	public float MaxRadius { get; set; } = 200f;
	[Export] // 每瓣周期
	public float PetalPeriod { get; set; } = 2f;
	[Export(PropertyHint.Range, "-180.0, 180.0, 0.1")] // 初相位（角度制）
	public float PhaseOffsetDeg { get; set; } = 0f;
	[Export(PropertyHint.Range, "-180.0, 180.0, 0.1")] // 旋转角度（角度制）
	public float RotationDeg { get; set; } = 0f;

	// ===== 内部变量 =====
	private float _rotationRad;    // 旋转角度（弧度制）
	private float _thetaPeriod;    // 几何闭合周期，θ 的最大取值
	private float _periodScale;    // 周期缩放比例，等于 _thetaPeriod / _period
	private float _phaseShiftRad;  // θ 初值，由 PhaseOffsetDeg 简单推导而来
	private int _n;                // r = a cos(nθ) 中的参数 n

    protected override void Initialize()
    {
		if (PetalPeriod <= 0.01f)
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

		// 时间周期、几何周期与周期缩放比例
		_period = PetalCount * PetalPeriod;
		_thetaPeriod = PetalCount % 2 == 1 ? Mathf.Pi : Mathf.Tau;
		_periodScale = _thetaPeriod / _period;
		// 角度转弧度
		float phaseOffsetRad = Mathf.DegToRad(PhaseOffsetDeg);
		_rotationRad = Mathf.DegToRad(RotationDeg);
		// 初相位
		_phaseShiftRad = phaseOffsetRad * _thetaPeriod / Mathf.Tau;
		// 奇偶分支参数
		_n = PetalCount % 2 == 1 ? PetalCount : PetalCount / 2;

		UpdatePosition();
    }

    protected override void UpdatePosition()
    {
		float theta = _periodScale * (float)_time + _phaseShiftRad;
		float radius = MaxRadius * Mathf.Cos(_n * theta);
		Vector2 direction = Vector2.FromAngle(_rotationRad + theta);
		GlobalPosition = Center + direction * radius;
    }
}