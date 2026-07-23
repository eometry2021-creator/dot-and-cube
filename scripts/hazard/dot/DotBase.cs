using Godot;

public partial class DotBase : Node2D
{
	protected const float INV_LN2 = 1.442695f; // 1 / ln(2)
	protected double _time = 0.0;              // 内部时间
    protected bool _isValid = false;           // 合法性检查
    protected float _period;                   // 周期
	public override void _Ready()
	{
		Initialize();
	}

	// 方法：初始化（由 _Ready() 调用一次）
	protected virtual void Initialize()
	{
		/*
		===== 子类重写此虚方法，实现初始化逻辑 =====
		具体包括：
		1. 导出变量的合法性检查
		2. 内部变量的预计算
		3. 移动到初始世界坐标
		*/
	}

	// 方法：更新坐标（每帧执行）
	protected virtual void UpdatePosition()
	{
		// =====子类重写此虚方法，实现运动逻辑=====
	}
	
	// 方法：特殊的周期重置（周期重置时执行）
	protected virtual void OnPeriodReset()
	{
		// （可选）=====子类重写此虚方法，以处理周期循环的特殊逻辑=====
	}

	// 方法：通用物理过程
	public override void _PhysicsProcess(double delta)
	{
		if (!_isValid) return;

    	_time += delta;
		// 让时间永远锁在 [0, _period) 的区间内，以防 _time 无限增长导致浮点精度下降
    	if (_time > _period)
    	{
    	    _time %= _period;
			OnPeriodReset();
		}

		UpdatePosition();
	}
}