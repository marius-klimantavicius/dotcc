struct LayoutEmpty {};
struct LayoutRoot {
  int root;
  virtual LayoutRoot *copy();
  virtual ~LayoutRoot();
};
struct LayoutLeft : virtual LayoutRoot { char left; };
struct LayoutRight : virtual LayoutRoot { short right; };
struct LayoutDiamond : LayoutLeft, LayoutRight {
  int diamond;
  LayoutDiamond *copy() override;
  ~LayoutDiamond() override;
};
LayoutRoot *LayoutRoot::copy() { return this; }
LayoutRoot::~LayoutRoot() {}
LayoutDiamond *LayoutDiamond::copy() { return this; }
LayoutDiamond::~LayoutDiamond() {}
LayoutDiamond layout_diamond;

struct LayoutFields : LayoutEmpty {
  char first;
  [[no_unique_address]] LayoutEmpty empty;
  alignas(16) int aligned;
  mutable unsigned narrow : 3;
  unsigned wider_than_type : 96;
  unsigned : 0;
  unsigned share_container_first : 9;
  unsigned share_container_second : 5;
  int initialized = 7;
  union { int anonymous_integer; char anonymous_character; };
};
LayoutFields layout_fields;
constexpr unsigned long layout_fields_size = sizeof(LayoutFields);
constexpr unsigned long layout_fields_alignment = alignof(LayoutFields);
constexpr unsigned long layout_diamond_size = sizeof(LayoutDiamond);
constexpr unsigned long layout_diamond_alignment = alignof(LayoutDiamond);

template <int Width> struct LayoutDependentWidth { unsigned member : Width; };
template <class Integer, int Width>
struct LayoutDependentType { Integer member : Width; };
LayoutDependentWidth<5> layout_resolved_width;
LayoutDependentType<unsigned, 6> layout_resolved_type;

class LayoutFriend;
struct LayoutFriends {
  friend class LayoutFriend;
  friend int layout_friend_function(LayoutFriends &);
  struct Nested;
  struct Nested { int value; };
};
class LayoutFriend { int value; };
int layout_friend_function(LayoutFriends &) { return 1; }

#if !defined(_MSC_VER)
struct __attribute__((packed)) LayoutPacked {
  char first;
  int second;
  unsigned __attribute__((packed)) bits : 5;
};
LayoutPacked layout_packed;
constexpr unsigned long layout_packed_size = sizeof(LayoutPacked);
constexpr unsigned long layout_packed_alignment = alignof(LayoutPacked);
#endif

#if defined(_MSC_VER)
struct __declspec(uuid("12345678-1234-1234-1234-123456789abc"))
       __single_inheritance LayoutNativeUuid { int value; };
int LayoutNativeUuid::*layout_native_member = &LayoutNativeUuid::value;
__interface LayoutNativeEvents { void Changed(int); };
struct LayoutNativeEventSource { __event __interface LayoutNativeEvents; };
LayoutNativeUuid layout_native_uuid;
#endif

int layout_observe(LayoutDiamond &object, LayoutFields &fields) {
  return object.copy()->diamond + fields.initialized + fields.narrow;
}
