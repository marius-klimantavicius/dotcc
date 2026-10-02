#ifndef EDG_C_TEST
#error Supply EDG_C_TEST using the driver define option.
#endif
struct Flags {unsigned a:3; signed b:5;};
static struct Flags flags={5,-3};
static volatile int counter;
int read_value(int n) {
  double value=1.5;
  switch(n) {case 3:value*=4;break;default:return 99;}
  return (int)value+flags.a+flags.b+ ++counter;
}
struct Opaque;
static struct Opaque* roundtrip(struct Opaque* p){return p;}
struct Flex {int n;int data[];};
static int flexible_tail(void){union {long align;unsigned char bytes[64];}memory={0};struct Flex* p=(struct Flex*)&memory;p->n=3;for(int i=0;i<p->n;++i)p->data[i]=i+7;return p->data[0]+p->data[1]+p->data[2]+(int)sizeof(struct Flex);}
int main(void){int n=71;if(flexible_tail()!=28)return 2;return read_value(3)!=EDG_C_TEST || (void*)roundtrip((struct Opaque*)&n)!=(void*)&n;}
