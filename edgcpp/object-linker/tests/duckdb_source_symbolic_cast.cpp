template<bool Value> struct Flag {};
template<class T> struct IsSigned : Flag<(T(-1) < T(0))> {};
template<class T> struct Owner { IsSigned<T> value = {}; };
int main() { return 0; }
