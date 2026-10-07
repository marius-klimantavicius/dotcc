#ifndef VARIABLE_ASM_TYPE_H
#define VARIABLE_ASM_TYPE_H
extern int constructions;
extern int destructions;
namespace demo {
struct alignas(32) Object {
    int value;
    Object(int initial) : value(initial) { ++constructions; }
    ~Object() { ++destructions; }
    int read() const { return value; }
};
}
#endif
