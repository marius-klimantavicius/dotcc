template<bool Enabled> struct Gate { int value; };
template<int N> struct Dependent : Gate<(N > 1)> { int extra; };
template<class T> struct MemberGate : Gate<T::enabled> { int extra; };
struct Settings { enum Choice { enabled = 1 }; };
template<int N, int Next = (N + 1)> int adjusted() { return Next; }
int score(Dependent<3> *value, MemberGate<Settings> *member);
template<double V, double Next = (V + 0.5)> double adjusted_float() { return Next; }
