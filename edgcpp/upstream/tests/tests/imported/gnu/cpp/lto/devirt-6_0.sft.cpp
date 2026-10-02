//type: fp
//options: 
# 0 "./lto/devirt-6_0.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/devirt-6_0.C"


# 1 "./lto/../ipa/devirt-6.C" 1






extern "C" void abort (void);
extern "C" void *malloc(long unsigned int);

inline void* operator new(long unsigned int, void* __p) throw() { return __p;}

int x;

class A {
public:
   virtual ~A() { }
};

class B : public A {
public:
   virtual ~B() { if (x == 1) abort (); x = 1; }
};

void __attribute__((noinline,noclone)) foo (void *p)
{
 B *b = reinterpret_cast<B *>(p);
 b->~B();
 new (p) A;
}

int main()
{
 void *p = __builtin_malloc (sizeof (B));
 new (p) B;
 foo(p);
 reinterpret_cast<A *>(p)->~A();
 return 0;
}
# 4 "./lto/devirt-6_0.C" 2
