struct BoundaryCell { unsigned value; unsigned canary; };
const BoundaryCell *boundary_echo(const BoundaryCell *cell);
unsigned boundary_read(const BoundaryCell *cell);
