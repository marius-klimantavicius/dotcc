template<class T, int N> int retained_original(T) { return N; }
int prime_original(int value) { return retained_original<int, 7>(value); }
int alias_original(int value) {
  using LocalInt = int;
  return retained_original<LocalInt, 7>(value);
}

template<class T> struct EnumHost {
  enum class State : unsigned;
};
template<class T>
enum class EnumHost<T>::State : unsigned { First = 1, Sized = sizeof(T) };
EnumHost<int>::State enum_state = EnumHost<int>::State::Sized;
