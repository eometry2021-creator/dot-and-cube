using System;
using Godot;

public partial class DotPetal : DotBase
{
	// ===== 导出参数 =====
	[Export] // 花朵中心
	public Vector2 Center { get; set; } = new(500f, 500f);
	[Export] // 花瓣数
	public int PetalCount { get; set; } = 3;
	[Export] // 最大半径
	public float MaxRadius { get; set; } = 200f;
	[Export] // 每瓣周期
	public float PetalPeriod { get; set; } = 2f;
	[Export] // 初相位（角度制）
	public float PhaseOffsetDeg { get; set; } = 0f;
	[Export] // 旋转角度
	public float RotationDeg { get; set; } = 0f;

	// ===== 内部变量 =====
	private Vector2 _unrotatedPosition; // 未经旋转的坐标
	private float _phaseOffsetRad; // 初相位（弧度制）
	private float _innerPeriod; // 几何闭合周期
	private float _theta; // 极坐标自变量参数
	private float _periodScale; // 周期放缩比例，等于 _innerPeriod / _period
	private int _n; // r = a cos(nθ) 中的参数 n

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

		_period = PetalCount * PetalPeriod;
		_n = PetalCount % 2 == 1
			? PetalCount
			: PetalCount / 2;
		_innerPeriod = PetalCount % 2 == 1
			? Mathf.Pi
			: Mathf.Tau;
		_periodScale = _innerPeriod / _period;
		_phaseOffsetRad = Mathf.DegToRad(PhaseOffsetDeg);
    }

    protected override void UpdatePosition()
    {
		// 计算 θ，由 _time / _period = θ / T 推导而来
		_theta = _periodScale * (float)_time;
		// 加上初相位
		_theta += _phaseOffsetRad * _innerPeriod / Mathf.Tau;

		// 极坐标转直角坐标
		(float sinValue, float cosValue) = Mathf.SinCos(_theta);
		float nCosValue = Mathf.Cos(_n * _theta);
		float xPos = MaxRadius * nCosValue * cosValue;
		float yPos = MaxRadius * nCosValue * sinValue;
		_unrotatedPosition = new(xPos, yPos);
		// 旋转
		GlobalPosition = Center + _unrotatedPosition.Rotated(Mathf.DegToRad(RotationDeg));
    }
}