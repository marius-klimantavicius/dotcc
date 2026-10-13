extern "C" typedef int RoutineCFunction(int);
RoutineCFunction routine_typed_declaration;
int routine_typed_declaration(int value) { return value; }

struct RoutineValue {
  int value;
  explicit RoutineValue(int input) : value(input) { }
  RoutineValue(RoutineValue const &other) : value(other.value) { }
  ~RoutineValue() { }
};
RoutineValue routine_class_result(int value) { return RoutineValue(value); }

struct RoutineBase {
  virtual RoutineBase *clone() { return this; }
  virtual int pure(int) = 0;
  virtual ~RoutineBase() { }
};
struct RoutinePrimary { int prefix; virtual ~RoutinePrimary() { } };
struct RoutineDerived : RoutinePrimary, RoutineBase {
  RoutineDerived *clone() override { return this; }
  int pure(int value) override final { return value; }
  int qualified(int value) const __restrict & { return value; }
};

struct RoutineTrivial { RoutineTrivial() = default; };
RoutineTrivial routine_trivial_object;
struct RoutineInherited : RoutineValue { using RoutineValue::RoutineValue; };
struct RoutineFriend {
  friend int routine_friend(RoutineFriend) { return 3; }
};

struct RoutineOperators {
  explicit operator int() const { return 4; }
  int operator+(int value) const { return value + 4; }
};
unsigned long long operator""_routine_raw(char const *) { return 5; }

template<class T> struct RoutineGuide { T value; RoutineGuide(T input) : value(input) { } };
RoutineGuide(int) -> RoutineGuide<long>;
RoutineGuide routine_guided(6);

template<class T> struct RoutineConstrained {
  int selected() requires (sizeof(T) > 1) { return 7; }
  int selected() requires (sizeof(T) == 1) { return 8; }
};
int routine_constraints() { return RoutineConstrained<int>{}.selected(); }

auto routine_trailing(int value) -> int { return value; }
consteval int routine_immediate(int value) { return value + 1; }
constexpr int routine_constexpr(int value) { return value + 2; }
static_assert(routine_immediate(1) == 2);
int (*routine_lambda_entry)(int) = [](int value) { return value + 3; };

#if defined(__GNUC__)
extern "C" int routine_alias_target(int value) { return value; }
extern "C" int routine_alias(int) __attribute__((alias("routine_alias_target")));
extern "C" int (*routine_resolver())(int) { return routine_alias_target; }
extern "C" int routine_ifunc(int) __attribute__((ifunc("routine_resolver")));

int routine_pure(int value) __attribute__((pure));
int routine_pure(int value) { return value; }
__attribute__((constructor(201))) void routine_constructor_priority() { }
__attribute__((destructor(202))) void routine_destructor_priority() { }
__attribute__((used, noinline, section(".routine_test"))) int routine_section(int value) { return value; }

extern inline __attribute__((gnu_inline)) int routine_inline_partner(int value) { return value; }
int routine_inline_partner(int value) { return value + 1; }

__attribute__((target("default"))) int routine_multiversion(int value) { return value; }
__attribute__((target("sse2"))) int routine_multiversion(int value) { return value + 1; }
#endif

int main() {
  RoutineDerived object;
  RoutineInherited inherited(2);
  return routine_typed_declaration(1) + object.pure(2) + object.qualified(3) +
         routine_class_result(4).value + inherited.value + routine_constraints() +
         routine_lambda_entry(5) + routine_friend(RoutineFriend{});
}
