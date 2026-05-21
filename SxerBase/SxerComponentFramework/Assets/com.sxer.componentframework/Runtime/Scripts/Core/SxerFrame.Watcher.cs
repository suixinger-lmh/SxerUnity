using System;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Sxer.Frame
{
    public partial class SxerFrame
    {
        #region 框架时间记录
        private DateTime _frameStartTime = DateTime.MinValue; // 框架启动时间
        private DateTime _initStartTime = DateTime.MinValue;  // 初始化开始时间
        private DateTime _initEndTime = DateTime.MinValue;    // 初始化结束时间

        // 内部钩子：在 InitProcess 协程中调用，记录时间
        internal void MarkInitStart()
        {
            _frameStartTime = DateTime.Now;
            _initStartTime = DateTime.Now;
        }

        internal void MarkInitEnd()
        {
            _initEndTime = DateTime.Now;
        }
        #endregion

        #region 非侵入式监控核心方法 (支持双轨制架构)

        /// <summary>
        /// 获取框架整体运行监控信息
        /// </summary>
        public string GetFrameWatchInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine("==================== 框架监控信息 ====================");
            sb.AppendLine($"当前框架状态: {CurrentState}");
            sb.AppendLine($"运行时长: {GetFrameRunTime()}");
            sb.AppendLine($"启动耗时: {GetFrameInitCostTime()}");
            sb.AppendLine("------------------------------------------------------");

            // 统计单例组件
            var singletons = _singletonComponents.Values;
            sb.AppendLine("【全局单例组件池】");
            sb.AppendLine($"总数: {singletons.Count} " +
                          $"| 成功: {singletons.Count(c => c.ComponentState == ComponentState.Inited && c.InitResult)} " +
                          $"| 失败: {singletons.Count(c => c.ComponentState == ComponentState.Inited && !c.InitResult)} " +
                          $"| 初始化中: {singletons.Count(c => c.ComponentState == ComponentState.InitIng)}");

            // 统计动态组件
            sb.AppendLine("【动态实例组件池】");
            sb.AppendLine($"总挂载数: {_dynamicComponents.Count} " +
                          $"| 运行中: {_dynamicComponents.Count(c => c.ComponentState == ComponentState.Inited)} " +
                          $"| 游离态: {_dynamicComponents.Count(c => c.ComponentState == ComponentState.UnInit)}");

            sb.AppendLine("======================================================");
            return sb.ToString();
        }

        /// <summary>
        /// 获取所有组件的详细监控信息（分类展示）
        /// </summary>
        public string GetAllComponentWatchInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine("==================== 详细组件监控 ====================");

            sb.AppendLine("\n>>> [全局单例组件 (Singleton)] <<<");
            var sortedSingletons = _singletonComponents.Values.OrderBy(c => c.Priority).ToList();
            foreach (var comp in sortedSingletons)
            {
                AppendComponentDetail(sb, comp);
            }

            sb.AppendLine("\n>>> [动态多实例组件 (Dynamic)] <<<");
            foreach (var comp in _dynamicComponents)
            {
                AppendComponentDetail(sb, comp);
            }

            sb.AppendLine("======================================================");
            return sb.ToString();
        }

        /// <summary>
        /// 获取指定组件类型的监控信息
        /// </summary>
        public string GetComponentWatchInfo(string componentId)
        {
            // 在单例和动态列表中查找匹配项
            var comp = _singletonComponents.Values.FirstOrDefault(c => c.ComponentID == componentId)
                       ?? _dynamicComponents.FirstOrDefault(c => c.ComponentID == componentId);

            if (comp == null)
                return $"组件[{componentId}]未找到";

            var sb = new StringBuilder();
            sb.AppendLine($"==================== 组件[{componentId}]监控信息 ====================");
            AppendComponentDetail(sb, comp);
            sb.AppendLine("====================================================================");
            return sb.ToString();
        }

        // 提取的公共拼接方法
        private void AppendComponentDetail(StringBuilder sb, SxerComponentBase comp)
        {
            sb.AppendLine($"组件ID: {comp.ComponentID}");
            sb.AppendLine($"  ├─ 类型: {comp.LifeType}");
            sb.AppendLine($"  ├─ 优先级: {comp.Priority}");
            sb.AppendLine($"  ├─ 初始化方式: {comp.InitType}");
            sb.AppendLine($"  ├─ 当前状态: {comp.ComponentState}");
            sb.AppendLine($"  ├─ 初始化结果: {comp.InitResult}");
            sb.AppendLine($"  └─ 挂载对象: {(comp.gameObject != null ? comp.gameObject.name : "Null")}");
            sb.AppendLine("------------------------------------------------------");
        }

        #endregion

        #region 时间计算辅助
        public string GetFrameRunTime()
        {
            if (CurrentState == FrameState.None) return "未启动";
            TimeSpan runTime = DateTime.Now - (_frameStartTime != DateTime.MinValue ? _frameStartTime : DateTime.Now);
            return $"{runTime.Hours:D2}:{runTime.Minutes:D2}:{runTime.Seconds:D2}.{runTime.Milliseconds:D3}";
        }

        public string GetFrameInitCostTime()
        {
            if (_initStartTime == DateTime.MinValue || _initEndTime == DateTime.MinValue)
                return "未完成或未记录";
            TimeSpan costTime = _initEndTime - _initStartTime;
            return $"{costTime.TotalMilliseconds:F0}ms (秒级: {costTime.Seconds}s)";
        }
        #endregion
    }
}