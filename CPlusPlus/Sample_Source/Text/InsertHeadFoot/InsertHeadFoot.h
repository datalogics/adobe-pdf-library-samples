//
// Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//
// File InsertHeadFoot.h
//

#define ALLOC_INCREMENT 8192
class CStreamContents {
  public:
    CStreamContents(CosObj coStream) : m_allocated_size(0), m_used_size(0), m_buffer(NULL) {
        // Read in the stream.  Since there is no API call to reveal the data length, and ASStmRead
        // cannot be used repeatedly to read additional chunks of data, we have to keep reading it
        // in its entirety, increasing the buffer each time, until the API tells us it didn't need
        // all the room we provided.
        ASUns32 bytes_read;
        do {
            m_buffer = (char *)realloc(m_buffer, m_allocated_size += ALLOC_INCREMENT);
            ASStm stm = CosStreamOpenStm(coStream, cosOpenFiltered);
            bytes_read = ASStmRead(m_buffer, 1, m_allocated_size, stm);
            ASStmClose(stm);
        } while ((0 != bytes_read) &&               // Check for empty stream
                 (bytes_read >= m_allocated_size)); // == should suffice; checking >= to be safe

        m_used_size = bytes_read;
    }

    ~CStreamContents() {
        if (m_buffer) {
            free(m_buffer);
            m_buffer = NULL;
        }
        m_allocated_size = m_used_size = 0;
    }

    const char *GetBuffer() { return m_buffer; }
    ASUns32 GetBufferSize() { return m_allocated_size; }
    ASUns32 GetSize() { return m_used_size; }

    // Prepend move/restore command to buffer
    void AddCommands(const char *sMoveCmd, const char *sRestoreCmd) {
        ASUns32 move_cmd_bytes = sMoveCmd ? strlen(sMoveCmd) : 0;
        ASUns32 rest_cmd_bytes = sRestoreCmd ? strlen(sRestoreCmd) : 0;
        ASUns32 cmd_bytes = move_cmd_bytes + rest_cmd_bytes;
        if (0 == cmd_bytes) {
            return;
        }
        ASUns32 free_bytes = m_allocated_size - m_used_size;
        if (free_bytes < cmd_bytes) {
            // Need to get some more space to accommodate command strings
            m_buffer = (char *)realloc(m_buffer, m_allocated_size = m_used_size + cmd_bytes);
        }

        // Move stream bytes over and insert the move command
        if (move_cmd_bytes) {
            // Note use of 'memmove' (not 'strcpy') - it accounts for overlapping buffers
            memmove(m_buffer + move_cmd_bytes, m_buffer, m_used_size);
            m_used_size += move_cmd_bytes;
            memmove(m_buffer, sMoveCmd, move_cmd_bytes);
        }

        // Append restore command
        if (rest_cmd_bytes) {
            memmove(m_buffer + m_used_size, sRestoreCmd, rest_cmd_bytes);
            m_used_size += rest_cmd_bytes;
        }
    }

  private:
    ASUns32 m_allocated_size;
    ASUns32 m_used_size;
    char *m_buffer;
};

ASBool GetPassWord(PDDoc Doc);
CosObj BuildSecurityHead(PDDoc InDoc, ASFixedRect *HeadSpace, ASFixedMatrix *InMatrix, CosObj &cosWorkFont);
CosObj BuildSecurityFoot(PDDoc InDoc, ASFixedRect *FootSpace, ASFixedMatrix *InMatrix, CosObj &cosWorkFont);
CosObj CopyStreamAddDisplacementStart(CosObj Stream, ASFixedMatrix *Matrix);
CosObj CopyStreamAddDisplacement(CosObj Stream, ASFixedMatrix *Matrix);
CosObj CopyStreamAddRestore(CosObj Stream);

std::string FixedMatrixToStrings(ASFixedMatrix *pM);
