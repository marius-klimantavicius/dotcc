namespace AssemblyNames {
int overloaded(int value);
long overloaded(long value);
class Receiver {
    class Implementation;
    Implementation *implementation;
public:
    int read() const;
};
int read_receiver(Receiver *receiver) { return receiver->read(); }
}
extern "C" int assembly_c_bridge(int value);
static int local_helper(int value) { return value + 1; }
int assembly_cpp_entry(void)
{
    return AssemblyNames::overloaded(2) + (int)AssemblyNames::overloaded(3L)
        + assembly_c_bridge(4) + local_helper(5);
}
