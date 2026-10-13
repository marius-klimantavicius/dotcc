struct LegacyBase {
  int value;
  LegacyBase();
  virtual ~LegacyBase();
  virtual int read() const;
};
LegacyBase::LegacyBase() : value(4) {}
LegacyBase::~LegacyBase() {}
int LegacyBase::read() const { return value; }
struct LegacyDerived : virtual LegacyBase {
  int other;
  LegacyDerived();
  ~LegacyDerived();
  int read() const;
};
LegacyDerived::LegacyDerived() : LegacyBase(), other(6) {}
LegacyDerived::~LegacyDerived() {}
int LegacyDerived::read() const { return value + other; }
int legacy_virtual(LegacyBase *object) { return object->read(); }
int legacy_construct() { LegacyDerived object; return object.read(); }
int LegacyBase::*legacy_data_member = &LegacyBase::value;
int (LegacyBase::*legacy_function_member)() const = &LegacyBase::read;
struct LegacyOuter {
  struct LegacyNested { int value; };
  LegacyNested nested;
};
LegacyOuter::LegacyNested legacy_nested;
