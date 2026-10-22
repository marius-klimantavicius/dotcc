int shared = 10;
int counter = 30;
int *selected = &shared;
int *storage_address(void) { return &shared; }
