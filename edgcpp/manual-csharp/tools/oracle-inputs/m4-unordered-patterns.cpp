// Neither transformed direct pattern is more specialized for two int arguments.
template<class T> int pattern(T, int) { return 1; }
template<class T> int pattern(int, T) { return 2; }
int entry() { return pattern(1, 1); }
