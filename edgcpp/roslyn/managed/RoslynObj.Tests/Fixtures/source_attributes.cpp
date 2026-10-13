template<int Value [[roslyn_fixture::marker(7)]] = 2> struct AttributedParameter {
  static constexpr int value = Value;
};

[[deprecated("shared declaration argument")]] int attributed_routine(int);
int attributed_routine(int input) { return input + AttributedParameter<>::value; }

alignas(int) int attributed_alignment;
void attributed_release(void *) { }
__attribute__((malloc(attributed_release, 1))) void *attributed_allocate();

int main() { return attributed_routine(3); }
