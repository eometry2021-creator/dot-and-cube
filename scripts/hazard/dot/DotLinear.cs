using Godot;
using System.Collections.Generic;
public partial class DotLinear : DotBase
{
	// ===== 导出参数 =====
	[Export] // 每秒移动速度
	public float Speed { get; set; } = 150f;
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
		new(200, 200),
		new(400, 200),
		new(400, 400),
		new(200, 400)
	};

	// ===== 内部变量 =====
	private List<float> _cumulativeTimes = new(); // 累计时间表
	//TODO：优化架构，作为数组声明而非列表
	private float _distance;                      // 累计距离
	private float _timeForward;                   // 正向时间，仅应用于 BackAndForth，等于半周期
	private float _timeEffective;                 // 有效时间，在 BackAndForth 的情况下使用三角波折叠映射进度
	private int _currentSegmentIndex;             // 当前线段指数，动点处于 Waypoints[i] 与 Waypoints[i+1] 的区间

	protected override void Initialize()
	{
		if (Waypoints.Length < 2)
		{
			GD.PrintErr("错误：列表 Waypoints 至少需要 2 个元素。");
			return;
		}
		if (Speed <= 0f)
		{
			GD.PrintErr("错误：速度必须大于 0。");
			return;
		}
		_isValid = true; // 若能运行到这里，说明节点数、速度合法，继续运行
		BuildTimetable();
		if (_isValid) GlobalPosition = Waypoints[0];
	}

	// 方法：建立时间表
	private void BuildTimetable()
	{
		_cumulativeTimes.Clear();
		_distance = 0f;
		_cumulativeTimes.Add(0f); // 补上起始时间 0
		for (int i = 0; i < Waypoints.Length - 1; i++)
		{
			float segmentLength = Waypoints[i].DistanceTo(Waypoints[i + 1]);
			if (segmentLength <= 0.01f)
			{
				GD.PrintErr($"错误：Waypoints[{i}] 与 Waypoints[{i + 1}] 坐标重合或距离过近。");
				_isValid = false;
				return;
			}
			_distance += segmentLength;
			_cumulativeTimes.Add(_distance / Speed);
		}
		switch (PathMode)
		{
			case PathModeEnum.ClosedLoop:
				// 闭环多一段：最后一个节点回到起点
				if (Waypoints[Waypoints.Length - 1].DistanceTo(Waypoints[0]) <= 0.01f)
				{
					GD.PrintErr("错误：Closed Loop 路径模式下，最后一个节点与起点坐标重合或距离过近。");
					_isValid = false;
					return;
				}
				_distance += Waypoints[Waypoints.Length - 1].DistanceTo(Waypoints[0]);
				_cumulativeTimes.Add(_distance / Speed);
				_period = _cumulativeTimes[_cumulativeTimes.Count - 1];
				break;

			case PathModeEnum.BackAndForth:
				_timeForward = _distance / Speed;
				_period = _timeForward * 2f;
				break;

			default:
				GD.PrintErr("错误：未输入有效的路径模式。");
				_isValid = false;
				break;
		}
	}

	// 方法：更新当前线段指数
	private void UpdateSegmentIndex()
	{
		while (_currentSegmentIndex < _cumulativeTimes.Count - 2 && _timeEffective > _cumulativeTimes[_currentSegmentIndex + 1])
		{
			_currentSegmentIndex += 1;
		}
		if (PathMode == PathModeEnum.BackAndForth)
		{
			while (_currentSegmentIndex > 0 && _timeEffective < _cumulativeTimes[_currentSegmentIndex])
			{
				_currentSegmentIndex -= 1;
			}
		}
	}

	protected override void UpdatePosition()
	{
		// 确定路径模式
		// TODO：优化架构，消除每帧的 switch 判断
		switch (PathMode)
		{
			case PathModeEnum.ClosedLoop:
				_timeEffective = (float)_time; // 闭环不需要折叠，直接用
				break;
			case PathModeEnum.BackAndForth:
				_timeEffective = (_time <= _timeForward) ? (float)_time : (_period - (float)_time);
				break;
		}
		// 先更新当前线段指数
		UpdateSegmentIndex();
		// 始/末路径点坐标
		Vector2 startPoint = Waypoints[_currentSegmentIndex];
		Vector2 endPoint = Waypoints[(_currentSegmentIndex + 1) % Waypoints.Length];
		// 插值比例
		float segmentProgress = (_timeEffective - _cumulativeTimes[_currentSegmentIndex]) / 
			(_cumulativeTimes[_currentSegmentIndex + 1] - _cumulativeTimes[_currentSegmentIndex]);

		GlobalPosition = startPoint.Lerp(endPoint, segmentProgress);
	}

    protected override void OnPeriodReset()
    {
        _currentSegmentIndex = 0;
    }

}