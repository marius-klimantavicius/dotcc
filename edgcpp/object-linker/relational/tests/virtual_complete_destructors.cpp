int trace;

struct Shared {
  int value;
  explicit Shared(int input) : value(input) {}
  virtual ~Shared() { trace = trace * 10 + 2; }
};

struct Leaf : virtual Shared {
  Leaf() : Shared(17) {}
  ~Leaf() { trace = trace * 10 + 1; }
};

int main() {
  {
    Leaf value;
    if (value.value != 17) return 1;
  }
  return trace != 12;
}
