/* Native Microsoft IDL attributes, with all six retained argument kinds. */
[module(type=dll, name="NativeModule",
        uuid="12345678-1234-1234-1234-123456789abc", version=1.2,
        lcid=1033, control=true)];

[uuid("23456789-1234-1234-1234-123456789abc")]
struct NativeUuid { int value; };

int native_attribute_value() { return sizeof(NativeUuid); }
