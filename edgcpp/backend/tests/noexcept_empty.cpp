int destroyed;
void empty_noexcept() noexcept {}
void return_noexcept() noexcept { return; }
void empty_blocks_noexcept() noexcept { {}; { return; } }
int may_throw(int enabled) { if (enabled) throw 17; return 9; }
int expression_return_noexcept() noexcept { return may_throw(0); }
void call_noexcept() noexcept { may_throw(0); }
struct Empty {
  Empty() noexcept {}
  ~Empty() noexcept {}
};
struct Member {
  ~Member() noexcept { ++destroyed; }
};
struct Owner {
  Member member;
  ~Owner() noexcept {} // Source-empty body still destroys member.
};
void cleanup_noexcept() noexcept { Member member; }
int main() {
  empty_noexcept();
  return_noexcept();
  empty_blocks_noexcept();
  if (expression_return_noexcept() != 9) return 1;
  call_noexcept();
  { Empty empty; Owner owner; }
  cleanup_noexcept();
  return destroyed != 2;
}
