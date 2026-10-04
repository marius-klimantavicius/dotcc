// The definition selected from the first TU must be retained for this call.
#include "merged_inline_members.h"
int main(){int x=17;Child a(&x);return a.read()==17 && a.route()==&x?0:1;}
