// Native exception storage ownership. Host tests additionally observe allocation
// balances through an instrumented generated NativeMemory boundary.
int live, destroyed, bad, alignment_bad, destructor_throws;
void (*destructor_observer)();
void (*shutdown_observer)();
struct alignas(64) Packet {
  Packet* self;
  int value;
  Packet(int n):self(this),value(n){++live;}
  Packet(const Packet& p):self(this),value(p.value){if(p.self!=&p)++bad;++live;}
  ~Packet() noexcept(false){if(self!=this)++bad;--live;++destroyed;if(destructor_observer)destructor_observer();if(destructor_throws)throw value;}
};
int read_storage_live(){return live;}
int read_storage_destroyed(){return destroyed;}
int read_storage_bad(){return bad;}
int read_storage_alignment(){return alignment_bad;}
void set_storage_destructor_throw(int n){destructor_throws=n;}
void raise_storage_packet(int n){throw Packet(n);}
void raise_storage_pointer(){throw &live;}
void raise_storage_int(){throw 17;}
int exercise_storage(int n,void (*hook)()) {
  int sum=0;
  for(int i=0;i<n;++i) {
    try {try {throw Packet(i);} catch(Packet& p) {
      if(((unsigned long long)&p & 63)!=0)++alignment_bad;
      hook();++p.value;throw;
    }} catch(Packet p) {hook();sum+=p.value;}
    try {try {throw &live;}catch(int*& p){p=0;hook();throw;}}
    catch(int* p){if(p!=&live)++bad;}
  }
  return sum;
}
int exercise_storage_callback(void (*hook)()) {
  try {Packet local(9);hook();return 0;}catch(int n){return n;}
}

struct ShutdownObserver { ~ShutdownObserver(){if(shutdown_observer)shutdown_observer();} };
void install_storage_shutdown(void (*hook)()){static ShutdownObserver value;shutdown_observer=hook;}
void install_storage_destructor(void (*hook)()){destructor_observer=hook;}
