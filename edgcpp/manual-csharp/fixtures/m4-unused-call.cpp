// Nondependent overload failure is diagnosed during unused template definition.
int need(int);
template<class T> T unused(T value) { need(); return value; }
