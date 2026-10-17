struct ThreadValue { int value; ~ThreadValue() { value = 0; } };
thread_local ThreadValue cleanup_thread_value = { 17 };
extern "C" int thread_observe(int) { return cleanup_thread_value.value; }
