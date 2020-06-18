﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace VahtiApp
{
    internal class TarjousPalvelu : Palvelut
    {
        //https://tarjouspalvelu.fi/Default/Index
        //Special caset
        //https://tarjouspalvelu.fi/tarjouspyynnot.aspx?p=279&g=4255edad-e038-4620-ba0d-6c62a78a5cb8
        //https://tarjouspalvelu.fi/tarjouspyynnot.aspx?p=1701&g=9b94cbe2-fe1c-4765-a3b7-2a2e8ed680de
        public TarjousPalvelu()
        {
            uriBuilder = new UriBuilder();
            uriBuilder.Scheme = "https";
            uriBuilder.Host = "tarjouspalvelu.fi";
            uriBuilder.Path = "Default/Index";
            uri = uriBuilder.Uri;
            strSivuTiedosto = "TPSivut.txt";
        }
        public TarjousPalvelu(string inHost, string inPath)
        {
        }
        /// <summary>
        /// https://tarjouspalvelu.fi/tarjouspyynnot.aspx?p=279&g=4255edad-e038-4620-ba0d-6c62a78a5cb8
        /// Hanki: 279 & PPY:1701 https://tarjouspalvelu.fi/tarjouspyynnot.aspx?p=1701&g=9b94cbe2-fe1c-4765-a3b7-2a2e8ed680de
        /// g= 8 hex - 4hex - 4hex - 4hex - 12hex
        /// </summary>
        /// <returns></returns>
        internal override bool PuraAlaSivut()
        {
            if (lstrAlasivut.Count == 0) return false;
            lstrAlasivut.Add("279");//Hankisivu not in frontpage
            bool bOk = ListastaÄäkösetPois();
            foreach (string strIterSivu in lstrAlasivut)
            {
                Console.WriteLine($"Käsitellään maakuntaa {strIterSivu}");
                uriBuilder.Path = $"/tarjouspyynnot.aspx?p={strIterSivu}&g=4255edad-e038-4620-ba0d-6c62a78a5cb8";
                uri = uriBuilder.Uri;
                bOk = false;
                /* Not work to javascript pages
                bOk = GetWebPage();
                bOk = EtusivuWebClient();
                bOk = EtusivuWebClientDLS();
                bOk = EtusivuRestRequest();
                */
                //WebBrowser WbBrowsr = new WebBrowser();
                //WbBrowsr.Navigate(uri);
                //WbBrowsr.Navigated
                //strEtusivu=WbBrowsr.Document.Body.InnerHtml;
                if (!bOk)
                {
                    Console.WriteLine("GetWebPage false;");
                    return false;
                }
                bOk = PuraTarjousSivut();
                if (!bOk)
                {
                    Console.WriteLine("PuraTarjousSivut false;");
                    return false;
                }


            }
            return true;

        }

        ///<table border="0" cellpadding="5" cellspacing="10" width="100%" style="font-family:Verdana;font-size:11pt;">
        ///<tr><td style = "border:1px dotted black;text-align:center;width:33%;" >
        ///        < a href="https://tarjouspalvelu.fi/2m" target="_self">
        ///        <img src = "/Default/Image/1632" alt="" border="0" />
        ///        <p style = "font-weight:bold;text-align:center;" > 2M - IT Oy</p>
        ///        </a>
        ///     </td>
        ///     ...
        /// </tr>
        /// </table> => 
        /// https://tarjouspalvelu.fi/default.aspx?p=1632&g=ed371efb-e367-4944-9801-29ffa61e089d
        /// https://tarjouspalvelu.fi/tarjouspyynnot.aspx?p=1632&g=ed371efb-e367-4944-9801-29ffa61e089d
        /// We are intrest only number of image source
        internal override bool PuraEtusivu()
        {
            string strOtsikko = "</table>";
            int iOtsikkoPit = strOtsikko.Length;
            int iTauluPaikka = strEtusivu.IndexOf("<table");
            if (-1 == iTauluPaikka)
                return false;
            //...<map id="map" name="map" >...
            strEtusivu = strEtusivu.Remove(0, iTauluPaikka);
            //name="map" >... => find first </map>" 
            iTauluPaikka = strEtusivu.IndexOf(strOtsikko) + strOtsikko.Length;
            if (-1 == iTauluPaikka)
                return false;
            //remove all letters after </map>, starting with < 
            strEtusivu = strEtusivu.Remove(iTauluPaikka);
            List<List<string>> table = new List<List<string>>();
            table.Add(HtmlToList(strEtusivu));
            lstrAlasivut.AddRange(table[0]);
            return true;

        }
        /// <summary>
        /// Table to Html
        /// </summary>
        /// <param name="strHtml"></param>
        /// <returns></returns>
        internal override List<string> HtmlToList(string strHtml)
        {
            char[] charsToTrim = { '{', ' ', '}', '\n', '\r','\"' };
            List<string> lstRetVal = new List<string>();

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(strHtml);
            HtmlNode firstTable = doc.DocumentNode.SelectSingleNode("//table");

            if ("" == firstTable.Id)
            {
                var orderedCellTexts = firstTable.Descendants("tr")
                    .Select(row => row.SelectNodes("th|td").Take(3).ToArray())
                    .Select(cellArr => new { 
                        AD = cellArr[0].InnerHtml.Replace(',', ' ').Replace("&#228;", "ä").Replace("&#246;", "ö").Replace("&#196;", "Ä"),
                        BD = cellArr[1].InnerHtml.Replace(',', ' ').Replace("&#228;", "ä").Replace("&#246;", "ö").Replace("&#196;", "Ä"),
                        CD = cellArr[2].InnerHtml.Replace(',', ' ').Replace("&#228;", "ä").Replace("&#246;", "ö").Replace("&#196;", "Ä")
                    })
                    .ToList();
                foreach (var Cell in orderedCellTexts)
                {
                    
                    string[] strPalat= Cell.ToString().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    foreach(string strVal in strPalat)
                    {
                        if (-1 != strVal.IndexOf("Image/"))
                        {
                            string strTmp = strVal.Remove(strVal.IndexOf("alt=")).Remove(0, strVal.IndexOf("Image/")).Replace("Image/", " ").Trim(charsToTrim);
                            lstRetVal.Add(strTmp);
                        }
                    }

                }
            }
            else
            {
                var orderedCellTexts = firstTable.Descendants("tr")
                    .Select(row => row.SelectNodes("th|td").Take(4).ToArray())
                    .Select(cellArr => new
                    {
                        ID = cellArr[0].InnerHtml.Replace(',', ' ').Replace("&#228;", "ä").Replace("&#246;", "ö").Replace("&#196;", "Ä"),
                        TP = cellArr[1].InnerText.Replace(',', ' ').Replace("&#228;", "ä").Replace("&#246;", "ö").Replace("&#196;", "Ä"),
                        KU = cellArr[2].InnerText.Replace(',', ' ').Replace("&#228;", "ä").Replace("&#246;", "ö").Replace("&#196;", "Ä"),
                        MA = cellArr[3].InnerText.Replace(',', ' ')
                    })
                     .ToList();
            }

            return lstRetVal;

        }
        /// <summary>
        /// there are two pages:
        /// <table width = "100%" height="100%" cellspacing="0" cellpadding="0" border="0">
        ///  <tbody><tr>
        ///   <td><img src = "/Content/images/spacer.gif" width="20" height="1"></td>
        ///   ...
        ///   <td><a id = "ctl00_PageContent_AvaaHaku".... Siirry hakuun &gt;&gt;</span></a>
        ///   ...
        ///   </table> 
        ///   Remove all before Siirry hakuun >> text. Next remofe text before first </table>. 
        /// 1. No offers => 
        ///   No table any more.
        /// 2. Offerlists=>
        ///     <table id="DPSIlmoituslista"
        ///     <table id="ctl00_PageContent_GridView1"
        ///     <table> NO id
        ///     columns 5 or 6 
        ///     <th class="bold sortable-cell sorting" style="width: 11%; color: white; cursor: pointer;" rowspan="1" colspan="1">Yksikkö</th>
        ///     </tbody>    </table>
        ///     If Yksikkö in table six columns, ofter cases five.
        /// </summary>
        /// <returns></returns>
        internal override bool PuraTarjousSivut()
        {
            string strEtiEka = "Siirry hakuun >>";
            
            bool bOk = false;
            int iOnPaikalla = strEtusivu.IndexOf(strEtiEka);
            if (-1 == iOnPaikalla)
            {
                Console.WriteLine($"Virhe {strEtiEka}");
                return false;
            }
            //
            strEtusivu = strEtusivu.Remove(0, iOnPaikalla);
            iOnPaikalla = strEtusivu.LastIndexOf("</table>");
            if (-1 == iOnPaikalla)
            {
                Console.WriteLine($"Virhe </table>");
                return false;
            }
            strEtusivu = strEtusivu.Remove(iOnPaikalla + "</table>".Length);
            //first <table> ->
            iOnPaikalla = strEtusivu.IndexOf("<table");
            if (-1 == iOnPaikalla)
            {
                Console.WriteLine($"Virhe <table");
                return false;
            }
            strEtusivu = strEtusivu.Remove(0, iOnPaikalla);
            //last </table> next
            iOnPaikalla = strEtusivu.LastIndexOf("</table>");
            if (-1 == iOnPaikalla)
            {
                Console.WriteLine($"Virhe </table>");
                return false;
            }
            strEtusivu = strEtusivu.Remove(iOnPaikalla + "</table>".Length);
            List<List<string>> table = new List<List<string>>();
            String strKunta = string.Empty;
            char[] charsToTrim = { '{', ' ', '}', '\n', '\r' };
            while (true)
            {
                iOnPaikalla = strEtusivu.IndexOf("</table>");
                if (-1 == iOnPaikalla)
                {   //No more table ends
                    if (!bOk)
                    {
                        Console.WriteLine($"Virhe </table>");
                        return false;
                    }
                    else
                    {
                        //make new lines from table.
                        foreach (List<string> tyot in table)
                        {
                            if (tyot[0].Contains("KN"))
                            {
                                strKunta = tyot[0].Remove(0, tyot[0].IndexOf("=") + 1).Replace("&#228;", "ä").Replace("&nbsp;", " ").Trim(charsToTrim);
                            }
                            else
                            {
                                tyot.RemoveAt(0);
                                foreach (string strOsa in tyot)
                                {
                                    //tyot[0].Trim(charsToTrim);

                                    string[] palat = strOsa.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                                    Tarjous clTarjous = new Tarjous(strKunta);
                                    clTarjous.strAlkuperainenLinkki = palat[0].Remove(0, palat[0].LastIndexOf("=\"") + 3).Trim(charsToTrim);
                                    clTarjous.strAlkuperainenLinkki = clTarjous.strAlkuperainenLinkki.Remove(clTarjous.strAlkuperainenLinkki.IndexOf("\">") + 1);
                                    clTarjous.strTunnus = palat[0].Remove(0, palat[0].IndexOf(">") + 1).Replace("&nbsp;", " ").Replace("</a>", " ").Trim(charsToTrim);
                                    clTarjous.strPyynto = palat[1].Remove(0, palat[1].IndexOf("=") + 1).Replace("&nbsp;", " ").Trim(charsToTrim);
                                    clTarjous.strKuvaus = palat[2].Remove(0, palat[2].IndexOf("=") + 1).Replace("&nbsp;", " ").Trim(charsToTrim);
                                    clTarjous.strAika = palat[3].Remove(0, palat[3].IndexOf("=") + 1).Replace("&nbsp;", " ").Trim(charsToTrim);
                                    //Must check if current offer is already in list
                                    lstTajoukset.Add(clTarjous);
                                }
                            }
                        }
                        return true;
                    }
                }
                string apuSivu = strEtusivu.Substring(0, iOnPaikalla + "</table>".Length);
                strEtusivu = strEtusivu.Remove(0, iOnPaikalla + "</table>".Length);
                table.Add(HtmlToList(apuSivu));
                bOk = true;
            }
            //return false; 
        }
        internal override string Tallenne() { return strSivuTiedosto; }
    }

}