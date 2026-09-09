#include <stdio.h>

static int initializations = 0;
static int initialize(void) { initializations++; return 10; }
static int shared(int mode) {
  int result = 0;
  switch (mode) {
    case 1: {
      int value = initialize();
      goto common;
      case 2:
      value = 20;
      common:
      result = value + mode;
      break;
    }
    case 3: {
      int value = 30;
      case 4:
      value = 40;
      result = value + mode;
      break;
    }
    default: result = 99;
  }
  return result;
}

static int branches(int mode, int flag) {
  int result = 0;
  switch (mode) {
    case 0:
      if (flag) { result = 1; case 1: result += 10; }
      else { result = 2; case 2: result += 20; }
      break;
    default: result = 99;
  }
  return result;
}

static int outer_continue(int mode) {
  int result = 0;
  for (int i = 0; i < 4; ++i) {
    switch (mode) {
      case 1: {
        result++;
        case 2:
        if (i == 1) continue;
        result += i;
        break;
      }
      default: result += 10;
    }
    result += 100;
  }
  return result;
}

static int duff(int count) {
  int remaining = (count + 3) / 4;
  int result = 0;
  switch (count % 4) {
    case 0: do { result++;
    case 3:      result++;
    case 2:      result++;
    case 1:      result++;
            } while (--remaining > 0);
  }
  return result;
}

static int nested_switch(int mode) {
  int result = 0;
  switch (mode) {
    case 0: {
      case 1:
      for (int i = 0; i < 4; ++i) {
        switch (i) {
          case 1: continue;
          case 2: result += 20; break;
          default: result += i;
        }
        result += 100;
      }
      break;
    }
    default: result = -1;
  }
  return result;
}

static int array_storage(int mode) {
  int result = 0;
  switch (mode) {
    case 1: {
      int values[2] = {initialize(), 3};
      result = values[0] + values[1];
      case 2:
      result++;
      break;
    }
    default: result = 99;
  }
  return result;
}

static int while_entry(int mode) {
  int i = 0;
  int result = 0;
  switch (mode) {
    case 0: while (i < 3) {
      result++;
      case 1:
      i++;
      if (i == 2) continue;
      result += 10;
    }
    break;
    default: result = 99;
  }
  return result;
}

int main(void) {
  printf("%d\n", shared(2));
  printf("%d\n", initializations);
  int one = shared(1);
  printf("%d %d\n", one, initializations);
  printf("%d %d %d\n", shared(3), shared(4), shared(8));
  printf("%d %d %d %d\n", branches(0, 1), branches(0, 0), branches(1, 0), branches(2, 1));
  printf("%d %d\n", outer_continue(1), outer_continue(2));
  printf("%d %d %d %d\n", duff(1), duff(4), duff(7), duff(13));
  printf("%d\n", nested_switch(1));
  printf("%d\n", array_storage(2));
  printf("%d\n", array_storage(1));
  printf("%d\n", initializations);
  printf("%d %d\n", while_entry(0), while_entry(1));
  return 0;
}
