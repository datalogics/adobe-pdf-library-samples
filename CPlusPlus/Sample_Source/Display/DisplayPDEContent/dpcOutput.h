//
// Copyright (c) 2008-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//

#include <string>
#include <iostream>

// Singleton -styled object for managing printing
class Outputter {
  public:
    static Outputter *Inst();
    static void Init(const char *);
    static void Indent();
    static void Outdent();
    static std::ostream &GetOfs();

  private:
    Outputter(){};
    Outputter(const Outputter &);
    static int m_indentation;
    static std::string m_filename;
    static std::ostream *m_ofs;

    static Outputter *m_instance;
};
