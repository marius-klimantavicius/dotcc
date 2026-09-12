/* Reduced from QuicTraceEvent -> QuicTrace -> clog. */
void sink(const char *format, ...);
#define LOG(fmt, ...) sink((fmt), ##__VA_ARGS__)
#define TRACE(name, fmt, ...) LOG((fmt " [" #name "]"), ##__VA_ARGS__, __FILE__, __LINE__)
#define EVENT(name, fmt, ...) TRACE(name, fmt, ##__VA_ARGS__)
void test(void) { EVENT(AllocFailure, "%s %d", "key", 42); }
