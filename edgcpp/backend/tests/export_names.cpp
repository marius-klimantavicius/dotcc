struct NamedMember {
  int value;
  NamedMember() : value(17) {}
  ~NamedMember() { value = 0; }
};
/* EDG synthesizes the owner constructor/destructor with meaningful source names. */
struct NamedOwner { NamedMember member; };
namespace Library {
  int add(int value) { return value + 3; }
  int add(int left, int right) { return left + right; }
}
extern "C" int cpp_public_api(int value) {
  NamedOwner owner;
  return owner.member.value + Library::add(value) + Library::add(value, 2);
}
