#include "program.h"

unsigned long layout_size() { return sizeof(Layout); }
unsigned long layout_alignment() { return alignof(Layout); }
unsigned long layout_embedded_size() { return sizeof(LayoutEmbedded); }
unsigned long fieldless_size() { return sizeof(Fieldless); }
unsigned long fieldless_alignment() { return alignof(Fieldless); }
unsigned long fieldless_embedded_size() { return sizeof(FieldlessEmbedded); }
unsigned long packed_size() { return sizeof(Packed); }
unsigned long packed_alignment() { return alignof(Packed); }
unsigned long packed_embedded_size() { return sizeof(PackedEmbedded); }
void layout_touch(Layout *value) { value->__record_bytes = 43; value->__record_alignment = 47; }
void fieldless_keep(Fieldless *) {}
void packed_touch(Packed *value) { value->marker = 11; }
void layout_embedded_keep(LayoutEmbedded *) {}
void fieldless_embedded_keep(FieldlessEmbedded *) {}
void packed_embedded_keep(PackedEmbedded *) {}
