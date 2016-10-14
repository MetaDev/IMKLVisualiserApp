using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class TempPropertyGetter
{

    /// <summary>
    /// Set properties to IMKL-Objects.
    /// </summary>
    /// <param name="_XmlDocElement">Xml document elements.</param>
    /// <returns><b>List string[]</b> A 2 string array containing: PropertyName PropertyValue.</returns>
    public static List<string[]> VNS_SetProperties(XElement _XmlDocElement)
    {
        var _lstStringArray = new List<string[]>();
        //Loop through each property
        if (_XmlDocElement.HasElements == true)
        {
            foreach (var dom4 in _XmlDocElement.Elements())
            {
                if (dom4.Name.LocalName == "liggingNauwkeurigheid") //OK
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "href"), "/") });
                }
                else if (dom4.Name.LocalName == "heeftExtraInformatie")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "href"), ":") });
                }
                else if (dom4.Name.LocalName == "opLeidingElementen")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "href"), ":") });
                }
                else if (dom4.Name.LocalName == "containerType")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "href"), "/") });
                }
                else if (dom4.Name.LocalName == "subThema") //OK
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetStringAfterLast(VNS_GetStringBeforeLast(VNS_GetAttVal(dom4, "href"), "SubThema"), "/") });
                }
                else if (dom4.Name.LocalName == "orientatie")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, dom4.Value });
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName + "Unit", VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "uom"), ":") });
                }
                else if (dom4.Name.LocalName == "isRisicovol")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, dom4.Value });
                }
                else if (dom4.Name.LocalName == "isBovengrondsZichtbaar")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, dom4.Value });
                }
                else if (dom4.Name.LocalName == "voorzorgsmaatregel")
                {
                    foreach (var dom5 in dom4.Elements())
                    {
                        if (dom5.Name.LocalName == "Voorzorgsmaatregel")
                        {
                            foreach (var dom6 in dom5.Elements())
                            {
                                if (dom6.Name.LocalName == "bestandLocatie")
                                {
                                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, dom6.Value });
                                }
                            }
                        }
                    }
                }
                else if (dom4.Name.LocalName == "kleur")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, dom4.Value });
                }
                else if (dom4.Name.LocalName == "standaardDekking")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "href"), ":") });
                }
                else if (dom4.Name.LocalName == "dekking")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetStringBeforeLast(VNS_GetStringAfterFirst(VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "href"), "/"), ":"), ":") });
                }
                else if (dom4.Name.LocalName == "diepte")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "href"), ":") });
                }
                else if (dom4.Name.LocalName == "heeftExtraTopografieen")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetAttVal(dom4, "href") });
                }
                else if (dom4.Name.LocalName == "heeftDieptes")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "href"), ":") });
                }
                else if (dom4.Name.LocalName == "inNetwork")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetStringBeforeLast(VNS_GetStringAfterFirst(VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "href"), "/"), ":"), ":") });
                }
                else if (dom4.Name.LocalName == "heeftKabelOfLeiding")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "href"), ":") });
                }
                else if (dom4.Name.LocalName == "heeftKabelEnLeidingContainer")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetAttVal(dom4, "href") });
                }
                else if (dom4.Name.LocalName == "heeftLeidingElement")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "href"), ":") });
                }
                else if (dom4.Name.LocalName == "diepteNauwkeurigheid")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "href"), "/") });

                }
                else if (dom4.Name.LocalName == "dieptePeil")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, dom4.Value });
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName + "Unit", VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "uom"), ":") });
                }
                else if (dom4.Name.LocalName == "maaiveldPeil")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, dom4.Value });
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName + "Unit", VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "uom"), ":") });
                }
                else if (dom4.Name.LocalName == "heeftUtilityNetwork")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "href"), "/") });
                }
                else if (dom4.Name.LocalName == "imklId")
                {
                    foreach (var dom5 in dom4.Elements())
                    {
                        if (dom5.Name.LocalName == "Identifier" && dom5.HasElements == true)
                        {
                            foreach (var dom6 in dom5.Elements())
                            {
                                if (dom6.Name.LocalName == "localId")
                                    _lstStringArray.Add(new string[] { dom6.Name.LocalName, dom6.Value });
                                else if (dom6.Name.LocalName == "namespace")
                                    _lstStringArray.Add(new string[] { dom6.Name.LocalName, dom6.Value });
                            }
                        }
                    }
                }

                else if (dom4.Name.LocalName == "beginLifespanVersion")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, dom4.Value });
                }
                else if (dom4.Name.LocalName == "endLifespanVersion")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetAttVal(dom4, "nilReason") });
                }
                else if (dom4.Name.LocalName == "label")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, dom4.Value });
                }
                else if (dom4.Name.LocalName == "omschrijving")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, dom4.Value });
                }
                else if (dom4.Name.LocalName == "taal")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "href"), "/") });
                }
                else if (dom4.Name.LocalName == "annotatieType")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "href"), "/") });
                }
                else if (dom4.Name.LocalName == "rotatiehoek")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, dom4.Value });
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName + "Unit", VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "uom"), ":") });
                }
                else if (dom4.Name.LocalName == "extraPlanType")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "href"), "/") });
                }
                else if (dom4.Name.LocalName == "bestandLocatie")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, dom4.Value });
                }



                //else if (dom4.Name.LocalName == "bestandMediaType")
                //{
                //    _VNS_NS.bestandMediaType = VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "href"), "/");
                //}
                else if (dom4.Name.LocalName == "adres" && dom4.HasElements == true)
                {
                    foreach (var dom5 in dom4.Elements())
                    {
                        if (dom5.Name.LocalName == "Adres" && dom5.HasElements == true)
                        {
                            foreach (var dom6 in dom5.Elements())
                            {
                                if (dom6.Name.LocalName == "gemeente")
                                    _lstStringArray.Add(new string[] { dom4.Name.LocalName + " " + dom6.Name.LocalName, dom6.Value });
                                else if (dom6.Name.LocalName == "postcode")
                                    _lstStringArray.Add(new string[] { dom4.Name.LocalName + " " + dom6.Name.LocalName, dom6.Value });
                                else if (dom6.Name.LocalName == "straatnaam")
                                    _lstStringArray.Add(new string[] { dom4.Name.LocalName + " " + dom6.Name.LocalName, dom6.Value });
                                else if (dom6.Name.LocalName == "huisnummer")
                                    _lstStringArray.Add(new string[] { dom4.Name.LocalName + " " + dom6.Name.LocalName, dom6.Value });
                            }
                        }
                    }
                }
                else if (dom4.Name.LocalName == "extraTopografieType")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "href"), "/") });
                }
                else if (dom4.Name.LocalName == "technischeSpecificaties" && dom4.HasElements)
                {
                    foreach (var dom5 in dom4.Elements())
                    {
                        if (dom5.Name.LocalName == "CharacterString")
                        {
                            _lstStringArray.Add(new string[] { dom4.Name.LocalName, dom4.Value });
                        }
                    }
                }
                else if (dom4.Name.LocalName == "kabelDiameter")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, dom4.Value });
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName + "Unit", VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "uom"), ":") });
                }
                else if (dom4.Name.LocalName == "hoogte")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, dom4.Value });
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName + "Unit", VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "uom"), ":") });
                }
                else if (dom4.Name.LocalName == "technischContactpersoon" && dom4.HasElements == true)
                {
                    foreach (var dom5 in dom4.Elements())
                    {
                        if (dom5.Name.LocalName == "TechnischContactpersoon" && dom5.HasElements == true)
                        {
                            foreach (var dom6 in dom5.Elements())
                            {
                                if (dom6.Name.LocalName == "naam")
                                    _lstStringArray.Add(new string[] { dom4.Name.LocalName + " " + dom6.Name.LocalName, dom6.Value });
                                else if (dom6.Name.LocalName == "telefoon")
                                    _lstStringArray.Add(new string[] { dom4.Name.LocalName + " " + dom6.Name.LocalName, dom6.Value });
                                else if (dom6.Name.LocalName == "email")
                                    _lstStringArray.Add(new string[] { dom4.Name.LocalName + " " + dom6.Name.LocalName, dom6.Value });
                            }
                        }
                    }
                }
                else if (dom4.Name.LocalName == "operatingVoltage")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, dom4.Value });
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName + "Unit", VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "uom"), ":") });
                }
                else if (dom4.Name.LocalName == "nominalVoltage")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, dom4.Value });
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName + "Unit", VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "uom"), ":") });
                }
                else if (dom4.Name.LocalName == "telecommunicationsCableMaterialType")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "href"), "/") });
                }
                else if (dom4.Name.LocalName == "oilGasChemicalsProductType")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "href"), "/") });
                }
                else if (dom4.Name.LocalName == "sewerWaterType")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "href"), "/") });
                }
                else if (dom4.Name.LocalName == "waterType")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "href"), "/") });
                }
                else if (dom4.Name.LocalName == "thermalProductType")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "href"), "/") });
                }
                else if (dom4.Name.LocalName == "utilityDeliveryType") //OK
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "href"), "/") });
                }
                else if (dom4.Name.LocalName == "warningType")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "href"), "/") });
                }
                else if (dom4.Name.LocalName == "ductWidth")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, dom4.Value });
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName + "Unit", VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "uom"), ":") });
                }
                else if (dom4.Name.LocalName == "pipeDiameter")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, dom4.Value });
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName + "Unit", VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "uom"), ":") });
                }
                else if (dom4.Name.LocalName == "pressure")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, dom4.Value });
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName + "Unit", VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "uom"), ":") });
                }
                else if (dom4.Name.LocalName == "appurtenanceType")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "href"), "/") });
                }
                else if (dom4.Name.LocalName == "towerHeight")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, dom4.Value });
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName + "Unit", VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "uom"), ":") });
                }
                else if (dom4.Name.LocalName == "poleHeight")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, dom4.Value });
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName + "Unit", VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "uom"), ":") });
                }
                else if (dom4.Name.LocalName == "currentStatus") //OK
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "href"), "/") });
                }
                else if (dom4.Name.LocalName == "validFrom")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, dom4.Value });
                }
                else if (dom4.Name.LocalName == "validTo")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetAttVal(dom4, "nilReason") });
                }
                else if (dom4.Name.LocalName == "verticalPosition")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, dom4.Value });
                }
                else if (dom4.Name.LocalName == "utilityNetworkType")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "href"), "/") });
                }
                else if (dom4.Name.LocalName == "authorityRole")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetAttVal(dom4, "nilReason") });
                }
                else if (dom4.Name.LocalName == "disclaimer")
                {
                    foreach (var dom5 in dom4.Elements())
                    {
                        if (dom5.Name.LocalName == "PT_FreeText" && dom5.HasElements == true)
                        {
                            foreach (var dom6 in dom5.Elements())
                            {
                                if (dom6.Name.LocalName == "textGroup" && dom6.HasElements == true)
                                {
                                    foreach (var dom7 in dom6.Elements())
                                    {
                                        if (dom7.Name.LocalName == "LocalisedCharacterString")
                                            _lstStringArray.Add(new string[] { dom4.Name.LocalName, dom7.Value });
                                    }
                                }
                            }
                        }
                    }
                }
                else if (dom4.Name.LocalName == "fictitious")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, dom4.Value });
                }
                else if (dom4.Name.LocalName == "link") //OK
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, VNS_GetStringBeforeFirst(VNS_GetStringAfterFirst(VNS_GetStringAfterLast(VNS_GetAttVal(dom4, "href"), "/"), ":"), ":") });
                }
                else if (dom4.Name.LocalName == "name")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, dom4.Value });
                }
                else if (dom4.Name.LocalName == "function" && dom4.HasElements == true)
                {
                    foreach (var dom5 in dom4.Elements())
                    {
                        if (dom5.Name.LocalName == "Function" && dom5.HasElements == true)
                        {
                            foreach (var dom6 in dom5.Elements())
                            {
                                if (dom6.Name.LocalName == "activity" && dom6.HasElements == true)
                                {
                                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, dom6.Value });
                                }
                            }
                        }
                    }
                }
                else if (dom4.Name.LocalName == "description")
                {
                    _lstStringArray.Add(new string[] { dom4.Name.LocalName, dom4.Value });
                }
            }
        }
        return _lstStringArray;
    }

    /// <summary>
    /// Get part of string before the last found.
    /// </summary>
    /// <param name="inputStr">Input string where to search in.</param>
    /// <param name="fndStr">Search for this string in the input string</param>
    public static string VNS_GetStringBeforeLast(string inputStr, string fndStr)
    {
        //If input is null exit
        if (inputStr == null) return "";
        //Find last Position of given Char 
        int CharPos = inputStr.LastIndexOf(fndStr);

        //If not found
        if (CharPos == -1)
            return inputStr;
        else
            CharPos = CharPos + 1;

        //remove last occerence
        if (CharPos != 0)
            inputStr = inputStr.Substring(0, CharPos - 1);
        return inputStr;
    }
    /// <summary>
    /// Get part of string after the last found.
    /// </summary>
    /// <param name="inputStr">Input string where to search in.</param>
    /// <param name="fndStr">Search for this string in the input string</param>
    public static string VNS_GetStringAfterLast(string inputStr, string fndStr)
    {
        //If input is null exit
        if (inputStr == null) return "";
        //Find last Position of given Char 
        int CharPos = inputStr.LastIndexOf(fndStr);

        //If not found
        if (CharPos == -1)
            return inputStr;
        else
            CharPos = CharPos + fndStr.Length;

        //remove last occerence
        if (CharPos != 0)
            inputStr = inputStr.Substring(CharPos);
        return inputStr;
    }
    /// <summary>
    /// Get part of string before the first found.
    /// </summary>
    /// <param name="inputStr">Input string where to search in.</param>
    /// <param name="fndStr">Search for this string in the input string</param>
    public static string VNS_GetStringBeforeFirst(string inputStr, string fndStr)
    {
        //If input is null exit
        if (inputStr == null) return "";
        //Find last Position of given Char 
        int CharPos = inputStr.IndexOf(fndStr);

        //If not found
        if (CharPos == -1)
            return inputStr;
        else
            CharPos = CharPos + 1;

        //remove last occerence
        if (CharPos != 0)
            inputStr = inputStr.Substring(0, CharPos - 1);
        return inputStr;
    }
    /// <summary>
    /// Get part of string after the first found.
    /// </summary>
    /// <param name="inputStr">Input string where to search in.</param>
    /// <param name="fndStr">Search for this string in the input string</param>
    public static string VNS_GetStringAfterFirst(string inputStr, string fndStr)
    {
        //If input is null exit
        if (inputStr == null) return "";
        //Find last Position of given Char 
        int CharPos = inputStr.IndexOf(fndStr);

        //If not found
        if (CharPos == -1)
            return inputStr;
        else
            CharPos = CharPos + fndStr.Length;

        //remove last occerence
        if (CharPos != 0)
            inputStr = inputStr.Substring(CharPos);
        return inputStr;
    }
    /// <summary>
    /// Get attribute value from XElement.
    /// </summary>
    /// <param name="_XmlDocElement">Xml document element.</param>
    /// <param name="AttName">Attribute name.</param>
    public static string VNS_GetAttVal(XElement _XmlDocElement, string AttName)
    {
        if (_XmlDocElement.HasAttributes == true)
        {
            foreach (var dom5 in _XmlDocElement.Attributes())
            {
                string temp = dom5.Name.LocalName;
                if (dom5.Name.LocalName == AttName)
                {
                    return dom5.Value;
                }
            }
            return null;
        }
        else
            return null;
    }
}
