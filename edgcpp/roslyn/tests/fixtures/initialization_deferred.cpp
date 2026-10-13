struct Tmp { Tmp(); ~Tmp(); };
struct Owner { Owner(const Tmp&); ~Owner(); };
void initialization_lifetime() { Owner value((Tmp())); }

struct Item { Item(); ~Item(); };
void take(const Item&);
void master_conditional(bool condition) { take(condition ? Item() : Item()); }

consteval unsigned location_value(unsigned line = __builtin_LINE()) { return line; }
void defaulted(unsigned value = location_value());
struct Deferred { unsigned value = location_value(); };
void use_deferred() {
  defaulted();
  Deferred instance;
  (void)instance.value;
}
