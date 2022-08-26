//
// Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, see:
// http://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/license-for-downloaded-pdf-samples/
//
// CountColorsInDoc.h
//

#include <vector>
#include <algorithm>

// Contains data accumulated when analyzing a document or page,
//    and will also be used to save color model -specific information
typedef struct colorsUsed {
    ASBool hadGray;
    ASBool hadRGB;
    ASBool hadCMYK;
    ASBool hadLab;
    ASBool hadICC;
    ASBool hadDeviceN;
    ASBool hadSep;
    ASBool hadIndex;
    ASBool hadPattern;
    ASBool hadShading;
    ASBool hadCalibrated;
    ASBool hadUncalibrated;
    ASBool hadNotGray;

    colorsUsed(bool Gray = false, bool RGB = false, bool CMYK = false, bool Lab = false, bool ICC = false,
               bool DeviceN = false, bool Sep = false, bool Index = false, bool Pattern = false,
               bool Shading = false, bool Calibrated = false, bool Uncalibrated = false, bool NotGray = false)
        : hadGray(Gray), hadRGB(RGB), hadCMYK(CMYK), hadLab(Lab), hadICC(ICC), hadDeviceN(DeviceN),
          hadSep(Sep), hadIndex(Index), hadPattern(Pattern), hadShading(Shading),
          hadCalibrated(Calibrated), hadUncalibrated(Uncalibrated), hadNotGray(NotGray){};

    // Overload to allow (logical) combination, for example to tally a page's information into a document
    colorsUsed &operator|=(const colorsUsed &rhs) {
        hadGray |= rhs.hadGray;
        hadRGB |= rhs.hadRGB;
        hadCMYK |= rhs.hadCMYK;
        hadLab |= rhs.hadLab;
        hadICC |= rhs.hadICC;
        hadDeviceN |= rhs.hadDeviceN;
        hadSep |= rhs.hadSep;
        hadIndex |= rhs.hadIndex;
        hadPattern |= rhs.hadPattern;
        hadShading |= rhs.hadShading;
        hadCalibrated |= rhs.hadCalibrated;
        hadUncalibrated |= rhs.hadUncalibrated;
        hadNotGray |= rhs.hadNotGray;
        return *this;
    }

    friend std::ostream &operator<<(std::ostream &ofs, const colorsUsed &cu) {
        ofs << (cu.hadNotGray ? "HAD" : "had NO") << " color" << std::endl;
        ofs << "\tHad DeviceGray Color:    " << (cu.hadGray & cu.hadUncalibrated ? "True" : "false")
            << std::endl;
        ofs << "\tHad DeviceRGB Color:     " << (cu.hadRGB & cu.hadUncalibrated ? "True" : "false")
            << std::endl;
        ;
        ofs << "\tHad DeviceCMYK Color:    " << (cu.hadCMYK & cu.hadUncalibrated ? "True" : "false")
            << std::endl;
        ;
        ofs << "\tHad CalGray Color:       " << (cu.hadGray & cu.hadCalibrated ? "True" : "false") << std::endl;
        ;
        ofs << "\tHad CalRGB Color:        " << (cu.hadRGB & cu.hadCalibrated ? "True" : "false") << std::endl;
        ;
        ofs << "\tHad Lab Color:           " << (cu.hadLab ? "True" : "false") << std::endl;
        ;
        ofs << "\tHad ICC Color:           " << (cu.hadICC ? "True" : "false") << std::endl;
        ;
        ofs << "\tHad DeviceN Color:       " << (cu.hadDeviceN ? "True" : "false") << std::endl;
        ;
        ofs << "\tHad Separation Color:    " << (cu.hadSep ? "True" : "false") << std::endl;
        ;
        ofs << "\tHad Indexed Color:       " << (cu.hadIndex ? "True" : "false") << std::endl;
        ;
        ofs << "\tHad Pattern Color:       " << (cu.hadPattern ? "True" : "false") << std::endl;
        ;
        ofs << "\tHad Shading Color:       " << (cu.hadShading ? "True" : "false") << std::endl
            << std::endl;
        return ofs;
    }
} ColorsUsed;

typedef struct colormodel {
    ASAtom name;
    std::string nameStr;
    ASBool neverColored;
    ASBool additive;
    ColorsUsed mark;

    colormodel(const char *_name, bool _neverColored, bool _additive, ColorsUsed _colorsUsed)
        : nameStr(_name), neverColored(_neverColored), additive(_additive), mark(_colorsUsed) {
        name = ASAtomFromString(nameStr.c_str());
    }

    colormodel() : name(0u), neverColored(false), additive(false) {}
} ColorModel;

struct MyComp {
    explicit MyComp(ASAtom a) : m_a(a) {}
    inline bool operator()(const ColorModel &cm) const { return cm.name == m_a; }

  private:
    ASAtom m_a;
};

class ColorModels {
  public:
    ColorModels() {
        m_models.push_back(ColorModel("DeviceGray", true, false,
                                      ColorsUsed(true, false, false, false, false, false, false,
                                                 false, false, false, false, true, false)));
        m_models.push_back(ColorModel("DeviceRGB", false, false,
                                      ColorsUsed(false, true, false, false, false, false, false,
                                                 false, false, false, false, true, false)));
        m_models.push_back(ColorModel("DeviceCMYK", false, true,
                                      ColorsUsed(false, false, true, false, false, false, false,
                                                 false, false, false, false, true, false)));
        m_models.push_back(ColorModel("CalGray", true, false,
                                      ColorsUsed(true, false, false, false, false, false, false,
                                                 false, false, false, true, false, false)));
        m_models.push_back(ColorModel("CalRGB", false, false,
                                      ColorsUsed(false, true, false, false, false, false, false,
                                                 false, false, false, true, false, false)));
        m_models.push_back(ColorModel("Lab", false, false,
                                      ColorsUsed(false, false, false, true, false, false, false,
                                                 false, false, false, true, false, false)));
        m_models.push_back(ColorModel("ICCBased", false, true,
                                      ColorsUsed(false, false, false, false, true, false, false,
                                                 false, false, false, true, false, false)));
        m_models.push_back(ColorModel("DeviceN", false, true,
                                      ColorsUsed(false, false, false, false, false, true, false,
                                                 false, false, false, false, false, false)));
        m_models.push_back(ColorModel("Separation", false, true,
                                      ColorsUsed(false, false, false, false, false, false, true,
                                                 false, false, false, false, false, false)));
    }
    const ColorModel &FindModel(ASAtom name) {
        std::vector<ColorModel>::const_iterator it =
            std::find_if(m_models.begin(), m_models.end(), MyComp(name));
        return (m_models.end() == it ? m_notfound : *it);
    }

  private:
    std::vector<ColorModel> m_models;
    ColorModel m_notfound;
};

void WalkPDETree(PDEContent content, ColorsUsed *colors);
void MarkColors(PDEColorSpec *color, ColorsUsed *colors);
