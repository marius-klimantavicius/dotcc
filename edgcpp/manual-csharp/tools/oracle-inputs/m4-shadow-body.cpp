// Nested lexical scopes still cannot shadow a template parameter.
template<class T> T shadow(T value) { { int T = 0; } return value; }
