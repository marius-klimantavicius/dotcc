// Stock lower_init emits eok_bassign for a ctor-initialized character array.
struct Text {
  unsigned prefix;
  char text[7];
  unsigned suffix;
  Text() : prefix(0x12345678), text("sample"), suffix(0x76543210) {}
};
int main() {
  Text value;
  if (value.prefix != 0x12345678 || value.suffix != 0x76543210) return 1;
  if (value.text[0] != 's' || value.text[5] != 'e' || value.text[6] != 0) return 2;
  value.text[1] = 'X';
  Text other;
  if (other.text[1] != 'a' || value.text[1] != 'X') return 3;
  return 0;
}
