// Synthetic implementation with libc++'s public type_info storage convention.
// Compiler RTTI tokens and independently constructed derived objects must both
// support these ordinary inline field reads without aliasing each other's data.
namespace std {
#ifdef __EDG__
#pragma define_type_info
#endif
class type_info {
#ifdef NO_COMPILER_RTTI
  // Test the storage adapter without requesting ANY compiler RTTI/vtable.
  // EDG's explicit define_type_info pragma admits this synthetic public prefix.
  // This branch is an emitter edge-case fixture, not a standard-library ABI.
  void* prefix;
#endif
  type_info(const type_info&) = delete;
  type_info& operator=(const type_info&) = delete;
protected:
  const char* __type_name;
  explicit type_info(const char* n) : __type_name(n) {}
public:
#ifndef NO_COMPILER_RTTI
  virtual
#endif
  ~type_info() {}
  const char* name() const { return __type_name; }
  bool operator==(const type_info& other) const { return __type_name == other.__type_name; }
  bool before(const type_info& other) const { return __type_name < other.__type_name; }
};
}
void* operator new(unsigned long, void* address) noexcept { return address; }
void operator delete(void*) noexcept {} // No dynamic objects are created here.
void operator delete(void*, unsigned long) noexcept {}
struct First {};
struct Second {};
static int destroyed;
struct CustomInfo : std::type_info {
  explicit CustomInfo(const char* name) : type_info(name) {}
  ~CustomInfo()
#ifndef NO_COMPILER_RTTI
    override
#endif
    { ++destroyed; }
};
extern "C" int type_info_fields(void (*hook)()) {
#ifndef NO_COMPILER_RTTI
  const auto& first = typeid(First);
  const auto& second = typeid(Second);
  const char* name = first.name();
  const std::type_info* saved = &first;
  if (!name[0] || !second.name()[0] || first == second || !(first == typeid(First))) return 1;
  hook();
  if (first.name() != name || &typeid(First) != saved || !(first == typeid(First))) return 2;
  if (first.before(second) == second.before(first)) return 3;
#endif
  const char custom_name[] = "custom";
  alignas(16) unsigned char storage[sizeof(CustomInfo)];
  auto custom = new (storage) CustomInfo(custom_name);
  std::type_info* base = custom;
  if (base->name() != custom_name) return 4;
#ifndef NO_COMPILER_RTTI
  if (*base == first) return 4;
#endif
  hook();
  if (base->name() != custom_name) return 5;
#ifndef NO_COMPILER_RTTI
  if (first.name() != name) return 5;
#endif
  int before = destroyed;
#ifndef NO_COMPILER_RTTI
  base->~type_info();
#else
  custom->~CustomInfo();
#endif
  return destroyed == before + 1 ? 0 : 6;
}
#ifdef NATIVE_ORACLE
static void noop() {}
int main() { return type_info_fields(noop); }
#endif
