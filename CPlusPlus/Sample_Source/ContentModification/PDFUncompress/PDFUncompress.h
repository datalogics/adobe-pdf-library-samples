//
// Copyright (c) 2010-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//

CosObj Uncompress(CosObj Stream);
CosObj UncompressAnnot(CosObj Annot);
ASBool CompressXobj(CosObj Key, CosObj Value, void *Data);
ASBool UncompressResource(CosObj Key, CosObj Value, void *clientData);
ASBool SubAnnot(CosObj Key, CosObj Value, void *Data);
