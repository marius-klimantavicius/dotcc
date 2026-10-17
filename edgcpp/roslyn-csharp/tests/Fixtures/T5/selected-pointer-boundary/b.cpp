#include "api.hpp"
const BoundaryCell *boundary_echo(const BoundaryCell *cell) { return cell; }
unsigned boundary_read(const BoundaryCell *cell) { return cell->value; }
