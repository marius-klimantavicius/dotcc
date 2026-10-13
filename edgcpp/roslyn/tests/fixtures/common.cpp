#define COMMON_INCREMENT(value) ((value) + 1)
namespace CommonRoot {
  int overloaded(int value) { return value + 1; }
  long overloaded(long value) { return value + 2; }
  enum class Choice { first = 3, second = 5 };
  inline namespace __attribute__((abi_tag("common"))) Version {
    int tagged(int value) { return value + 3; }
  }
}
namespace CommonAlias = CommonRoot;
namespace CommonAliasAgain = CommonAlias;
namespace CommonUse {
  using CommonRoot::overloaded;
  using enum CommonRoot::Choice;
  using CommonRoot::Choice::first;
  using namespace CommonRoot;
}
namespace CommonStrong {
  namespace debug { int value = 7; }
  using namespace debug __attribute__((__strong__));
}
namespace {
  namespace InnerAnonymous { int internal_value = 11; }
  int anonymous_value = 13;
}
int global_common = 17;
namespace CommonGlobal { using ::global_common; }
struct CommonBase {
  CommonBase(int value) : member(value) {}
protected:
  int member;
};
struct CommonDerived : CommonBase {
  using CommonBase::CommonBase;
  using CommonBase::member;
  int read() const { return member; }
};
template<class T> struct CommonTemplate {
  [[maybe_unused]] static int declared;
  T value;
};
template<class T> int CommonTemplate<T>::declared = 19;
constexpr int common_constexpr(int value) { return value + 1; }
[[deprecated("retained common flags")]] int common_deprecated(int value) {
  return value;
}
CommonDerived common_return(int value) {
  CommonDerived result(value);
  return result;
}
int common_locals(int value) {
  int first = value, second = COMMON_INCREMENT(first);
  {
    using CommonRoot::overloaded;
    extern int global_common;
    struct LocalRecord { int value; } local{second};
    second = overloaded(local.value) + global_common;
  }
  return ({ int statement_local = second; statement_local + 1; });
}
int common_labels(int value) {
  if (value) goto likely_label;
  goto unlikely_label;
[[likely]] likely_label:
  return value + 1;
[[unlikely]] unlikely_label:
  return value + 2;
}
int common(int value) {
  CommonTemplate<int> object{value};
  CommonDerived record = common_return(value);
  return common_locals(object.value) + record.read() + CommonUse::overloaded(value)
    + CommonStrong::value + CommonGlobal::global_common + anonymous_value
    + InnerAnonymous::internal_value + common_constexpr(value);
}
