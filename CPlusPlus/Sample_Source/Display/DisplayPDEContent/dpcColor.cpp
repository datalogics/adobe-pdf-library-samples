//
// Copyright (c) 2008-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//

#include <algorithm>
#include <string>
#include <vector>
#include <sstream>

#include "ASCalls.h"
#include "CosCalls.h"
#include "PERCalls.h"
#include "PEWCalls.h"
#include "PagePDECntCalls.h"
#include "DisplayPDEContent.h"

class CColorSpaces {
  public:
    enum Colorspaces {
        Unknown_cs = 0,
        Gray_cs = 1,
        RGB_cs,
        CMYK_cs,
        CalGray_cs,
        CalRGB_cs,
        LAB_cs,
        ICC_cs,
        Pattern_cs,
        Index_cs,
        Separation_cs,
        DeviceN_cs
    };

    struct ColorSelect {
        std::string m_name;
        ASAtom m_atom;
        Colorspaces m_group;
        ColorSelect(const char *name, Colorspaces cs) : m_name(name), m_group(cs) {
            m_atom = m_name.length() ? ASAtomFromString(name) : ASAtomNull;
        }
    };

    CColorSpaces() {
        m_spaces.push_back(ColorSelect("DeviceGray", Gray_cs));
        m_spaces.push_back(ColorSelect("DeviceRGB", RGB_cs));
        m_spaces.push_back(ColorSelect("DeviceCMYK", CMYK_cs));
        m_spaces.push_back(ColorSelect("CalGray", CalGray_cs));
        m_spaces.push_back(ColorSelect("CalRGB", CalRGB_cs));
        m_spaces.push_back(ColorSelect("Lab", LAB_cs));
        m_spaces.push_back(ColorSelect("ICCBased", ICC_cs));
        m_spaces.push_back(ColorSelect("Pattern", Pattern_cs));
        m_spaces.push_back(ColorSelect("Indexed", Index_cs));
        m_spaces.push_back(ColorSelect("Separation", Separation_cs));
        m_spaces.push_back(ColorSelect("DeviceN", DeviceN_cs));
        m_spaces.push_back(ColorSelect("", Unknown_cs));
    }

    struct MyComp {
        explicit MyComp(ASAtom a) : m_a(a) {}
        inline bool operator()(const ColorSelect &cs) const { return cs.m_atom == m_a; }

      private:
        ASAtom m_a;
    };

    ColorSelect &FindColor(ASAtom c) {
        return *std::find_if(m_spaces.begin(), m_spaces.end(), MyComp(c));
    }

  private:
    std::vector<ColorSelect> m_spaces;
};

std::string DisplayColor(PDEColorSpec *Color) {
    static CColorSpaces sColorSpaces;
    char Amount[20][20];
    PDEColorSpace Space = Color->space;
    ASAtom SpaceName = PDEColorSpaceGetName(Space);

    std::ostringstream oss;

    CColorSpaces::ColorSelect &ColorInfo = sColorSpaces.FindColor(SpaceName);

    switch (ColorInfo.m_group) {
    case CColorSpaces::Unknown_cs:
        oss << "This is not a valid color space!";
        break;

    case CColorSpaces::Gray_cs:
        if (Color->value.color[0] == 0) {
            oss << " Gray - Black";
            break;
        }
        if (Color->value.color[0] == fixedOne) {
            oss << " Gray - White";
            break;
        }
        ASFixedToCString((fixedOne - Color->value.color[0]) * 100, Amount[0], 20, 5);
        oss << " " << Amount[0] << "% Gray ";
        break;

    case CColorSpaces::RGB_cs:
        if ((Color->value.color[0] == Color->value.color[1]) ||
            (Color->value.color[1] == Color->value.color[2])) {
            // This is a gray color in RGB
            if (Color->value.color[0] == 0) {
                oss << " RGB - Black";
                break;
            }
            if (Color->value.color[0] == fixedOne) {
                oss << " RGB - White";
                break;
            }

            ASFixedToCString((fixedOne - Color->value.color[0]) * 100, Amount[0], 20, 5);
            oss << " " << Amount[0] << "% Gray (In RGB) ";
            break;
        }

        ASFixedToCString(Color->value.color[0], Amount[0], 20, 5);
        ASFixedToCString(Color->value.color[1], Amount[1], 20, 5);
        ASFixedToCString(Color->value.color[2], Amount[2], 20, 5);
        oss << " RGB (" << Amount[0] << ", " << Amount[1] << ", " << Amount[2] << ")";
        break;

    case CColorSpaces::CMYK_cs:
        if ((Color->value.color[0] == Color->value.color[1]) ||
            (Color->value.color[1] == Color->value.color[2]) ||
            (Color->value.color[2] == Color->value.color[3])) {
            // This is a gray in CMYK
            if ((Color->value.color[0] == 0) && (Color->value.color[3] == 0)) {
                oss << " CMYK - White";
                break;
            }
            if ((Color->value.color[0] == 0) && (Color->value.color[3] == fixedOne)) {
                oss << " CMYK - True Black";
                break;
            }
            if (Color->value.color[0] == fixedOne) {
                oss << " CMYK - Black";
                break;
            }

            if (Color->value.color[0] == 0) {
                ASFixedToCString((fixedOne - Color->value.color[3]) * 100, Amount[0], 20, 5);
                oss << " " << Amount[0] << "% Gray (In CMYK Black) ";
                break;
            }

            if (Color->value.color[3] == 0) {
                ASFixedToCString((fixedOne - Color->value.color[0]) * 100, Amount[0], 20, 5);
                oss << " " << Amount[0] << "% Gray (In CMYK Colors) ";
                break;
            }
        }

        ASFixedToCString(Color->value.color[0], Amount[0], 20, 5);
        ASFixedToCString(Color->value.color[1], Amount[1], 20, 5);
        ASFixedToCString(Color->value.color[2], Amount[2], 20, 5);
        ASFixedToCString(Color->value.color[3], Amount[3], 20, 5);
        oss << " CMYK (" << Amount[0] << ", " << Amount[1] << ", " << Amount[2] << ", " << Amount[3] << ")";
        break;

    case CColorSpaces::Index_cs: {
        ASInt32 IndexRange = PDEColorSpaceGetHiVal(Space);
        ASUns8 *ColorTable;
        ASInt32 Comps = PDEColorSpaceGetBaseNumComps(Space);
        ASUns8 *ColorBase;
        ASUns8 Index;

        CColorSpaces::ColorSelect &BaseColorInfo = sColorSpaces.FindColor(PDEColorSpaceGetBase(Space));
        oss << " Indexed Color space of " << IndexRange + 1 << " Value in "
            << BaseColorInfo.m_name.c_str() << ". Value " << Color->value.color[0] << " (";

        ColorTable = (ASUns8 *)ASmalloc(Comps * (IndexRange + 1));
        PDEColorSpaceGetCTable(Space, ColorTable);
        ColorBase = &ColorTable[static_cast<int>(ASFixedToFloat(Color->value.color[0])) * Comps];
        for (Index = 0; Index < Comps; Index++) {
            oss << ColorBase[Index];
            if (Index + 1 < Comps)
                oss << " ";
        }
        oss << ")";
        ASfree(ColorTable);
        break;
    }

    case CColorSpaces::CalGray_cs:
        /* TBD Cal gray color space */
        break;

    case CColorSpaces::CalRGB_cs:
        /* TBD Cal RGB Color space */
        break;

    case CColorSpaces::LAB_cs:
        /* TBD LAB Color space */
        break;

    case CColorSpaces::ICC_cs:
        /* TBD ICC Color space */
        break;

    case CColorSpaces::Pattern_cs:
        /* TBD Pattern color spaces (Tiles and gradients) */
        break;

    case CColorSpaces::Separation_cs:
        /* TBD Separation color spaces */
        break;

    case CColorSpaces::DeviceN_cs:
        /* TBD DeviceN Color Spaces */
        break;
    }
    return oss.str();
}
