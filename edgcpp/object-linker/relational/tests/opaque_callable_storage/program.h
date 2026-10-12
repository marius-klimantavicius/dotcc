#pragma once
struct Result;
using Factory = Result (*)();
struct Holder { Factory callback; };
void set_callback(Holder*, Factory);
Factory get_callback(const Holder*);
void copy_holder(Holder*, const Holder*);
void clear_holder(Holder*);
bool is_empty(const Holder*);
Result make_result();
Factory bound_factory();
