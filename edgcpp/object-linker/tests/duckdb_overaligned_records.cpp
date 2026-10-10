struct alignas(64) CacheLine { long value; long next; };
struct alignas(32) HalfLine { int value; };
struct Nested { int prefix; CacheLine lines[2]; };
#pragma pack(push,1)
struct Packed { char prefix; CacheLine line; };
#pragma pack(pop)
CacheLine global_line = { 11, 17 };
thread_local CacheLine tls_line = { 19, 23 };
Nested global_nested = { 3, {{29,31},{37,41}} };
Packed packed = { 2, {43,47} };
static int aligned(const void* p, unsigned long n) { return ((unsigned long)p & (n-1)) == 0; }
static CacheLine make(long value) { CacheLine result={value,value+1}; return result; }
static long consume(CacheLine item) { return aligned(&item,64) ? item.value+item.next : -1000; }
int main() {
  CacheLine local={5,7}; HalfLine half={13}; CacheLine array[2]={{2,3},{5,7}};
  Nested nested={9,{{11,13},{17,19}}};
  if(sizeof(CacheLine)!=64 || alignof(CacheLine)!=64 || sizeof(Nested)!=192) return 1;
  if(!aligned(&global_line,64)||!aligned(&tls_line,64)||!aligned(&global_nested.lines[1],64))return 2;
  if(!aligned(&local,64)||!aligned(&half,32)||!aligned(&array[1],64)||!aligned(&nested.lines[1],64))return 3;
  if(consume(make(53))!=107||consume(local)!=12) return 4;
  if(packed.line.value!=43||packed.line.next!=47) return 5;
  packed.line.value+=2;
  if(packed.line.value!=45||global_line.next!=17||tls_line.next!=23)return 6;
  tls_line.value=59;
  return tls_line.value==59 ? 0 : 7;
}
