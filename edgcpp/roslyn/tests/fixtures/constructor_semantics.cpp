// GNU array copy: expr.c56440-56534 requires class elements with nontrivial copy.
struct ConstructorCopyElement {
  int value;
  ConstructorCopyElement(int v = 0) : value(v) {}
  ConstructorCopyElement(ConstructorCopyElement const& source) : value(source.value) {}
  ~ConstructorCopyElement() {}
};
struct ConstructorArrayCopy {
  ConstructorCopyElement values[2];
  ConstructorArrayCopy(ConstructorCopyElement (&source)[2]) : values(source) {}
};
struct ConstructorDelegation {
  int value;
  ConstructorDelegation(int input) : value(input) {}
  ConstructorDelegation() : ConstructorDelegation(7) {}
};
int constructor_probe() {
  ConstructorCopyElement source[2];
  ConstructorArrayCopy copy(source);
  ConstructorDelegation delegated;
  return copy.values[0].value + delegated.value;
}
