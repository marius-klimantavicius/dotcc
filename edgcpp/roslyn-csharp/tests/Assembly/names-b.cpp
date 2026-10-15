namespace AssemblyNames {
int overloaded(const int value) { return value + 10; }
long overloaded(long value) { return value + 20; }
class Receiver {
    class Implementation;
    Implementation *implementation;
public:
    int read() const;
};
int Receiver::read() const { return 17; }
}
extern "C" int assembly_c_bridge(int value) { return value + 30; }
static int local_helper(int value) { return value + 2; }
int assembly_cpp_second(void) { return local_helper(6); }
