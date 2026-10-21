// A nondependent required initializer inside an unused template needs id<int>.
template<class T> constexpr T identity(T value) { return value; }
template<class U> int outer(U value) { constexpr int result = identity(1); return result; }
