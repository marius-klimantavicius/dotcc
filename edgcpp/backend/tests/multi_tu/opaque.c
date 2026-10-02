struct Item;
extern struct Item* get_item(void);
extern int read_item(struct Item*);
static int internal_value = 11;

int main(void) {
  return read_item(get_item()) == 23 && internal_value == 11 ? 0 : 1;
}
