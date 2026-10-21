// The equal-match ordinary function wins; invalid concrete template body unused.
void sink(int);
int pick(int);
template<class T> T pick(T value) { return sink(value); }
int entry() { return pick(1); }
