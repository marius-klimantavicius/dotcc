struct BaseBytes { unsigned long long base; };
struct Layout : virtual BaseBytes {
    unsigned long long __record_bytes;
    unsigned long long __record_alignment;
};
struct Fieldless : virtual BaseBytes {};
#pragma pack(push, 1)
struct Packed : virtual BaseBytes { unsigned char marker; };
#pragma pack(pop)
struct LayoutEmbedded { unsigned char before; Layout value; unsigned char after; };
struct FieldlessEmbedded { unsigned char before; Fieldless value; unsigned char after; };
struct PackedEmbedded { unsigned char before; Packed value; unsigned char after; };

unsigned long layout_size();
unsigned long layout_alignment();
unsigned long layout_embedded_size();
unsigned long fieldless_size();
unsigned long fieldless_alignment();
unsigned long fieldless_embedded_size();
unsigned long packed_size();
unsigned long packed_alignment();
unsigned long packed_embedded_size();
void layout_touch(Layout *value);
void fieldless_keep(Fieldless *value);
void packed_touch(Packed *value);
void layout_embedded_keep(LayoutEmbedded *value);
void fieldless_embedded_keep(FieldlessEmbedded *value);
void packed_embedded_keep(PackedEmbedded *value);
