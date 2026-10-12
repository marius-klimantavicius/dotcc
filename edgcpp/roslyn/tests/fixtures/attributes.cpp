[[deprecated("retained message")]] int old_api(int value) { return value; }
struct [[gnu::aligned(16)]] Aligned { int value; };
int __attribute__((noinline, unused)) retained_attribute(int value) { return value+1; }
int attr_use() { Aligned a{3}; return retained_attribute(a.value); }
