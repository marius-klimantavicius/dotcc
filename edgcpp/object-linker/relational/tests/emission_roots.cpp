static int used_data = 5;
static int dormant_data = 91;

static int active_helper(int value) { return value + used_data; }
static int dormant_static(int value) { return value + dormant_data; }
inline int dormant_inline(int value) { return value + 101; }

int load_dynamic() { return 7; }
int dynamic_value = load_dynamic();
int selected_entry(int value) { return active_helper(value) + dynamic_value; }

int main() { return selected_entry(3) != 15; }
