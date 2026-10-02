//remark:C-mode null pointer constants in unevaluated contexts
//options:--c11;rp


int main() {
  return !_Generic(1 ? (void*)(2 - 2) : (int*)0, int*: 1, void*: 0);
}
