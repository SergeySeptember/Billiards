using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Billiards.Core;

public class MatchTimer
{
    private DateTime? _startUtc;
    private DateTime? _pauseStartUtc;
    private TimeSpan _totalPaused;
    private TimeSpan _elapsedAtStop;

    public bool IsRunning { get; private set; }
    public bool IsPaused { get; private set; }

    public void Start()
    {
        _startUtc = DateTime.UtcNow;
        _pauseStartUtc = null;
        _totalPaused = TimeSpan.Zero;
        _elapsedAtStop = TimeSpan.Zero;

        IsRunning = true;
        IsPaused = false;
    }

    public void Pause()
    {
        if (!IsRunning || IsPaused || _startUtc is null)
        {
            return;
        }

        _pauseStartUtc = DateTime.UtcNow;
        IsPaused = true;
    }

    public void Resume()
    {
        if (!IsRunning || !IsPaused || _startUtc is null || _pauseStartUtc is null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        _totalPaused += now - _pauseStartUtc.Value;

        _pauseStartUtc = null;
        IsPaused = false;
    }

    public void Stop()
    {
        if (_startUtc is not null && IsRunning)
        {
            _elapsedAtStop = GetElapsedInternal(DateTime.UtcNow);
        }

        IsRunning = false;
        IsPaused = false;
        _pauseStartUtc = null;
    }

    public void Reset()
    {
        _startUtc = null;
        _pauseStartUtc = null;
        _totalPaused = TimeSpan.Zero;
        _elapsedAtStop = TimeSpan.Zero;

        IsRunning = false;
        IsPaused = false;
    }

    public TimeSpan GetElapsed(DateTime nowUtc)
    {
        if (_startUtc is null)
        {
            return _elapsedAtStop; // до старта или после сброса — 0
        }

        if (!IsRunning)
        {
            return _elapsedAtStop;
        }

        return GetElapsedInternal(nowUtc);
    }

    private TimeSpan GetElapsedInternal(DateTime nowUtc)
    {
        if (_startUtc is null)
        {
            return TimeSpan.Zero;
        }

        var effectiveNow = nowUtc;

        // если на паузе — время замирает на момент начала паузы
        if (IsPaused && _pauseStartUtc.HasValue)
        {
            effectiveNow = _pauseStartUtc.Value;
        }

        var elapsed = effectiveNow - _startUtc.Value - _totalPaused;
        if (elapsed < TimeSpan.Zero)
        {
            elapsed = TimeSpan.Zero;
        }

        return elapsed;
    }
}