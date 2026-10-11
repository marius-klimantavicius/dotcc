#include "program.h"

int trace = 0;
int alive = 0;

int mark(int event) {
    trace = trace * 10 + event;
    return event;
}

Guard::Guard(int selected) : event(selected) {
    ++alive;
    mark(event);
}

Guard::~Guard() {
    mark(event + 1);
    --alive;
}
