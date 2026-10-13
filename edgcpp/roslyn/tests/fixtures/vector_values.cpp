typedef int ConstantVector __attribute__((vector_size(16)));
typedef unsigned char ConstantByteVector __attribute__((vector_size(8)));
const ConstantVector constant_vector = {1, 2, 3, 4};
const ConstantByteVector constant_byte_vector = {5, 6, 7, 8, 9, 10, 11, 12};
ConstantVector constant_vector_add(ConstantVector left, ConstantVector right) {
  return left + right;
}
