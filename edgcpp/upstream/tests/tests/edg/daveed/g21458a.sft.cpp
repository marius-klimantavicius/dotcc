//remark:vector types are trivially copyable
//options:--gnu=80000;fp

int main() {
  using vector = int __attribute__((vector_size(sizeof(int))));
  static_assert(__is_trivially_copyable(vector), "");
  return 0;
}
