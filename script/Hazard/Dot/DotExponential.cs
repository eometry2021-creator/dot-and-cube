using Godot;

public partial class DotExponential : Node2D
{
	// ===== 运动参数 =====
	[Export] // 运动时间
	public float MoveTime { get; set; } = 0.5f;
    [Export] // 停顿时间
    public float WaitTime { get; set; } = 0.5f;
    [Export(PropertyHint.Range, "4.0,12.0,0.2")] // 指数衰减率，f(t) = 1 - 2 ^ (-r × t) 中的 r
    public float DecayRate { get; set; } = 6.0f;
	// 两种路径模式的枚举
	public enum PathModeEnum
	{
		ClosedLoop,
		BackAndForth
	}
	[Export] // 路径模式
	public PathModeEnum PathMode { get; set; } = PathModeEnum.ClosedLoop;

	[Export] // 路径点坐标
	public Vector2[] Waypoints { get; set; } =
	{
		new(350, 200),
		new(200, 459.81f),
		new(500, 459.81f)
	};
	// ===== 内部变量 =====
	private const float INV_LN2 = 1.442695f; // 1 / ln(2)
	
	private double _time = 0.0;
	private bool _isValid = false;
	private float[] _timeScales;      // 时间缩放系数表
	private float _period;            // 周期
	private float _stepTime;          // 每步（径段）时间，等于 MoveTime + WaitTime
	private int _totalSegment;        // 总径段数，等于数组 _timeScales 的元素个数
	private int _currentSegmentIndex; // 当前径段索引，动点处于 Waypoints[i] 与 Waypoints[i+1] 的区间

	// 初始化
	public override void _Ready()
	{
		if (Waypoints.Length < 2)
		{
			GD.PushError("错误：列表 Waypoints 至少需要 2 个元素。");
			return;
		}
		if (MoveTime <= 0.01f || WaitTime <= 0.01f)
		{
			GD.PushError("错误：移动时间或停顿时间必须大于 0。");
			return;
		}
		_isValid = true;

		_totalSegment = PathMode == PathModeEnum.ClosedLoop
    		? Waypoints.Length
    		: 2 * (Waypoints.Length - 1);
		_timeScales = new float[_totalSegment];

		_stepTime = MoveTime + WaitTime;
		_period = _stepTime * _totalSegment;
		_currentSegmentIndex = 0;

		BuildScaleTable();
		if (_isValid) GlobalPosition = Waypoints[0];
	}

	// 方法：计算 log₂(x)
	private static float Log2(float x)
	{
		// 对数换底公式
		return Mathf.Log(x) * INV_LN2;
	}

	// 方法：建立缩放系数表
/*
数组 float[] _timeScales 用于建立动画时间到指数缓动函数时间定义域的映射。
1. 指数缓动函数 f(t) = 1 - 2^(-r × t) 的自然定义域为 t ∈ [0, +∞)，
   f(0) = 0，随着 t → +∞，f(t) → 1，故函数本身作为当前径段进程的 Lerp 插值比例使用。
   而游戏中每段运动的持续时间固定为 MoveTime。因此，需要为每个路径段建立一个时间映射，
   将每条径段上的等效运动时间 timeEffective ∈ [0, MoveTime]
   线性映射到指数缓动函数的采样区间 t ∈ [0, ΔT]。
2. ΔT 为当前路径段对应的采样区间长度。
   它根据当前路径段长度 d 以及指数衰减率 DecayRate (r) 自动计算，
   使得运动结束后，终点误差不超过 min(0.5% × d, 1 px)。
   计算方式：ΔT = -(log₂(min(0.005, 1 / d))) / r。
3. 因此：初版 _timeScales[i] = MoveTime / ΔT，用于将 timeEffective 映射为
   指数缓动函数的采样时间，再计算 f(t) 作为当前路径段的 Lerp 插值比例。
4. 为消除 UpdatePosition() 中每帧一次的乘法运算，终版数组中的所有元素均进行预计算，除以 ln2。

流程示意图：
动画时间 timeEffective -> 乘以 _timeScales[i] -> 指数缓动时间 (函数自变量) -> f(t) -> Lerp 插值比例
Lerp 插值比例 -> 预运算，乘以 INV_LN2 -> 最终数组
*/
	private void BuildScaleTable()
	{
		for (int i = 0; i < Waypoints.Length - 1; i++)
		{
			float segmentLength = Waypoints[i].DistanceTo(Waypoints[i + 1]);
			if (segmentLength <= 0.01f)
			{
				GD.PushError($"错误：Waypoints[{i}] 与 Waypoints[{i + 1}] 坐标重合或距离过近。");
				_isValid = false;
				return;
			}
			// 计算 ΔT = -log₂(min(0.005, 1/d)) / r
			float domainLength = -Log2(Mathf.Min(0.005f, 1f / segmentLength)) / DecayRate;
/*
为避免极短路径或较大的 DecayRate 导致 ΔT 过小，从而使缓动几乎在动画开始瞬间完成，影响视觉观感，
最终采用：ΔT = max(理论ΔT, 1.0)，即：采样区间至少保持为 1.0。
*/
			_timeScales[i] = MoveTime / Mathf.Max(domainLength, 1.0f);
		}
		switch (PathMode)
		{
			case PathModeEnum.ClosedLoop:
				// 闭环最后一段：最后一个节点回到起点
				float segmentLength = Waypoints[Waypoints.Length - 1].DistanceTo(Waypoints[0]);
				if (segmentLength <= 0.01f)
				{
					GD.PushError("错误：Closed Loop 路径模式下，最后一个节点与起点坐标重合或距离过近。");
					_isValid = false;
					return;
				}
				float domainLength = -Log2(Mathf.Min(0.005f, 1f / segmentLength)) / DecayRate;
				_timeScales[Waypoints.Length - 1] = MoveTime / Mathf.Max(domainLength, 1.0f);
			break;

			case PathModeEnum.BackAndForth:
				// 往返：镜像映射
				for (int i = Waypoints.Length - 1; i < _timeScales.Length; i++)
				{
					_timeScales[i] = _timeScales[_timeScales.Length - 1 - i];
				}
			break;

			default:
				GD.PushError("错误：未输入有效的路径模式。");
				_isValid = false;
			break;
		}
		// 初版缩放系数表建立完成后，需要预乘 INV_LN2 以消除后续 _Process() 方法中的每帧乘法操作。
		for (int i = 0; i < _timeScales.Length; i++)
		{
    		_timeScales[i] *= INV_LN2;
		}
	}

	// 方法：更新坐标
/*
WaitTime 并不会冻结指数函数。动画在 MoveTime 后仍继续采样指数缓动函数。
由于 BuildScaleTable() 已确保 MoveTime 结束时的终点误差不超过 1 px，
因此 WaitTime 内剩余位移已经小于视觉可察觉范围。
这样既避免了函数截断造成的一阶导数突变，也使停止过程更加自然。
*/
	private void UpdatePosition()
	{
		// 始/末路径点坐标
		Vector2 startPoint = new(0f, 0f);
		Vector2 endPoint = new(0f, 0f);
		// 径段上的有效时间
		float timeEffective = (float)_time - _currentSegmentIndex * _stepTime;
		// 插值比例
		float segmentProgress = 0f;
		switch (PathMode)
		{
			case PathModeEnum.ClosedLoop:
				startPoint = Waypoints[_currentSegmentIndex];
				endPoint = Waypoints[(_currentSegmentIndex + 1) % Waypoints.Length];
			break;

			case PathModeEnum.BackAndForth:
				int index;
				if (_currentSegmentIndex < Waypoints.Length - 1)
    			{
        			// 正向
        			index = _currentSegmentIndex;
        			startPoint = Waypoints[index];
        			endPoint = Waypoints[index + 1];
    			}
    			else
    			{
        			// 返向：镜像映射
        			index = _totalSegment - 1 - _currentSegmentIndex;
        			startPoint = Waypoints[index + 1];
        			endPoint = Waypoints[index];
    			}
			break;
		}
		// 计算插值比例 r = 1 - 2 ^ (-r × (MoveTime / ΔT) × timeEffective)
		// 数学上 2 ^ (−x) 等价于 e ^ (−x × ln2)，后者在底层少一次通用幂运算。
		segmentProgress = 1f - Mathf.Exp(-DecayRate * timeEffective / _timeScales[_currentSegmentIndex]);
		GlobalPosition = startPoint.Lerp(endPoint, segmentProgress);
	}

	// 方法：更新当前径段索引
	private void UpdateSegmentIndex()
	{
		_currentSegmentIndex = (int)(_time / _stepTime);
	}

	public override void _Process(double delta)
	{
		if (!_isValid) return;

		_time += delta;
		while (_time >= _period)
		{
			_time -= _period;
		}

		UpdateSegmentIndex();
		UpdatePosition();
	}
}