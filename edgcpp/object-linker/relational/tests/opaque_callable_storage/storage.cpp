#include "program.h"
void set_callback(Holder* holder, Factory callback) { holder->callback = callback; }
Factory get_callback(const Holder* holder) { return holder->callback; }
void copy_holder(Holder* destination, const Holder* source) { *destination = *source; }
void clear_holder(Holder* holder) { holder->callback = nullptr; }
bool is_empty(const Holder* holder) { return holder->callback == nullptr; }
