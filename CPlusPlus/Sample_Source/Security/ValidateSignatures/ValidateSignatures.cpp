//
// Copyright (c) 2004-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//                                                                                                                        
// PDF documents can allow readers to add digital signatures.  In many cases someone
// who signs a PDF document is simply showing that he or she has read the document and
// approves of the content.  But it is possible for a digital signature or digital
// signatures to be added to a PDF document so that the document becomes binding, and
// can be presented a legal document or business contract just as valid as if the document
// were printed and signed by hand. Recent changes in Federal and State law make this possible.
//
// But for a PDF file to be used in this setting, it must be possible to lock the PDF document
// after a digital signature is added, to prevent any further changes from being applied. More
// important, it must also be possible to validate the signature on that document, to demonstrate
// that the PDF document was not altered or tampered with after a digital signature was added. 
// 
// This sample analyzes an input PDF document, lists the number of signatures found in that document,
// and determines if those signatures are valid.  This represents a programmatic way to validate a PDF document.
//
// ValidateSignatures does not define a default output file.
//
// For more detail see the description of the ValidateSignature sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#validatesignature

#include "CosCalls.h"

#include "InitializeLibrary.h"
#include "APDFLDoc.h"

#include "ValidateSignatures.h"

#define INPUT_DIR "../../../../Resources/Sample_Input/"
#define INPUT_FILE "Signed_Gibson_PKCS7_DETACHED_Sha256.pdf"

int main (int argc, char *argv[])
{
    APDFLib libInit;       // Initialize the library
    if (libInit.isValid() == false)
    {
        ASErrorCode errCode = libInit.getInitError();
        std::cout << "APDFL Library initialization failed with code " << errCode << std::endl;
        return errCode;
    }

    std::string csInputFile ( argc > 1 ? argv[1] : INPUT_DIR INPUT_FILE );
    std::cout << "Analyzing file " << csInputFile.c_str() << std::endl;

DURING
    // Open the document
    APDFLDoc apdflDoc ( csInputFile.c_str(), true);
    PDDoc pdDoc = apdflDoc.getPDDoc();

    // Get the COS version of the document 
    CosDoc cosDoc = PDDocGetCosDoc (pdDoc);

    // Obtain the document root or document catalog dictionary
    CosObj cosRoot = CosDocGetRoot (cosDoc);

    // Get the AcroForms dictionary from the document root
    CosObj AcroForms = CosDictGet (cosRoot, ASAtomFromString ("AcroForm"));

    // If it is not there, then shut down
    if (CosObjGetType (AcroForms) != CosDict)
    {
        std::cout << "Document has no Acroforms!" << std::endl;
        return -1;
    }

    // Obtain the fields array
    CosObj Fields = CosDictGet (AcroForms, ASAtomFromString ("Fields"));

    // Search the fields for signatures, and set Invalid to true if an invalid signature is seen.
    // Increment signature count for each signature seen, and return false if an invalid form is seen.
    ASBool Invalid = false;
    ASUns32 Signatures = 0;
    if (!EnumerateFields (Fields, &Invalid, &Signatures))
    {
        std::cout << "Document has invalid Acroforms!" << std::endl;
        PDDocClose(pdDoc);
        return -2;
    }

    if (Invalid)
    {
        std::cout << "There were " << Signatures << " signatures, at least one was invalid." << std::endl;
    }
    else
    {
        std::cout << "There were " << Signatures << " valid signatures." << std::endl;
    }
    PDDocClose(pdDoc);

HANDLER
    APDFLib::displayError(ERRORCODE);
    return ERRORCODE;
END_HANDLER
    return 0;
}


// Verify that the signature is correct
// Return TRUE for valid signature, FALSE for invalid signature
static ASBool   VerifySig (CosObj Signature)
{
    if (CosObjGetType (Signature) != CosDict)
    {
        // If this is not a dictionary, it cannot be a valid signature!
        return (false);
    }

    // Byte range is an array of segments of the document which must be digested via
    // MD5 to produce a verifiable digest for the document
    CosObj  ByteRange = CosDictGet (Signature, ASAtomFromString ("ByteRange"));
    // There must be a byte range object
    if (CosObjGetType (ByteRange) != CosArray)
        return (false);

    // The length must be a multiple of two
    ASUns32 Ranges = CosArrayLength (ByteRange);
    if ((Ranges % 2) != 0)
        return (false);
    Ranges /= 2;

    // Obtain the file that contains the document. First, CosDoc from CosObj,
    // then PDDoc from CosDoc, then ASFile from PDDoc
    CosDoc  cosDoc = CosObjGetDoc (Signature);
    PDDoc pdDoc = PDDocFromCosDoc (cosDoc);
    ASFile asFile = PDDocGetFile (pdDoc);

    // Initialize the MD5 digest
    MD5_CTX md5Context;
    MD5Init (&md5Context);

    ASUns32 Index;

    // The result of the MD5 process is a 16 byte MD5 Digest
    ASUns8  Digest[16];

    for (Index = 0; Index < Ranges; Index++)
    {
        CosObj  BaseObj = CosArrayGet (ByteRange, Index * 2);
        CosObj  LengthObj = CosArrayGet (ByteRange, (Index * 2) + 1);

        // Both base and length of a byte range must be integers
        if ((CosObjGetType (BaseObj) != CosInteger) ||
            (CosObjGetType (LengthObj) != CosInteger))
            return (false);

        // Get the base and the length. Use 64 bit values,
        // so that the large files created by later APDFL's can also be read
        ASUns64 Base = CosInteger64Value (BaseObj);
        ASUns64 Length = CosInteger64Value (LengthObj);

        // Set the file to the base position
        ASFileSetPos64 (asFile, Base);

        // Read and digest the data in 4096 byte chunks
        while (Length)
        {
            ASUns8  Buffer[4096];
            ASUns32 Size;
            ASUns32 ReadSize = 4096;

            if (Length < 4096)
                ReadSize = (ASUns32)Length;

            Size = ASFileRead (asFile, (char *)Buffer, ReadSize);
            Length -= Size;
            MD5Update (&md5Context, Buffer, Size);
        }

    }

    // Finish the MD5 and obtain a Digest
    MD5Final (Digest, &md5Context);

    // Digest now contains the calculated digest value for the current
    // document. If is is the same as the stored content value, then the
    // signature is still valid. If it is not, then the signature is not valid

    // The contents field always contains the stored digest,
    // Usually encrypted
    CosObj  Contents = CosDictGet (Signature, ASAtomFromString ("Contents"));
    if (CosObjGetType (Contents) != CosString)
        return (false);
    ASUns32  StoredContentSize;
    ASUns8  *StoredContent = (ASUns8 *)CosCopyStringValue (Contents, (ASTCount *)&StoredContentSize);

    // This is where we will build the stored digest
    ASUns8  StoredDigest[16];

    // SubFilter describes the filter that encrypts the signature value and key
    CosObj  CryptType = CosDictGet (Signature, ASAtomFromString ("SubFilter"));
    if (CosObjGetType (CryptType) != CosName)
        return (false);

    // It must be one of these three for this code to work
    ASAtom  CryptName = CosNameValue (CryptType);

    if ( ( CryptName != ASAtomFromString ("adbe.pkcs7.sha1") ) &&
         ( CryptName != ASAtomFromString ("adbe.pkcs7.detached") ) &&
         ( CryptName != ASAtomFromString ("adbe.x509.rsa_sha1") ) )
    {
        return (false);
    }

    // The code needed to complete the function that decrypts the stored digest for the PDF document
    // is not included in this sample program. This function always returns "false" in response.
    //

    // Free the copy of the stored content
    ASfree (StoredContent);

    // Compare the stored digest to the calculated digest
    if (!memcmp ((char *)StoredDigest, (char *)Digest, 16))
        return (true);
    else
        return (false);
}


ASBool   EnumerateFields (CosObj Fields, ASBool *Invalid, ASUns32 *Signatures)
{
    // Type MUST be array
    if (CosObjGetType (Fields) != CosArray)
        return false;

    // For each field in the array, the object may be a list of sub fields, or a field.
    // If the object is a field, the program ignores anything that is not a signature.
    // If it is a signature, the program verifies it.
    // If it is not a field, the program works at the level of the children.
    ASUns32 FieldCount = CosArrayLength (Fields);
    ASUns32 Index;
    for (Index = 0; Index < FieldCount; Index++)
    {
        CosObj  Field = CosArrayGet (Fields, Index);
        CosObj  FieldTypeObj = CosDictGet (Field, ASAtomFromString ("FT"));
        CosObj  SubFields = CosDictGet (Field, ASAtomFromString ("Kids"));
        ASAtom  FieldType;

        if (CosObjGetType (FieldTypeObj) == CosName)
        {
            // This is a terminal field (it may have children, 
            // but they must all be of the same type)
            FieldType = CosNameValue (FieldTypeObj);
            if (FieldType == ASAtomFromString ("Sig"))
            {
                // This is a signature, go and verify it.  Ignore anything else
                *Signatures += 1;   // Count the signatures
                if (CosObjGetType (Field) != CosDict)
                    return false;

                // The value field of the annotation is the signature
                CosObj  SigValue = CosDictGet (Field, ASAtomFromString ("V"));
                *Invalid |= !VerifySig (SigValue);
            }
        }
        else
            // Descend into the field's children
            EnumerateFields (SubFields, Invalid, Signatures);
    }

    return true;
}

// Encodes input (UINT4) into output (unsigned char). Assumes len is a multiple of 4.
void Encode (ASUns8 *output, ASUns32 *input, ASUns32 len)
{
    ASUns32 i, j;

    for (i = 0, j = 0; j < len; i++, j += 4) 
    {
        output[j] = (ASUns8)(input[i] & 0xff);
        output[j+1] = (ASUns8)((input[i] >> 8) & 0xff);
        output[j+2] = (ASUns8)((input[i] >> 16) & 0xff);
        output[j+3] = (ASUns8)((input[i] >> 24) & 0xff);
    }
} 

// Decodes input (unsigned char) into output (UINT4). Assumes len is a multiple of 4.
void Decode (ASUns32 *output, ASUns8 *input, ASUns32 len)
{
    ASUns32 i, j;

    for (i = 0, j = 0; j < len; i++, j += 4)
    {
        output[i] = ((ASUns32)input[j]) | (((ASUns32)input[j+1]) << 8) |
                    (((ASUns32)input[j+2]) << 16) | (((ASUns32)input[j+3]) << 24);
    }
}

// MD5 basic transformation. Transforms state based on block.
void MD5Transform (ASUns32 state[4],  ASUns8 block[64])
{
    ASUns32 a = state[0], b = state[1], c = state[2], d = state[3], x[16];

    Decode (x, block, 64); 
    /* Round 1 */
    FF (a, b, c, d, x[ 0], S11, 0xd76aa478); /* 1 */
    FF (d, a, b, c, x[ 1], S12, 0xe8c7b756); /* 2 */
    FF (c, d, a, b, x[ 2], S13, 0x242070db); /* 3 */
    FF (b, c, d, a, x[ 3], S14, 0xc1bdceee); /* 4 */
    FF (a, b, c, d, x[ 4], S11, 0xf57c0faf); /* 5 */
    FF (d, a, b, c, x[ 5], S12, 0x4787c62a); /* 6 */
    FF (c, d, a, b, x[ 6], S13, 0xa8304613); /* 7 */
    FF (b, c, d, a, x[ 7], S14, 0xfd469501); /* 8 */
    FF (a, b, c, d, x[ 8], S11, 0x698098d8); /* 9 */
    FF (d, a, b, c, x[ 9], S12, 0x8b44f7af); /* 10 */
    FF (c, d, a, b, x[10], S13, 0xffff5bb1); /* 11 */
    FF (b, c, d, a, x[11], S14, 0x895cd7be); /* 12 */
    FF (a, b, c, d, x[12], S11, 0x6b901122); /* 13 */
    FF (d, a, b, c, x[13], S12, 0xfd987193); /* 14 */
    FF (c, d, a, b, x[14], S13, 0xa679438e); /* 15 */
    FF (b, c, d, a, x[15], S14, 0x49b40821); /* 16 */

    /* Round 2 */
    GG (a, b, c, d, x[ 1], S21, 0xf61e2562); /* 17 */
    GG (d, a, b, c, x[ 6], S22, 0xc040b340); /* 18 */
    GG (c, d, a, b, x[11], S23, 0x265e5a51); /* 19 */
    GG (b, c, d, a, x[ 0], S24, 0xe9b6c7aa); /* 20 */
    GG (a, b, c, d, x[ 5], S21, 0xd62f105d); /* 21 */
    GG (d, a, b, c, x[10], S22,  0x2441453); /* 22 */
    GG (c, d, a, b, x[15], S23, 0xd8a1e681); /* 23 */
    GG (b, c, d, a, x[ 4], S24, 0xe7d3fbc8); /* 24 */
    GG (a, b, c, d, x[ 9], S21, 0x21e1cde6); /* 25 */
    GG (d, a, b, c, x[14], S22, 0xc33707d6); /* 26 */
    GG (c, d, a, b, x[ 3], S23, 0xf4d50d87); /* 27 */
    GG (b, c, d, a, x[ 8], S24, 0x455a14ed); /* 28 */ 
    GG (a, b, c, d, x[13], S21, 0xa9e3e905); /* 29 */ 
    GG (d, a, b, c, x[ 2], S22, 0xfcefa3f8); /* 30 */ 
    GG (c, d, a, b, x[ 7], S23, 0x676f02d9); /* 31 */ 
    GG (b, c, d, a, x[12], S24, 0x8d2a4c8a); /* 32 */ 

    /* Round 3 */
    HH (a, b, c, d, x[ 5], S31, 0xfffa3942); /* 33 */
    HH (d, a, b, c, x[ 8], S32, 0x8771f681); /* 34 */
    HH (c, d, a, b, x[11], S33, 0x6d9d6122); /* 35 */
    HH (b, c, d, a, x[14], S34, 0xfde5380c); /* 36 */
    HH (a, b, c, d, x[ 1], S31, 0xa4beea44); /* 37 */
    HH (d, a, b, c, x[ 4], S32, 0x4bdecfa9); /* 38 */
    HH (c, d, a, b, x[ 7], S33, 0xf6bb4b60); /* 39 */
    HH (b, c, d, a, x[10], S34, 0xbebfbc70); /* 40 */
    HH (a, b, c, d, x[13], S31, 0x289b7ec6); /* 41 */
    HH (d, a, b, c, x[ 0], S32, 0xeaa127fa); /* 42 */
    HH (c, d, a, b, x[ 3], S33, 0xd4ef3085); /* 43 */
    HH (b, c, d, a, x[ 6], S34,  0x4881d05); /* 44 */
    HH (a, b, c, d, x[ 9], S31, 0xd9d4d039); /* 45 */
    HH (d, a, b, c, x[12], S32, 0xe6db99e5); /* 46 */
    HH (c, d, a, b, x[15], S33, 0x1fa27cf8); /* 47 */
    HH (b, c, d, a, x[ 2], S34, 0xc4ac5665); /* 48 */

    /* Round 4 */
    II (a, b, c, d, x[ 0], S41, 0xf4292244); /* 49 */
    II (d, a, b, c, x[ 7], S42, 0x432aff97); /* 50 */
    II (c, d, a, b, x[14], S43, 0xab9423a7); /* 51 */
    II (b, c, d, a, x[ 5], S44, 0xfc93a039); /* 52 */
    II (a, b, c, d, x[12], S41, 0x655b59c3); /* 53 */
    II (d, a, b, c, x[ 3], S42, 0x8f0ccc92); /* 54 */
    II (c, d, a, b, x[10], S43, 0xffeff47d); /* 55 */
    II (b, c, d, a, x[ 1], S44, 0x85845dd1); /* 56 */
    II (a, b, c, d, x[ 8], S41, 0x6fa87e4f); /* 57 */
    II (d, a, b, c, x[15], S42, 0xfe2ce6e0); /* 58 */
    II (c, d, a, b, x[ 6], S43, 0xa3014314); /* 59 */
    II (b, c, d, a, x[13], S44, 0x4e0811a1); /* 60 */
    II (a, b, c, d, x[ 4], S41, 0xf7537e82); /* 61 */
    II (d, a, b, c, x[11], S42, 0xbd3af235); /* 62 */
    II (c, d, a, b, x[ 2], S43, 0x2ad7d2bb); /* 63 */
    II (b, c, d, a, x[ 9], S44, 0xeb86d391); /* 64 */

    state[0] += a; 
    state[1] += b; 
    state[2] += c; 
    state[3] += d; 

    memset ((char *)x, 0, sizeof (x));
} 

void MD5Init (MD5_CTX *context)
{
    context->count[0] = context->count[1] = 0;
    // Load magic initialization constants.
    context->state[0] = 0x67452301;
    context->state[1] = 0xefcdab89;
    context->state[2] = 0x98badcfe;
    context->state[3] = 0x10325476;
} 

// MD5 block update operation. Continues an MD5 message-digest
// operation, processing another message block, and updating the context. 
void MD5Update (MD5_CTX *context, ASUns8 *input, ASUns32 inputLen)
{
    ASUns32 i, index, partLen;

    // Compute number of bytes mod 64 
    index = ((context->count[0] >> 3) & 0x3F);

    // Update number of bits
    if ((context->count[0] += (inputLen << 3)) < (inputLen << 3))
        context->count[1]++;
    context->count[1] += (inputLen >> 29);

    partLen = 64 - index;

    // Transform as many times as possible.
    if (inputLen >= partLen) {
        memcpy((char *)&context->buffer[index], (char *)input, partLen);
        MD5Transform (context->state, context->buffer);

        for (i = partLen; i + 63 < inputLen; i += 64)
            MD5Transform (context->state, &input[i]);

        index = 0;
    }
    else
        i = 0;

    // Buffer remaining input
    memcpy ((char *)&context->buffer[index], (char *)&input[i], inputLen-i);
}

void MD5Final (ASUns8 *digest, MD5_CTX *context)
{
    ASUns8    bits[8];
    ASUns32   index, padLen;

    // Save number of bits
    Encode (bits, context->count, 8);

    // Pad out to 56 mod 64.
    index = ((context->count[0] >> 3) & 0x3f);
    padLen = (index < 56) ? (56 - index) : (120 - index);
    MD5Update (context, PADDING, padLen);

    // Append length (before padding)
    MD5Update (context, bits, 8);

    // Store state in digest
    Encode (digest, context->state, 16);

    memset ((char *)context, 0, sizeof (*context));
} 
