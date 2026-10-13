// Source occurrences intentionally include repeated declarations and friends.
#define SOURCE_ORDER_VALUE 7
#pragma STDC FP_CONTRACT ON

namespace SourceOrder {
struct Sequence;
extern int value;
int repeated(int input);
using Integer = int;
typedef int RepeatedAlias;
typedef int RepeatedAlias;

struct Sequence {
  struct Nested;
  int first;
  static int static_value;
  friend int source_friend(Sequence &input);
  int second;
  struct Nested { int member; };
  using Alias = Integer;
  static_assert(sizeof(Integer) > 0);
  int method() const;
};

enum Choice { explicit_choice = SOURCE_ORDER_VALUE, implicit_choice };
int value = explicit_choice;
int Sequence::static_value = 2;
int repeated(int input) { return input + value; }
int Sequence::method() const { return first + second; }
int source_friend(Sequence &input) { return input.first; }

template<class T> T instantiate(T input) { return input; }
extern template int instantiate<int>(int);
template long instantiate<long>(long);

int local_entries() {
  extern int external_local(int);
  struct Local { int member; };
  Local local{3};
  return external_local(local.member);
}
}

namespace SourceOrder { int namespace_extension = 4; }
namespace SourceAlias = SourceOrder;

extern "C" {
int source_linkage(int);
int source_linkage(int input) { return input; }
}

#if defined(_MSC_VER)
#pragma comment(lib, "source-order-test-library")
#pragma conform(forScope, push, on)
#pragma conform(forScope, pop)
template<class T> struct Exists {
  __if_exists(T::member) { int retained_member; }
  __if_not_exists(T::member) { int retained_other; }
};
#endif

int main() { return SourceOrder::repeated(source_linkage(1)); }
