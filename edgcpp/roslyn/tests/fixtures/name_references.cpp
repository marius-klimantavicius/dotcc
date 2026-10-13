int name_reference_global;
namespace NameReferenceScope {
namespace Nested {
template<class T> T name_target(T value) { return value; }
}
}
int name_reference_use() {
  return NameReferenceScope::Nested::name_target<int>(::name_reference_global);
}

extern int name_external(int first, ...);
int name_external_use() { return name_external(1, 2); }

struct NameRecord {
  int value;
  ~NameRecord() { }
};
int name_reference_field(NameRecord &record) { return record.NameRecord::value; }
void name_reference_destroy(NameRecord *record) { record->NameRecord::~NameRecord(); }
