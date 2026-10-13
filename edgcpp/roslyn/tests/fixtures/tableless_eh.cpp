int tableless_effects;

struct TablelessTemporary {
  int value;
  explicit TablelessTemporary(int input) : value(input) { tableless_effects += input; }
  ~TablelessTemporary() { tableless_effects -= value; }
};

struct TablelessObject {
  int value;
  TablelessObject(TablelessTemporary const &first, TablelessTemporary const &second)
    : value(first.value + second.value) { tableless_effects += value; }
  ~TablelessObject() { tableless_effects -= value; }
};

int tableless_overlapping_initialization(int input) {
  TablelessObject first{TablelessTemporary(input), TablelessTemporary(input + 1)};
  TablelessObject second{TablelessTemporary(input + 2), TablelessTemporary(input + 3)};
  return first.value + second.value;
}
