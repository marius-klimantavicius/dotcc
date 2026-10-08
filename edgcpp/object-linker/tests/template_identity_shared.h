#ifndef TEMPLATE_IDENTITY_SHARED_H
#define TEMPLATE_IDENTITY_SHARED_H
namespace Named { template<class T, int N> struct Box { T value; }; }
namespace Other { template<class T, int N> struct Box { T value; }; }
namespace Outer { namespace Named { template<class T, int N> struct Box { T value; }; } }
// An external inline function's local type is shared across its ODR copies.
// It must not acquire a blanket TU suffix merely because it is a local class.
inline auto shared_local_value() {
  struct SharedLocal { int value; };
  SharedLocal result = { 47 };
  return result;
}
using SharedLocalValue = decltype(shared_local_value());
using SharedLocalCallback = SharedLocalValue (*)();
static auto private_local_value() {
  struct PrivateLocal { int value; };
  PrivateLocal result = { 67 };
  return result;
}
// Both TUs see exactly the same declaration path and position. Anonymous
// namespace identities still belong to each TU, even with identical layouts.
namespace {
struct LocalSame { int value; };
struct LocalDifferent {
#ifdef TEMPLATE_IDENTITY_WIDE_LOCAL
  long value;
#else
  int value;
#endif
};
}
extern Named::Box<int, 3> shared_box;
inline Named::Box<SharedLocalValue, 7> shared_local_box = { { 53 } };
inline SharedLocalValue provider_shared_local() { return shared_local_value(); }
inline SharedLocalCallback provider_local_callback() { return &shared_local_value; }
inline int read_shared_local_box(Named::Box<SharedLocalValue, 7>* p) { return p->value.value; }
void* shared_local_box_address();
int provider_read_shared_local_box();
Named::Box<int, 3>* shared_address();
int read_int(Named::Box<int, 3>*);
long read_long(Named::Box<long, 3>*);
int read_four(Named::Box<int, 4>*);
int read_other(Other::Box<int, 3>*);
int read_nested(Outer::Named::Box<int, 3>*);
void* provider_same_address();
void* provider_different_address();
void* provider_private_function_address();
int change_provider_locals();
#endif
