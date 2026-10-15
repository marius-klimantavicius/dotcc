static volatile unsigned long long value;
int fixture(void) { value = 17; return (int)value; }
