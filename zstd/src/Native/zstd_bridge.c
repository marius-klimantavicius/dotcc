/* Authored ABI boundary. Algorithms, contexts and dictionary training remain
 * upstream zstd. No native library is loaded by the managed product. */
#include <stddef.h>
#include <stdlib.h>
#include <string.h>
#include "zstd.h"
#include "zstd_errors.h"
#include "zdict.h"

typedef unsigned long long dz_size;

unsigned dz_version_number(void) { return ZSTD_versionNumber(); }
const char* dz_version(void) { return ZSTD_versionString(); }
unsigned dz_is_error(dz_size result) { return ZSTD_isError((size_t)result); }
int dz_error_code(dz_size result) { return (int)ZSTD_getErrorCode((size_t)result); }
const char* dz_error_name(dz_size result) { return ZSTD_getErrorName((size_t)result); }
dz_size dz_compress_bound(dz_size size) { return (dz_size)ZSTD_compressBound((size_t)size); }
dz_size dz_frame_content_size(const void* data, dz_size size) { return ZSTD_getFrameContentSize(data, (size_t)size); }
unsigned dz_frame_dictionary_id(const void* data, dz_size size) { return ZSTD_getDictID_fromFrame(data, (size_t)size); }
unsigned dz_dictionary_id(const void* data, dz_size size) { return ZSTD_getDictID_fromDict(data, (size_t)size); }

void* dz_create_cctx(void) { return ZSTD_createCCtx(); }
void* dz_create_dctx(void) { return ZSTD_createDCtx(); }
dz_size dz_free_cctx(void* context) { return (dz_size)ZSTD_freeCCtx((ZSTD_CCtx*)context); }
dz_size dz_free_dctx(void* context) { return (dz_size)ZSTD_freeDCtx((ZSTD_DCtx*)context); }
dz_size dz_c_reset(void* context) { return (dz_size)ZSTD_CCtx_reset((ZSTD_CCtx*)context, ZSTD_reset_session_only); }
dz_size dz_d_reset(void* context) { return (dz_size)ZSTD_DCtx_reset((ZSTD_DCtx*)context, ZSTD_reset_session_only); }
dz_size dz_c_parameter(void* context, int parameter, int value) { return (dz_size)ZSTD_CCtx_setParameter((ZSTD_CCtx*)context, (ZSTD_cParameter)parameter, value); }
dz_size dz_d_parameter(void* context, int parameter, int value) { return (dz_size)ZSTD_DCtx_setParameter((ZSTD_DCtx*)context, (ZSTD_dParameter)parameter, value); }
dz_size dz_c_pledge(void* context, dz_size size) { return (dz_size)ZSTD_CCtx_setPledgedSrcSize((ZSTD_CCtx*)context, size); }
dz_size dz_c_dictionary(void* context, const void* data, dz_size size) { return (dz_size)ZSTD_CCtx_loadDictionary((ZSTD_CCtx*)context, data, (size_t)size); }
dz_size dz_d_dictionary(void* context, const void* data, dz_size size) { return (dz_size)ZSTD_DCtx_loadDictionary((ZSTD_DCtx*)context, data, (size_t)size); }
dz_size dz_compress(void* context, void* destination, dz_size capacity, const void* source, dz_size size) { return (dz_size)ZSTD_compress2((ZSTD_CCtx*)context, destination, (size_t)capacity, source, (size_t)size); }
dz_size dz_decompress(void* context, void* destination, dz_size capacity, const void* source, dz_size size) { return (dz_size)ZSTD_decompressDCtx((ZSTD_DCtx*)context, destination, (size_t)capacity, source, (size_t)size); }

dz_size dz_c_stream(void* context, void* destination, dz_size capacity, const void* source, dz_size size, int directive, dz_size* consumed, dz_size* written)
{
    ZSTD_inBuffer input = { source, (size_t)size, 0 };
    ZSTD_outBuffer output = { destination, (size_t)capacity, 0 };
    size_t result = ZSTD_compressStream2((ZSTD_CCtx*)context, &output, &input, (ZSTD_EndDirective)directive);
    *consumed = (dz_size)input.pos;
    *written = (dz_size)output.pos;
    return (dz_size)result;
}
dz_size dz_d_stream(void* context, void* destination, dz_size capacity, const void* source, dz_size size, dz_size* consumed, dz_size* written)
{
    ZSTD_inBuffer input = { source, (size_t)size, 0 };
    ZSTD_outBuffer output = { destination, (size_t)capacity, 0 };
    size_t result = ZSTD_decompressStream((ZSTD_DCtx*)context, &output, &input);
    *consumed = (dz_size)input.pos;
    *written = (dz_size)output.pos;
    return (dz_size)result;
}
dz_size dz_train(void* dictionary, dz_size capacity, const void* samples, const dz_size* sizes, unsigned count)
{
    size_t* native_sizes = (size_t*)malloc((size_t)count * sizeof(size_t));
    size_t result;
    unsigned i;
    if (native_sizes == NULL) return (dz_size)(0 - (size_t)ZSTD_error_memory_allocation);
    for (i = 0; i < count; ++i) native_sizes[i] = (size_t)sizes[i];
    result = ZDICT_trainFromBuffer(dictionary, (size_t)capacity, samples, native_sizes, count);
    free(native_sizes);
    return (dz_size)result;
}

/* Probe actual upstream struct storage after translation; C# mirrors are absent. */
dz_size dz_abi(int index)
{
    switch (index) {
    case 0: return sizeof(void*);
    case 1: return sizeof(size_t);
    case 2: return sizeof(ZSTD_inBuffer);
    case 3: return _Alignof(ZSTD_inBuffer);
    case 4: return offsetof(ZSTD_inBuffer, src);
    case 5: return offsetof(ZSTD_inBuffer, size);
    case 6: return offsetof(ZSTD_inBuffer, pos);
    case 7: return sizeof(ZSTD_outBuffer);
    case 8: return _Alignof(ZSTD_outBuffer);
    case 9: return offsetof(ZSTD_outBuffer, dst);
    case 10: return offsetof(ZSTD_outBuffer, size);
    case 11: return offsetof(ZSTD_outBuffer, pos);
    default: return (dz_size)-1;
    }
}
int dz_abi_buffers(const void* source, dz_size size, void* destination, dz_size capacity, dz_size* consumed, dz_size* written)
{
    ZSTD_inBuffer input = { source, (size_t)size, 0 };
    ZSTD_outBuffer output = { destination, (size_t)capacity, 0 };
    size_t count = input.size < output.size ? input.size : output.size;
    memcpy(output.dst, input.src, count);
    input.pos += count;
    output.pos += count;
    *consumed = (dz_size)input.pos;
    *written = (dz_size)output.pos;
    return input.src == source && output.dst == destination && input.size == size && output.size == capacity;
}
