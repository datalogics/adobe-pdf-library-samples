/*
// Copyright 2015, Datalogics, Inc. All rights reserved.
//
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
*/

#ifndef _SAMPLE_CUSTOM_EXCEPTIONS_H_
#define _SAMPLE_CUSTOM_EXCEPTIONS_H_

#include <stdexcept>

/* Custom Exceptions thrown by Datalogics Samples */

/* The nature of these exceptions is essentially identical to what one finds
 * in Java and C# compositions. The exceptions tend to be self documenting
 * (the typename of the exception being highly descriptive). They are also
 * easy to collect into chains of exceptions (think: Matryoshka doll), further
 * aiding the software engineer's analysis of what / where things went wrong */

struct UnimplementedException : public std::exception {};
struct OutOfMemoryException : public std::exception {};
struct BadArgumentException : public std::exception {
  public:
    BadArgumentException() : std::exception() {}
    BadArgumentException(const char *msg) : std::exception(msg) {}
};

struct GeneralException : public std::exception { /* catch all for unknown conditions (and basis for more specific types) */
  public:
    GeneralException() : std::exception() {}
    GeneralException(const char *msg) : std::exception(msg) {}
};

struct StringException : public GeneralException { /* basis for other string (usually unicode to non-unicode translations) related failures */
  public:
    StringException() : GeneralException() {}
    StringException(const char *msg) : GeneralException(msg) {}
};
struct StringContainsUnsupportedUnicodeException
    : public StringException { /* at times, pdfl cannot support unicode */
  public:
    StringContainsUnsupportedUnicodeException() : StringException() {}
    StringContainsUnsupportedUnicodeException(const char *msg) : StringException(msg) {}
};

struct FileException : public GeneralException { /* basis for other file system related failures */
  public:
    FileException() : GeneralException() {}
    FileException(const char *msg) : GeneralException(msg) {}
};
struct FileNotFoundException : public FileException { /* file does not exist, bad path, etc. */
  public:
    FileNotFoundException() : FileException() {}
    FileNotFoundException(const char *msg) : FileException(msg) {}
};
struct FileAccessException
    : public FileException { /* general file system access related failure (file locked, creds don't allow read or write, etc) */
  public:
    FileAccessException() : FileException() {}
    FileAccessException(const char *msg) : FileException(msg) {}
};
struct InvalidFileException
    : public FileException { /* the file is the wrong type or locked or something similar */
  public:
    InvalidFileException() : FileException() {}
    InvalidFileException(const char *msg) : FileException(msg) {}
};

struct PrintException : public GeneralException { /* general print related failure */
  public:
    PrintException() : GeneralException() {}
    PrintException(const char *msg) : GeneralException(msg) {}
};
struct PrintSetupException
    : public PrintException { /* something that is not legal happened (e.g., conflicting settings) */
  public:
    PrintSetupException() : PrintException() {}
    PrintSetupException(const char *msg) : PrintException(msg) {}
};
struct InvalidPrinterNameException
    : public PrintException { /* printer does not exist (i.e., not installed) by that name */
  public:
    InvalidPrinterNameException() : PrintException() {}
    InvalidPrinterNameException(const char *msg) : PrintException(msg) {}
};

struct PlatformPrintException : public PrintException { /* platform print related failure */
  public:
    PlatformPrintException() : PrintException() {}
    PlatformPrintException(const char *msg) : PrintException(msg) {}
};
struct PlatformPrintAccessException
    : public PrintException { /* platform print related failure (due to policy / credential restrictions) */
  public:
    PlatformPrintAccessException() : PrintException() {}
    PlatformPrintAccessException(const char *msg) : PrintException(msg) {}
};

struct ExportPostScriptException : public PrintException { /* export postscript related failure */
  public:
    ExportPostScriptException() : PrintException() {}
    ExportPostScriptException(const char *msg) : PrintException(msg) {}
};

#endif /* _SAMPLE_CUSTOM_EXCEPTIONS_H_ */
