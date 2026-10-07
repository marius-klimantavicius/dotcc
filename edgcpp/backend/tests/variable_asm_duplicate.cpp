int first __asm__("duplicate_storage") = 1;
int second __asm__("duplicate_storage") = 2;
extern int view __asm__("duplicate_storage");
int main() { return view; }
