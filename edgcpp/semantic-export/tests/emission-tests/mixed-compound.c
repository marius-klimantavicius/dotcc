/* C promotes x to unsigned int before division; direct C# x /= y differs. */
int mixed(short x, unsigned int y) { x /= y; return x; }
