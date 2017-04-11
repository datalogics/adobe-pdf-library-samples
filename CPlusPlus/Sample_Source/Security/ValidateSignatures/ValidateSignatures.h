//
// Copyright (c) 2004-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//                                                                                                                        
// Portions Copyright (C) 2000-2017 Adobe Systems Incorporated:
//                                                                                                                        
// ADOBE SYSTEMS INCORPORATED
// Copyright (C) 2000-2017 Adobe Systems Incorporated
// All rights reserved.
//                                                                                                                        
// NOTICE: Adobe permits you to use, modify, and distribute this file
// in accordance with the terms of the Adobe license agreement
// accompanying it. If you have received this file from a source other
// than Adobe, then your use, modification, or distribution of it
// requires the prior written permission of Adobe.
//

// Construct an MD5 digest of a set of ranges of text
typedef struct {
    ASUns32    state[4];      /* state (ABCD) */
    ASUns32    count[2];      /* number of bits, modulo 2^64 (lsb first) */
    ASUns8     buffer[64];    /* input buffer */
} MD5_CTX; 

static ASUns8 PADDING[64] = {
    0x80, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 
};

#define S11 7
#define S12 12
#define S13 17
#define S14 22
#define S21 5
#define S22 9
#define S23 14
#define S24 20
#define S31 4
#define S32 11
#define S33 16
#define S34 23
#define S41 6
#define S42 10
#define S43 15
#define S44 21

// F, G, H and I are basic MD5 functions.
#define F(x, y, z) (((x) & (y)) | ((~x) & (z)))
#define G(x, y, z) (((x) & (z)) | ((y) & (~z)))
#define H(x, y, z) ((x) ^ (y) ^ (z))
#define I(x, y, z) ((y) ^ ((x) | (~z)))

// ROTATE_LEFT rotates x left n bits.
#define ROTATE_LEFT(x, n) (((x) << (n)) | ((x) >> (32-(n))))

// FF, GG, HH, and II transformations for rounds 1, 2, 3, and 4.
// Rotation is separate from addition to prevent recomputation.
#define FF(a, b, c, d, x, s, ac) { \
    (a) += F ((b), (c), (d)) + (x) + (ASUns32)(ac); \
    (a) = ROTATE_LEFT ((a), (s)); \
    (a) += (b); \
    }

#define GG(a, b, c, d, x, s, ac) { \
    (a) += G ((b), (c), (d)) + (x) + (ASUns32)(ac); \
    (a) = ROTATE_LEFT ((a), (s)); \
    (a) += (b); \
    }

#define HH(a, b, c, d, x, s, ac) { \
    (a) += H ((b), (c), (d)) + (x) + (ASUns32)(ac); \
    (a) = ROTATE_LEFT ((a), (s)); \
    (a) += (b); \
    }

#define II(a, b, c, d, x, s, ac) { \
    (a) += I ((b), (c), (d)) + (x) + (ASUns32)(ac); \
    (a) = ROTATE_LEFT ((a), (s)); \
    (a) += (b); \
    }

static void Encode (ASUns8 *output, ASUns32 *input, ASUns32 len);
static void Decode (ASUns32 *output, ASUns8 *input, ASUns32 len);
static void MD5Transform (ASUns32 state[4],  ASUns8 block[64]);
static void MD5Init (MD5_CTX *context);
static void MD5Update (MD5_CTX *context, ASUns8 *input, ASUns32 inputLen);
static void MD5Final (ASUns8 *digest, MD5_CTX *context);
static ASBool   VerifySig (CosObj Signature);
static ASBool   EnumerateFields (CosObj Fields, ASBool *Invalid, ASUns32 *Signatures);

