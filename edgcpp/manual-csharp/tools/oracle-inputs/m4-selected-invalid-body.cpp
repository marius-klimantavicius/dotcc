// The dependent return conversion becomes invalid in the selected int instance.
void sink(int);
template<class T> T pick(T value) { return sink(value); }
int entry() { return pick(1); }
