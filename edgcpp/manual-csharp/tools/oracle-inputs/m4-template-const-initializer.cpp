// A const integral initializer can be usable by a required assertion.
template<class T> constexpr T identity(T value) { return value; }
template<class U> int outer(U value) { const int result = identity(1); static_assert(result == 1); return result; }
