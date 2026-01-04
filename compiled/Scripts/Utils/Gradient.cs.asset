using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Interpolation mode for gradient sampling.
/// </summary>
public enum GradientInterpolationMode
{
    Constant,  // No interpolation, use the value of the previous point
    Linear     // Linear interpolation between points
}

/// <summary>
/// A point in a gradient with a progress value (0-1) and an element value.
/// </summary>
[Serializable]
public class GradientPoint<T> : IComparable<GradientPoint<T>> where T : struct
{
    public float progress;  // Time/progress value (0-1)
    public T element;       // Value at this point

    public GradientPoint(T element, float progress)
    {
        this.element = element;
        this.progress = progress;
    }

    public int CompareTo(GradientPoint<T> other)
    {
        if (other == null) return 1;
        return progress.CompareTo(other.progress);
    }

    public override bool Equals(object obj)
    {
        if (obj is GradientPoint<T> other)
        {
            return Math.Abs(progress - other.progress) < 0.0001f && element.Equals(other.element);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return progress.GetHashCode() ^ element.GetHashCode();
    }
}

/// <summary>
/// A gradient that can sample interpolated values over a progress range (0-1).
/// Useful for controlling values over time, such as enemy spawn rates.
/// </summary>
[Serializable]
public class Gradient<T> where T : struct
{
    private List<GradientPoint<T>> points = new List<GradientPoint<T>>();
    private GradientInterpolationMode interpolationMode = GradientInterpolationMode.Linear;

    /// <summary>
    /// Add a point to the gradient.
    /// </summary>
    public int AddPoint(T element, float progress)
    {
        var point = new GradientPoint<T>(element, progress);
        points.Add(point);
        points.Sort();
        return points.IndexOf(point);
    }

    /// <summary>
    /// Remove a point at the specified index.
    /// </summary>
    public void RemovePoint(int index)
    {
        if (index >= 0 && index < points.Count)
        {
            points.RemoveAt(index);
            points.Sort();
        }
    }

    /// <summary>
    /// Set all points at once.
    /// </summary>
    public void SetPoints(List<GradientPoint<T>> newPoints)
    {
        points = new List<GradientPoint<T>>(newPoints);
        points.Sort();
    }

    /// <summary>
    /// Get all points.
    /// </summary>
    public List<GradientPoint<T>> GetPoints()
    {
        return new List<GradientPoint<T>>(points);
    }

    /// <summary>
    /// Set the progress value for a point at the specified index.
    /// </summary>
    public void SetProgress(int index, float progress)
    {
        if (index >= 0 && index < points.Count)
        {
            points[index].progress = progress;
            points.Sort();
        }
    }

    /// <summary>
    /// Get the progress value for a point at the specified index.
    /// </summary>
    public float GetProgress(int index)
    {
        if (index >= 0 && index < points.Count)
        {
            return points[index].progress;
        }
        return 0.0f;
    }

    /// <summary>
    /// Set the element value for a point at the specified index.
    /// </summary>
    public void SetElement(int index, T element)
    {
        if (index >= 0 && index < points.Count)
        {
            points[index].element = element;
        }
    }

    /// <summary>
    /// Get the element value for a point at the specified index.
    /// </summary>
    public T GetElement(int index)
    {
        if (index >= 0 && index < points.Count)
        {
            return points[index].element;
        }
        return default(T);
    }

    /// <summary>
    /// Reverse the gradient (mirror it).
    /// </summary>
    public void Reverse()
    {
        foreach (var point in points)
        {
            point.progress = 1.0f - point.progress;
        }
        points.Sort();
    }

    /// <summary>
    /// Check if the gradient is valid (has at least one point).
    /// </summary>
    public bool IsValid()
    {
        return points.Count > 0;
    }

    /// <summary>
    /// Sample the gradient at the specified progress (0-1).
    /// </summary>
    public T Sample(float progress)
    {
        if (!IsValid())
        {
            return default(T);
        }

        // Clamp progress to [0, 1]
        progress = Math.Max(0.0f, Math.Min(1.0f, progress));

        // Binary search for the appropriate point
        int low = 0;
        int high = points.Count - 1;
        int middle = 0;

        while (low <= high)
        {
            middle = (low + high) / 2;
            var point = points[middle];
            
            if (point.progress > progress)
            {
                high = middle - 1;
            }
            else if (point.progress < progress)
            {
                low = middle + 1;
            }
            else
            {
                return point.element;
            }
        }

        if (points[middle].progress > progress)
        {
            middle--;
        }

        int first = middle;
        int second = middle + 1;

        // Handle edge cases
        if (second >= points.Count)
        {
            return points[points.Count - 1].element;
        }

        if (first < 0)
        {
            return points[0].element;
        }

        var pointFirst = points[first];
        var pointSecond = points[second];

        switch (interpolationMode)
        {
            case GradientInterpolationMode.Constant:
                return pointFirst.element;
                
            case GradientInterpolationMode.Linear:
                float absProgress = (progress - pointFirst.progress) / (pointSecond.progress - pointFirst.progress);
                return Lerp(pointFirst.element, pointSecond.element, absProgress);
        }

        return pointFirst.element;
    }

    /// <summary>
    /// Set the interpolation mode.
    /// </summary>
    public void SetInterpolationMode(GradientInterpolationMode mode)
    {
        interpolationMode = mode;
    }

    /// <summary>
    /// Get the interpolation mode.
    /// </summary>
    public GradientInterpolationMode GetInterpolationMode()
    {
        return interpolationMode;
    }

    /// <summary>
    /// Linear interpolation between two values.
    /// Override this for custom types if needed.
    /// </summary>
    private T Lerp(T a, T b, float t)
    {
        // Handle common types
        if (typeof(T) == typeof(float))
        {
            float aFloat = (float)(object)a;
            float bFloat = (float)(object)b;
            float result = aFloat + (bFloat - aFloat) * t;
            return (T)(object)result;
        }
        else if (typeof(T) == typeof(int))
        {
            int aInt = (int)(object)a;
            int bInt = (int)(object)b;
            int result = (int)(aInt + (bInt - aInt) * t);
            return (T)(object)result;
        }
        else if (typeof(T) == typeof(double))
        {
            double aDouble = (double)(object)a;
            double bDouble = (double)(object)b;
            double result = aDouble + (bDouble - aDouble) * t;
            return (T)(object)result;
        }

        // Fallback: return first value for unsupported types
        return a;
    }

    /// <summary>
    /// Get the number of points in the gradient.
    /// </summary>
    public int GetPointCount()
    {
        return points.Count;
    }

    /// <summary>
    /// Clear all points.
    /// </summary>
    public void Clear()
    {
        points.Clear();
    }
}
