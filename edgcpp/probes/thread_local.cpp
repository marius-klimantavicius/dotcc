int initial() { return 42; }
thread_local int value = initial();
int main() { return value == 42 ? 0 : 1; }
