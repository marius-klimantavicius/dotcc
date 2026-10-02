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
int dynamic_constant_query(int value) { return __builtin_constant_p(value); }
int never_evaluated(int value) { ++evaluations; return value; }
static_assert(__builtin_constant_p(7 * 9), "EDG retains proven constants");

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
  // GNU leaves nonconstant runtime queries to the backend. The operand is
  // never evaluated, even if it contains calls, increments or volatile reads.
  if (__builtin_constant_p(never_evaluated(++evaluations))) return 5;
  if (__builtin_constant_p(*(volatile int*)0)) return 6;
  if (evaluations != 1) return 7;
  int dynamic = dynamic_constant_query(value);
  if (dynamic != 0 && dynamic != 1) return 8; // optimization-dependent GNU result
  return 0;
}
