struct LayoutCEmpty {};
struct LayoutCFields {
  char first;
  int value;
  unsigned narrow : 3;
  unsigned next : 7;
  unsigned : 0;
  int flexible[];
};
union LayoutCUnion { unsigned value; unsigned char bytes[4]; };
struct LayoutCFields layout_c_fields;
union LayoutCUnion layout_c_union;
const unsigned long layout_c_size = sizeof(struct LayoutCFields);
const unsigned long layout_c_alignment = __alignof__(struct LayoutCFields);
const unsigned long layout_c_value_offset = __builtin_offsetof(struct LayoutCFields, value);
struct __attribute__((packed)) LayoutCPacked { char first; int second; };
struct LayoutCPacked layout_c_packed;
const unsigned long layout_c_packed_size = sizeof(struct LayoutCPacked);
int layout_c_observe(void) { return layout_c_fields.value + layout_c_fields.narrow; }
