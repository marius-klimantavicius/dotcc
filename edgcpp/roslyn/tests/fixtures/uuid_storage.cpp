struct __declspec(uuid("12345678-1234-1234-1234-123456789abc")) UuidClass {
  int value;
};

enum __declspec(uuid("23456789-2345-2345-2345-23456789abcd")) UuidEnum {
  uuid_enum_value = 1
};

const auto* class_uuid_address() { return &__uuidof(UuidClass); }
const auto* enum_uuid_address() { return &__uuidof(UuidEnum); }
