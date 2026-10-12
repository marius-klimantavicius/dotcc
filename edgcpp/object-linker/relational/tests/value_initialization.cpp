class NoZero {
  int value_;
public:
  NoZero() : value_(17) {}
  int value() const { return value_; }
};

class NeedsZero {
  NoZero member_;
  int value_;
public:
  NeedsZero() = default;
  int value() const { return member_.value() + value_; }
};

struct Holder {
  NeedsZero value{};
  Holder() {}
};

int main() {
  NoZero user_provided{};
  NoZero ordinary;
  NeedsZero implicit{};
  Holder holder;
  return user_provided.value() != 17 || ordinary.value() != 17 ||
         implicit.value() != 17 || holder.value.value() != 17;
}
