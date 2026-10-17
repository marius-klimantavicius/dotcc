int next_value() { return 17; }
thread_local int dynamic_thread_value = next_value();
extern "C" int thread_observe(int) { return dynamic_thread_value; }
