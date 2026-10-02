constexpr void empty() {}
constexpr void discard(int) {}
struct Annotated {
  int flag;
  constexpr void annotate() const {}
};
void return_empty() { return empty(); }
int exercise(int value, int* effects) {
  Annotated object{value};
  auto read = [effects](Annotated& other) {
    if (other.flag) other.annotate();
    empty();
    ++*effects;
    return other.flag;
  };
  int result = read(object);
  (value++ ? empty() : empty());
  int more = (empty(), ++*effects);
  return_empty();
  return result + value + more;
}
int main() {
  int arguments = 0;
  discard(++arguments);
  if (arguments != 1) return 3;
  int effects = 0;
  if (exercise(0, &effects) != 3 || effects != 2) return 1;
  if (exercise(7, &effects) != 19 || effects != 4) return 2;
  return 0;
}
