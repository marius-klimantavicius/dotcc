int source_vla(int count, int columns) {
  typedef int Row[columns];
  Row rows[count];
  rows[0][0] = count;
  return (int)sizeof(rows) + rows[0][0];
}
