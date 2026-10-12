export template<class T> T exported_add(T left, T right) {
  return left + right;
}
export template<class T> struct ExportedBox {
  T value;
  T read() const { return value; }
};
int exported_use() {
  ExportedBox<int> value;
  value.value = 5;
  return exported_add(2, 3) + value.read();
}
