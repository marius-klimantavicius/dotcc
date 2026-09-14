#include <stdio.h>

struct Entry { int value; struct Peer *peer; };
struct Peer { struct Entry entry; };
union Number { int value; unsigned bits; };
enum Mode { ModeOn = 1 };
enum event { EventReady = 2 };
struct __DotCcTags { int marker; };

int Entry(struct Entry *item) { return item->value + item->peer->entry.value; }
int Number(union Number *item) { return item->value; }
int Mode(enum Mode item) { return item == ModeOn; }
int event(enum event item) { return item; }

int check(void) {
  struct Peer peer = {{20, 0}};
  struct Entry item = {21, &peer};
  union Number number = {.value = 1};
  struct __DotCcTags marker = {2};
  int (*read)(struct Entry *) = Entry;
  return read(&item) + Number(&number) + Mode(ModeOn) - marker.marker + event(EventReady) - 1;
}

int main(void) { printf("%d\n", check()); return 0; }
