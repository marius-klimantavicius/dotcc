static unsigned int value;
int fixture(void) { return (int)__atomic_load_n(&value, __ATOMIC_ACQUIRE); }
