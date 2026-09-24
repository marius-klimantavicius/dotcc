/* Reduced from Valkey's adlist.h, geo.h, crc16_slottable.h and dict.h. */
#include <stdio.h>
typedef struct list { int count; } list;
void clear(list *list);
void clear(list *list) { list->count = 0; }
list after_prototype_and_definition;

struct polygon { double (*points)[2]; };
extern const char table[][4];
int table_value(void) { return table[1][3]; }
unsigned long row_size(void) { return sizeof(table[0]); }
const char table[2][4] = {{1,2,3,4},{5,6,7,8}};

typedef struct dict { int count; } dict;
void run(dict *d, void(callback)(dict *)) { callback(d); }
void update(dict *d) { d->count = 42; }

int main(void) {
    list a = {3};
    clear(&a);
    after_prototype_and_definition.count = 7;
    double points[2][2] = {{1,2},{3,4}};
    struct polygon p = {points};
    dict d = {0};
    run(&d, update);
    printf("%d %d %.0f %lu %d %lu %d\n", a.count,
        after_prototype_and_definition.count, p.points[1][1],
        (unsigned long)sizeof(*p.points), table_value(), row_size(), d.count);
    return 0;
}
