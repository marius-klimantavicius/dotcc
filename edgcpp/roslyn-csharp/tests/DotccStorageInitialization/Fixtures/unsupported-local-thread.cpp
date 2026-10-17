extern "C" int thread_observe(int) {
    static thread_local int local_thread_value = 17;
    return ++local_thread_value;
}
