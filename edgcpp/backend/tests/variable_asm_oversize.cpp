extern "C" { char raw_storage[1]; }
extern long view __asm__("raw_storage");
int main() { return (int)view; }
