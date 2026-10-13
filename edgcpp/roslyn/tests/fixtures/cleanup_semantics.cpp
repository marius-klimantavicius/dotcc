int cleanup_effects;
int cleanup_effect(int value) { cleanup_effects += value; return value; }

struct CleanupBase {
  int value;
  CleanupBase(int input = 0) : value(cleanup_effect(input)) { }
  CleanupBase(CleanupBase const &other) : value(cleanup_effect(other.value)) { }
  virtual ~CleanupBase() { cleanup_effect(value); }
};

struct CleanupVirtual : virtual CleanupBase {
  CleanupBase members[2];
  CleanupVirtual(int input)
    : members{CleanupBase(input + 1), CleanupBase(input + 2)},
      CleanupBase(input) { }
  ~CleanupVirtual() { cleanup_effect(1); }
};

struct CleanupFurther : CleanupVirtual {
  CleanupFurther(int input) : CleanupVirtual(input) { }
  ~CleanupFurther() { cleanup_effect(2); }
};

int cleanup_subobject_vtt(int input) {
  CleanupFurther value(input);
  return value.value;
}

CleanupBase &cleanup_local_static(int input) {
  static CleanupBase persistent(input);
  return persistent;
}

int cleanup_paths(int input) {
  CleanupBase outer(input);
  if (input) {
    CleanupBase inner(input + 1);
    goto completed;
  }
  {
    CleanupBase separate(input + 2);
    cleanup_effect(separate.value);
  }
completed:
  cleanup_effect(outer.value);
  try {
    CleanupBase thrown(input + 3);
    throw thrown;
  } catch (CleanupBase caught) {
    cleanup_effect(caught.value);
  }
  return cleanup_local_static(input).value;
}

int cleanup_array_parameter(CleanupVirtual input) {
  CleanupBase locals[2]{CleanupBase(input.value), CleanupBase(input.value + 1)};
  return locals[0].value + locals[1].value;
}

struct CleanupThrownAggregate {
  int prefix;
  CleanupBase first;
  CleanupBase second;
};

void cleanup_throw_aggregate(int input) {
  throw CleanupThrownAggregate{input, CleanupBase(input), CleanupBase(input + 1)};
}

struct CleanupThrownZeroAggregate {
  CleanupBase first;
  CleanupBase second;
};

void cleanup_throw_zero_aggregate(int input) {
  throw CleanupThrownZeroAggregate{CleanupBase(input), CleanupBase(input + 1)};
}

#if defined(__GNUC__)
int cleanup_vla(int count) {
  CleanupBase values[count];
  return sizeof(values);
}
#endif
