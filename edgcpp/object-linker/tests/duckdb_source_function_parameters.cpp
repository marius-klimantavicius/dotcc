// Original named parameter declarations used in body-local source type recipes.
void marker() {}
int convert(int value) { return value; }
long convert(long value) { return value; }
template<class T> T copy_value(T input) {
  using Result = decltype(convert(input));
  Result result = input;
  return result;
}
template<class T> T copy_other(T input) {
  using Result = decltype(convert(input));
  Result result = input;
  return result;
}
template<class T> T choose(T left, T right) {
  using Left = decltype(left);
  using Right = decltype(right);
  Left first = left;
  Right second = right;
  return first + second;
}
template<class T> struct Holder {
  T copy(T input) {
    using Result = decltype(convert(input));
    Result result = input;
    return result;
  }
};
int main() {
  marker();
  Holder<int> holder;
  return copy_value(5) + copy_other(7) + choose(2, 3) + holder.copy(4) + int(copy_value(9L)) == 30 ? 0 : 1;
}
