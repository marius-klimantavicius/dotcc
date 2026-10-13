struct ForwardRecord;
using ForwardAlias = ForwardRecord;
using ForwardPointer = ForwardRecord*;
typedef int AliasingInteger __attribute__((may_alias));
struct __attribute__((packed)) PackedRecord { char byte; int value; };
inline namespace __attribute__((abi_tag("retained"))) TaggedNamespace {
  struct TaggedRecord { int value; };
}
enum __attribute__((packed)) SmallEnum { small_first=1, small_second=2 };
enum UnsignedUnderlying { unsigned_first=1, unsigned_second=3 };
struct PolymorphicType { virtual ~PolymorphicType(); };
int type_runtime_use(PolymorphicType *value) {
  try { throw value; } catch (PolymorphicType *caught) { return caught!=0; }
}
template<class T, int N> struct TypeDependent {
  using Pointer = T*;
  using Function = T(T*);
  using Array = T[N];
  T value;
};
typedef int IntegerVector __attribute__((vector_size(16)));
IntegerVector vector_identity(IntegerVector value) { return value; }
