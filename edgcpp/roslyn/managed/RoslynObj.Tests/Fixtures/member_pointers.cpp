struct PointerBase {
  int data;
  constexpr int method(int input) const { return data + input; }
};
struct PointerDerived : PointerBase { int other; };

constexpr int PointerBase::* base_data = &PointerBase::data;
constexpr int PointerDerived::* converted_data =
  static_cast<int PointerDerived::*>(base_data);
constexpr int PointerDerived::* null_data = nullptr;

using BaseMethod = int (PointerBase::*)(int) const;
using DerivedMethod = int (PointerDerived::*)(int) const;
constexpr BaseMethod base_function = &PointerBase::method;
constexpr DerivedMethod converted_function = static_cast<DerivedMethod>(base_function);
constexpr DerivedMethod null_function = nullptr;

int main() {
  PointerDerived instance{{4}, 2};
  return instance.*converted_data + (instance.*converted_function)(3)
    + (null_data == nullptr) + (null_function == nullptr);
}
