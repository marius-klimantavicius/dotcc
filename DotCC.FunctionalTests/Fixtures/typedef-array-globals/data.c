#include "types.h"
const FileId compound_id = {255, 128, 1};
Matrix cells = {{1, 2}, {3, 4, 5}};
int mutate(void) { cells[1][2] += 7; return cells[1][2]; }
