// No standard library: ordered C++ initialization is a dotobj responsibility.
int startup_trace;
int startup_error;
extern int initialized_later;
extern int constant_later;

int record_startup(int digit) {
    startup_trace = startup_trace * 10 + digit;
    return digit * 11;
}

int inspect_initial_storage() {
    // This write is observable: initialized_later cannot be promoted ahead of it.
    if (initialized_later != 0) startup_error = 2;
    initialized_later = 7;
    if (constant_later != 42) startup_error = 1;
    return record_startup(1);
}

int first = inspect_initial_storage();
int initialized_later = record_startup(2);
int constant_later = 42;
int last = record_startup(3);

int main() {
    return startup_error || startup_trace != 123 || first != 11 ||
           initialized_later != 22 || last != 33 || constant_later != 42;
}
