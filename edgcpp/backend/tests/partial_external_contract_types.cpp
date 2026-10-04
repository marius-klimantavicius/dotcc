struct Payload { int left; unsigned right; };
enum Flavor : unsigned { ordinary = 3, large = 0xf1234567u };
extern "C" Payload host_transform(Payload, Flavor);
extern "C" int host_inspect(const Payload *);
extern "C" Flavor host_flavor(Flavor);
extern "C" Payload host_record;
extern "C" int host_call(int (*)(int), int);
static int callback_bias = 3;
static int triple(int value) { return value * 3 + ++callback_bias; }

extern "C" int check_external_contract_types() {
  Payload input = {2, 3};
  Payload (*indirect)(Payload, Flavor) = &host_transform;
  Payload first = host_transform(input, large);
  Payload second = indirect(input, ordinary);
  if (first.left != 5 || first.right != 103 || second.left != 5 || second.right != 6) return -1;
  if (host_inspect(&first) != 108 || host_flavor(ordinary) != large) return -2;
  int before = callback_bias;
  if (host_call(triple, 7) != 21 + before + 1 + 5 || callback_bias != before + 1) return -3;
  Payload *record = &host_record;
  int previous = record->left;
  record->left = previous + 1;
  record->right = 0xffffffffu;
  return previous + 20;
}
