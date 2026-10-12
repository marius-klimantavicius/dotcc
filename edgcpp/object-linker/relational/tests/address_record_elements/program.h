#pragma once
enum Kind { First = 3, Second = 7 };
struct Entry { Kind kind; char text[48]; };
struct Holder { int canary; volatile int field; int values[3]; };
extern Entry entries[2];
extern Holder holder;
volatile int* field_address();
volatile int* indexed_address();
int receiver_calls();
int index_calls();
void reset_calls();
Entry* entry_address(int);
int inspect_entries();
