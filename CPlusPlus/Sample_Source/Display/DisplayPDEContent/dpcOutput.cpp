//
// Copyright (c) 2008-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//

#include <iostream>
#include <fstream>

#include "dpcOutput.h"

Outputter *Outputter::m_instance = 0;
int Outputter::m_indentation = 0;
std::string Outputter::m_filename;
std::ostream *Outputter::m_ofs;

Outputter *Outputter::Inst() {
    if (!m_instance) {
        m_instance = new Outputter();
    }
    return m_instance;
}

void Outputter::Init(const char *filename) {
    m_filename = filename;
    m_ofs = new std::ofstream(m_filename.c_str());
}

void Outputter::Indent() { m_indentation += 4; }

void Outputter::Outdent() {
    m_indentation -= 4;
    if (m_indentation < 0) {
        // Shouldn't happen if indents/outdents are matched, but just in case
        m_indentation = 0;
    }
}

std::ostream &Outputter::GetOfs() {
    for (int i = 0; i < m_indentation; ++i) {
        *m_ofs << ' ';
    }
    return *m_ofs;
}
