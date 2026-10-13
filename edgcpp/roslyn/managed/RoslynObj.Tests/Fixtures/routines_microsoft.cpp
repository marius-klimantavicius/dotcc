struct RoutineSelectiveBase {
  virtual int call(int value) = 0;
  virtual ~RoutineSelectiveBase() { }
};
struct RoutineSelectiveDerived : RoutineSelectiveBase {
  int RoutineSelectiveBase::call(int value) { return value + 1; }
};
__interface RoutineNativeInterface { int call(int); };
struct RoutineNativeImplementation : RoutineNativeInterface {
  int RoutineNativeInterface::call(int value) { return value + 2; }
};
int __stdcall routine_native_convention(int value) { return value; }
int main() {
  RoutineSelectiveDerived derived;
  RoutineNativeImplementation implementation;
  return derived.call(1) + implementation.call(2) + routine_native_convention(3);
}
