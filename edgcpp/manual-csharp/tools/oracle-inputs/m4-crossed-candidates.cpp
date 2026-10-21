// Ordinary preference must not override crossed argument conversion quality.
int cross(int, long);
template<class T> int cross(long, T value) { return value; }
int entry() { return cross(1, 1); }
