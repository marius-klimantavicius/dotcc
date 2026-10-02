#include "lifetimes.cpp"
#include <cstdio>
int fail_at;
void hook(int n) {if(n==fail_at)throw n;}
void show(const char* label,void(*f)(Hook),int fail) {
    fail_at=fail;reset();int caught=0;
    try {f(hook);}catch(int n){caught=n;}
    std::printf("%s:%d:%llu\n",label,caught,read_trace());
}
void nested_normal(Hook h){nested(h,0);}
void nested_early(Hook h){nested(h,1);}
int main() {
    show("nested",nested_normal,0);show("early",nested_early,0);
    show("first-ctor",nested_normal,1);show("second-ctor",nested_normal,2);
    show("body",nested_normal,9);show("members",members,0);
    show("member-ctor",members,2);show("outer-ctor",members,3);
    show("member-body",members,9);show("loops",loops,0);
    show("jump",jump,0);show("jump-throw",jump,9);
    show("backward",reverse_jump,0);show("throwing-dtor",throwing_dtor,8);
}
