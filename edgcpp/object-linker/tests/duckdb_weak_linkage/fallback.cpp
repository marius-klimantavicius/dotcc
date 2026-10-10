extern "C" __attribute__((weak)) int selected() { return 11; }
extern "C" int from_fallback() { return selected(); }
