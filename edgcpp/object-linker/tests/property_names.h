struct Packet { int value; };
extern struct Packet packet;
extern struct Packet event;
extern int other;
struct Packet *get_packet(void);
struct Packet *get_packet__2(void);
struct Packet *get_event(void);
int get_other(void);
