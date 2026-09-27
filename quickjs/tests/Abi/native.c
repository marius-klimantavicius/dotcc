/* Native truth for the actual upstream public header, never mirror structs. */
#include "quickjs.h"
#include <stdio.h>
#include <stddef.h>
#include <math.h>
#include <string.h>
static JSValue identity(JSContext *ctx, JSValueConst self, int argc, JSValueConst *argv) {
    (void)self;
    return argc == 1 ? JS_DupValue(ctx, argv[0]) : JS_UNDEFINED;
}
int quickjs_abi_probe(JSContext *ctx) {
    printf("ABI {\"valueSize\":%zu,\"valueAlign\":%zu,\"payloadOffset\":%zu,\"tagOffset\":%zu,\"pointerSize\":%zu,\"longSize\":%zu}\n",
           sizeof(JSValue), _Alignof(JSValue), offsetof(JSValue, u), offsetof(JSValue, tag), sizeof(void *), sizeof(long));
    printf("ALLOCATOR {\"stateSize\":%zu,\"countOffset\":%zu,\"sizeOffset\":%zu,\"limitOffset\":%zu,\"opaqueOffset\":%zu}\n",
           sizeof(JSMallocState), offsetof(JSMallocState, malloc_count), offsetof(JSMallocState, malloc_size),
           offsetof(JSMallocState, malloc_limit), offsetof(JSMallocState, opaque));
    printf("CHAR {\"promotion255\":%d}\n", (int)(char)0xff);
    if ((int)(char)0xff != 255) return 1;
    JSValue f = JS_NewCFunction(ctx, identity, "identity", 1);
    JSValue values[] = { JS_NewInt32(ctx, -42), JS_NewFloat64(ctx, -0.0), JS_NewFloat64(ctx, NAN),
        JS_NewBigInt64(ctx, 42), JS_NewBigInt64(ctx, INT64_MAX), JS_Eval(ctx, "1n << 80n", 9, "<abi-bigint>", JS_EVAL_TYPE_GLOBAL), JS_NewStringLen(ctx, "A\0B", 3) };
    int failed = JS_IsException(f);
    for (unsigned i = 0; i < sizeof(values)/sizeof(values[0]); i++) {
        JSValue result = JS_Call(ctx, f, JS_UNDEFINED, 1, &values[i]);
        failed |= JS_IsException(values[i]) || JS_IsException(result);
        failed |= JS_VALUE_GET_TAG(result) != JS_VALUE_GET_TAG(values[i]);
        failed |= memcmp(&result.u, &values[i].u, sizeof(JSValueUnion)) != 0;
        printf("VALUE %u tag=%d payload=%016llx\n", i, JS_VALUE_GET_TAG(result),
               (unsigned long long)(i >= 5 ? 0 : result.u.uint64));
        if (i == 6 && !JS_IsException(result)) {
            size_t length = 0;
            const char *bytes = JS_ToCStringLen(ctx, &length, result);
            failed |= !bytes || length != 3 || memcmp(bytes, "A\0B", 3) != 0;
            JS_FreeCString(ctx, bytes);
        }
        JS_FreeValue(ctx, result); JS_FreeValue(ctx, values[i]);
    }
    JS_FreeValue(ctx, f);
    return failed;
}
