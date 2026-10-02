struct Item { int value; };
static int internal_value = 23;
static struct Item item;

struct Item* get_item(void) {
  item.value = internal_value;
  return &item;
}
int read_item(struct Item* value) { return value->value; }
