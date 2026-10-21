// The same assertion is diagnosed when its template body is instantiated.
template<class T> int assertion(T value) { static_assert(false); return 1; }
int entry() { return assertion(1); }
