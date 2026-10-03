#include "api.h"
typedef struct Item { int value; } Item;
int sum(const Item *items, int count)
{
    int total = 0;
    for (int i = 0; i < count; ++i) {
        if (items[i].value < 0) continue;
        total += items[i].value;
    }
    return total + API_VERSION;
}
