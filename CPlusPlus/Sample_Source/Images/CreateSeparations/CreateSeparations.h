//
// Copyright (c) 2004-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
// File: CreateSeparations.h
// 

// This structure represents one single plate of the separation.
// There will be one created for each color used on the page
typedef struct separation
{
    ASUns32  m_cmyk;   // Color;                // The CMYK representation of this color
    ASAtom   m_name;                            // The name, as an atom, of this color
    ASUns8  *m_buffer;                          // Pointer to the plate bitmap
    ASUns8  *m_mask;                            // Pointer to a mask bitmap, used only for process colors
    ASUns32  m_size;                            // Size of the plate bitmap
    ASUns32  m_mask_size;                       // Size of the mask bitmap
    ASBool   m_present;                         // True if this color used in image
    ASBool   m_process_color;                   // True if this is a process color
    ASBool   m_cyan,                            // True for specific process color
             m_magenta, 
             m_yellow, 
             m_black;    
    PDEImage m_masked_image;                    // A PDE Image used to display plate (sample purposes only)
    ASInt32  m_width,                           // Width and depth, in pixels, of image 
             m_depth;                    

    separation() : m_cmyk ( 0 ), m_name ( ASAtomNull ), m_buffer ( NULL ), m_mask ( NULL ),
                   m_size ( 0 ), m_mask_size ( 0 ), m_present ( false ), m_process_color ( false ), m_cyan ( false ),
                   m_magenta ( false ), m_yellow ( false ), m_black ( false ), m_masked_image ( 0 ),
                   m_width ( 0 ), m_depth ( 0 )
            {}
    separation ( ASInt32 width, ASInt32 depth, ASInt32 row_width,  const char* name ) :
                   m_cmyk ( 0 ), m_present ( false ), m_process_color ( true ), m_cyan ( false ),
                   m_magenta ( false ), m_yellow ( false ), m_black ( false ), m_masked_image ( 0 ),
                   m_width ( width ), m_depth ( depth )
    {
        m_name = ASAtomFromString ( name );
        m_size = width * depth;
        m_buffer = new ASUns8[m_size];
        memset ( m_buffer, 0, m_size );
        m_mask_size = row_width * depth;
        m_mask = new ASUns8[m_mask_size];
        memset ( m_mask, 0, m_mask_size );
    }

    ~separation()
    {
        if ( NULL != m_buffer )
        {
            delete[] m_buffer;
            m_buffer = NULL;
        }
        if ( NULL != m_mask )
        {
            delete[] m_mask;
            m_mask = NULL;
        }
        PDERelease ((PDEObject)m_masked_image);
    }

    void ReleaseBuffersIfAbsent()
    {
        if ( !m_present )
        {
            ReleaseBuffer();
            ReleaseMask();
        }
    }

    void ReleaseBuffer()
    {
        if ( m_buffer )
        {
            delete[] m_buffer;
            m_size = 0;
            m_buffer = NULL;
        }
    }

    void ReleaseMask()
    {
        if ( m_mask )
        {
            delete[] m_mask;
            m_mask_size = 0;
            m_mask = NULL;
        }
    }

    void ReallocBuffer ( ASInt32 width, ASInt32 depth, ASInt32 row_width )
    {
        if ( m_buffer )
        {
            delete[] m_buffer;
        }
        m_width = width;
        m_depth = depth;
        m_size = row_width * depth;
        m_buffer = new ASUns8[m_size];
        memset  ( m_buffer, 0, m_size );
        m_present = false;
    }

    void AddCMYK ( float c, float m, float y, float k )
    {
        m_cmyk |= ( FloatToASFixed ( c ) * 255 ) >> 16;
        m_cmyk |= ( ( FloatToASFixed ( m ) * 255 ) >> 16 ) * 256;
        m_cmyk |= ( ( FloatToASFixed ( y ) * 255 ) >> 16 ) * 256 * 256;
        m_cmyk |= ( ( FloatToASFixed ( k ) * 255 ) >> 16 ) * 256 * 256 * 256;
    }

} SEPARATION;

typedef std::vector<SEPARATION*> SeparationsContainer;



void DestroyColorList ( SeparationsContainer& );
void VerifyValidate ( std::string& ifname, std::string& ofname, const char* input, const char* output );
void  CreateColorList ( PDPage Page, SeparationsContainer* sc );
void ProcessSeparation ( ASUns8 *OriginalImage, ASInt32 InWidth, ASInt32 InDepth, SeparationsContainer* sc );
void CreateSeparation ( ASUns8 *OriginalImage, ASInt32 Width, ASInt32 Depth, SeparationsContainer* sc );
ASUns8 *CreateCMYKBitmap (PDPage Page, double Resolution, ASInt32 *Width, ASInt32 *Depth);
void CreateMaskedImage (SEPARATION *Sep);
