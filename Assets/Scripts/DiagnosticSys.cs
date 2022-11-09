using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Diagnostics;
using UnityEngine;

public class DiagnosticSys : MonoBehaviour
{
    private PerformanceStatsSubsystem performanceStatsHelpers;

    private void Start()
    {
        performanceStatsHelpers = new PerformanceStatsSubsystem();

        Debug.Log($"Subsystems: {XRSubsystemHelpers.GetAllSubsystems<PerformanceStatsSubsystem>().Count}");
    }

    private void Update()
    {
        //PrintVar.print(1, $"Performances: {performanceStatsHelpers.FrameRate}");
    }
}
