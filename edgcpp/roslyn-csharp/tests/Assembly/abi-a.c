extern int abi_shared(int value);
int abi_caller(void) { return abi_shared(3); }
