using System.Diagnostics;
using McpOsDoctor.Core.Errors;
using McpOsDoctor.Core.Interfaces;

namespace McpOsDoctor.Tools;

/// <summary>
/// Helper for building standardized <see cref="ToolResponse{TData}"/> envelopes.
/// </summary>
public static class ToolResponseBuilder
{
    /// <summary>
    /// Builds a <see cref="ToolResponse{T}"/> populated with common metadata.
    /// </summary>
    /// <typeparam name="T">Type of the data payload.</typeparam>
    /// <param name="data">The primary data payload.</param>
    /// <param name="stopwatch">Stopwatch started at the beginning of tool execution.</param>
    /// <param name="privilegeChecker">Privilege checker for elevation status.</param>
    /// <param name="warnings">Optional list of non-fatal warnings.</param>
    /// <param name="totalAvailable">Total matching items when the result was truncated.</param>
    /// <returns>A fully populated tool response envelope.</returns>
    public static ToolResponse<T> Build<T>(
        T data,
        Stopwatch stopwatch,
        IPrivilegeChecker privilegeChecker,
        List<string> warnings = null,
        int? totalAvailable = null)
    {
        stopwatch.Stop();

        return new ToolResponse<T>
        {
            Data = data,
            Warnings = warnings?.AsReadOnly() ?? (IReadOnlyList<string>)Array.Empty<string>(),
            TotalAvailable = totalAvailable,
            ElapsedMs = stopwatch.ElapsedMilliseconds,
            Platform = "windows",
            IsElevated = privilegeChecker.IsElevated
        };
    }
}