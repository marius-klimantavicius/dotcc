// No headers or runtime providers: all retained initializers are constants.
int process_value = 29;
thread_local int thread_zero;
thread_local int thread_value = 7;
thread_local int thread_array[3] = { 11, 13, 17 };
thread_local int *thread_pointer = &process_value;
alignas(64) thread_local int thread_aligned = 23;
__thread int extension_thread_value = 31;

extern "C" int thread_observe(int index) {
    if (index == 0) return thread_zero;
    if (index == 1) return thread_value;
    if (index == 2) return thread_array[0];
    if (index == 3) return thread_array[1];
    if (index == 4) return thread_array[2];
    if (index == 5) return *thread_pointer;
    if (index == 6) return thread_aligned;
    return extension_thread_value;
}
extern "C" void thread_change(int value) {
    thread_zero = value;
    thread_value += value;
    thread_array[1] += value;
    thread_aligned += value;
    extension_thread_value += value;
}
extern "C" int *thread_address(int index) {
    if (index == 0) return &thread_zero;
    if (index == 1) return &thread_value;
    if (index == 2) return &thread_array[0];
    if (index == 3) return &thread_aligned;
    if (index == 4) return &extension_thread_value;
    return thread_pointer;
}
