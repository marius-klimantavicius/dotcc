#include "program.h"
Watch::Watch(int *count, int input) : alive(count), value(input) { if (input < 0) throw 17; ++*alive; }
Watch::~Watch() { --*alive; }
Watch::operator bool() const { return value != 0; }
int *next_pointer(int *value, int *remaining) { if (*remaining <= 0) return nullptr; --*remaining; return value; }
void step(int *index, int *alive, int *bad) { if (*alive != 1) ++*bad; ++*index; }
int exercise() {
    int alive = 0;
    int bad = 0;
    if (int base = 2; int selected = base + 1) { if (selected != 3) return 1; }
    if (Watch selected{&alive, 0}) return 2; else if (alive != 1) return 3;
    if (alive != 0) return 4;
    if (Watch outer{&alive, 1}; int selected = alive) { if (selected != 1 || alive != 1) return 14; }
    if (alive != 0) return 15;
    int outer_loops = 0;
    for (Watch outer{&alive, 1}; outer_loops < 2; ++outer_loops) { if (alive != 1) return 16; }
    if (alive != 0 || outer_loops != 2) return 17;
    int loops = 0;
    while (Watch selected{&alive, ++loops < 3}) {
        if (alive != 1) return 5;
        if (loops == 1) continue;
    }
    if (loops != 3 || alive != 0) return 6;
    int bodies = 0;
    for (int index = 0; Watch selected{&alive, index < 3}; step(&index, &alive, &bad)) {
        ++bodies;
        int nested = 0;
        while (nested < 2) { ++nested; continue; }
        if (index == 0) continue;
        if (alive != 1) return 7;
    }
    if (bodies != 3 || alive != 0 || bad != 0) return 8;
    while (Watch selected{&alive, 1}) { break; }
    if (alive != 0) return 9;
    try { if (Watch selected{&alive, 1}) throw 6; } catch (int value) { if (value != 6 || alive != 0) return 10; }
    try { while (Watch selected{&alive, -1}) return 11; } catch (int value) { if (value != 17 || alive != 0) return 12; }
    int value = 5;
    int remaining = 2;
    int sum = 0;
    while (int *selected = next_pointer(&value, &remaining)) sum += *selected;
    if (sum != 10 || remaining != 0) return 13;
    return 0;
}
