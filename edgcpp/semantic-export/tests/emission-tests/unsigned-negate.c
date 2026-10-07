/* C permits unsigned negation; C# has no unary minus operator on ulong. */
unsigned long long negate_wide(unsigned long long value) { return -value; }
