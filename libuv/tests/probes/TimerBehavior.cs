using System;
using Managed.Libuv.Probes;
public static unsafe class TimerBehavior
{
    private static readonly int[] Trace = new int[4];
    private static int Count, Repeats;
    private static void Fired(TimerProbe.uv_timer_s* timer)
    {
        int id = (int)(nuint)timer->data;
        Trace[Count++] = id;
        if (id == 3 && ++Repeats == 2) TimerProbe.uv_timer_stop(timer);
    }
    public static void Run()
    {
        TimerProbe.uv_loop_s loop = default;
        TimerProbe.uv_timer_s a = default, b = default, r = default;
        loop.handle_queue.next = loop.handle_queue.prev = &loop.handle_queue;
        loop.time = 100;
        TimerProbe.uv_timer_init(&loop, &a); TimerProbe.uv_timer_init(&loop, &b); TimerProbe.uv_timer_init(&loop, &r);
        Console.WriteLine($"invalid={TimerProbe.uv_timer_start(&a, null, 1, 0)},{TimerProbe.uv_timer_again(&a)}");
        a.data = (void*)1; b.data = (void*)2; r.data = (void*)3;
        TimerProbe.uv_timer_start(&a, &Fired, 5, 0); TimerProbe.uv_timer_start(&b, &Fired, 5, 0); TimerProbe.uv_timer_start(&r, &Fired, 3, 3);
        Console.WriteLine($"initial={TimerProbe.uv__next_timeout(&loop)},{loop.active_handles}");
        loop.time = 103; TimerProbe.uv__run_timers(&loop);
        loop.time = 105; TimerProbe.uv__run_timers(&loop);
        loop.time = 106; TimerProbe.uv__run_timers(&loop);
        Console.WriteLine($"timers={Trace[0]},{Trace[1]},{Trace[2]},{Trace[3]} active={loop.active_handles} next={TimerProbe.uv__next_timeout(&loop)}");
    }
}
