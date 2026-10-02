// Runtime intrinsic adaptation mirrors c_gen_be.c::dump_intrinsic_call.
constexpr int evaluation_kind() { return __builtin_is_constant_evaluated() ? 11 : 29; }
constexpr int folded = evaluation_kind();
static_assert(folded == 11, "manifest constant evaluation remains in EDG");
int runtime_evaluation(int input) { return evaluation_kind() + input; }

void* operator new(unsigned long, void* address) noexcept { return address; }
struct Box { const int value; };
static Box box{7};
static int evaluations;
Box* box_address() { ++evaluations; return &box; }

int main() {
  if (runtime_evaluation(3) != 32) return 1;
  Box* before = &box;
  box.~Box();
  new (&box) Box{19};
  Box* after = __builtin_launder(before);
  if (after != before || after->value != 19) return 2;
  const Box* observed = __builtin_launder(box_address());
  if (observed->value != 19 || evaluations != 1) return 3;
  int value = 37;
  if (*__builtin_launder(&value) != 37) return 4;
  return 0;
}
