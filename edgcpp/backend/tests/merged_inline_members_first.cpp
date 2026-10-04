// The first translation unit sees the class but never calls its inline read().
#include "merged_inline_members.h"
int first(){return sizeof(Child);}
