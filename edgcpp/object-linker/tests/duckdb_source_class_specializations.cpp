namespace original_specializations {
template<class T, bool Enabled> struct Choice { static T select(T v) { return v; } };
template<class T> struct Choice<T, true> { static T select(T v) { return v; } };
template<class T, bool Flag> struct Choice<T*, Flag> { static T* select(T* v) { return v; } };
template<class T> concept Selected = requires(T value, long* pointer) { Choice<int, true>::select(value); Choice<long*, false>::select(pointer); };
template<Selected T> struct Use { T field = {}; };
static_assert(Selected<int>);
}
int main() { return original_specializations::Choice<int,true>::select(0); }
